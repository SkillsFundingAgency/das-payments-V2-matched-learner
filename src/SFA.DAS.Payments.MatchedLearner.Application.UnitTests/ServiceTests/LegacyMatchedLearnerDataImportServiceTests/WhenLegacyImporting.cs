﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.ServiceTests.LegacyMatchedLearnerDataImportServiceTests
{
    [TestFixture]
    public class WhenLegacyImporting
    {
        private SubmissionJobSucceeded _submissionSucceededEvent;
        private LegacyMatchedLearnerDataImportService _sut;
        private List<DataLockEventModel> _dataLockEvents;
        private List<ApprenticeshipModel> _apprenticeships;
        private readonly Guid _dataLockEventId = Guid.NewGuid();

        private Mock<ILegacyMatchedLearnerRepository> _mockMatchedLearnerRepository;
        private Mock<IPaymentsRepository> _mockPaymentsRepository;
        private Mock<ILogger<LegacyMatchedLearnerDataImportService>> _mockLogger;

        [SetUp]
        public async Task SetUp()
        {
            _submissionSucceededEvent = new SubmissionJobSucceeded
            {
                Ukprn = 173658,
                CollectionPeriod = 5,
                AcademicYear = 2021,
                JobId = 123,
            };
            
            _mockMatchedLearnerRepository = new Mock<ILegacyMatchedLearnerRepository>();
            _mockPaymentsRepository = new Mock<IPaymentsRepository>();
            _mockLogger = new Mock<ILogger<LegacyMatchedLearnerDataImportService>>();

            _dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    EventId = _dataLockEventId,
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>
                    {
                        new DataLockEventPayablePeriodModel
                        {
                            ApprenticeshipId = 112
                        }
                    },
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
                    {
                        new DataLockEventNonPayablePeriodModel
                        {
                            Failures = new List<DataLockEventNonPayablePeriodFailureModel>
                            {
                                new DataLockEventNonPayablePeriodFailureModel
                                {
                                    ApprenticeshipId = 114
                                }
                            }
                        }
                    }
                }
            };

            _apprenticeships = new List<ApprenticeshipModel>
            {
                new ApprenticeshipModel
                {
                    Id = 112,
                    Uln = 1000112
                },
                new ApprenticeshipModel
                {
                    Id = 114,
                    Uln = 1000114
                }
            };

            _mockPaymentsRepository.Setup(x => x.GetDataLockEvents(_submissionSucceededEvent))
                .ReturnsAsync(_dataLockEvents);

            _mockPaymentsRepository.Setup(x => x.GetApprenticeships(It.IsAny<List<long>>()))
                .ReturnsAsync(_apprenticeships);

            _sut = new LegacyMatchedLearnerDataImportService(_mockMatchedLearnerRepository.Object, _mockPaymentsRepository.Object, _mockLogger.Object);

            await _sut.Import(_submissionSucceededEvent, _dataLockEvents);
        }

        [Test]
        public void ThenRemovesPreviousSubmissionDataForCurrentPeriod()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemovePreviousSubmissionsData(_submissionSucceededEvent.Ukprn, _submissionSucceededEvent.AcademicYear, It.Is<IList<byte>>(y => y.Contains(_submissionSucceededEvent.CollectionPeriod))));
        }

        [Test]
        public void ThenRemovesPreviousSubmissionDataForPreviousPeriod()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemovePreviousSubmissionsData(_submissionSucceededEvent.Ukprn, _submissionSucceededEvent.AcademicYear, It.Is<IList<byte>>(y => y.Contains(_submissionSucceededEvent.CollectionPeriod))));
        }

        [Test]
        public void ThenRemovesPreviousApprenticeshipData()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemoveApprenticeships(It.Is<List<long>>(
                y => y.Count == 2 
                     && y.Contains(112) 
                     && y.Contains(114))));
        }

        [Test]
        public void ThenStoresNewApprenticeshipData()
        {
            _mockMatchedLearnerRepository.Verify(x => x.StoreApprenticeships(It.Is<List<ApprenticeshipModel>>(
                y => y.Count == 2
                     && y.Any(z => z.Id == 112 && z.Uln == 1000112)
                     && y.Any(z => z.Id == 114 && z.Uln == 1000114)
            )));
        }

        [Test]
        public void ThenStoresDataLocks()
        {
            _mockMatchedLearnerRepository.Verify(x => x.SaveDataLockEvents(It.Is<List<DataLockEventModel>>(
                y => y.Count == 1
                     && y.Any(z => z.EventId == _dataLockEventId)
            )));
        }
    }
}