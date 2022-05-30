﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.ServiceTests.MatchedLearnerDataImportServiceTests
{
    [TestFixture]
    public class WhenImporting
    {
        private ImportMatchedLearnerData _importMatchedLearnerData;
        private MatchedLearnerDataImportService _sut;
        private List<DataLockEventModel> _dataLockEvents;
        private List<ApprenticeshipModel> _apprenticeships;
        private readonly Guid _dataLockEventId = Guid.NewGuid();
        private readonly Guid _nonPayableEventId1 = Guid.NewGuid();

        private Mock<IMatchedLearnerRepository> _mockMatchedLearnerRepository;
        private Mock<IPaymentsRepository> _mockPaymentsRepository;
        private Mock<ILogger<MatchedLearnerDataImportService>> _mockLogger;

        [SetUp]
        public async Task SetUp()
        {
            _importMatchedLearnerData = new ImportMatchedLearnerData
            {
                Ukprn = 173658,
                CollectionPeriod = 5,
                AcademicYear = 2021,
                JobId = 123,
            };
            
            _mockMatchedLearnerRepository = new Mock<IMatchedLearnerRepository>();
            _mockPaymentsRepository = new Mock<IPaymentsRepository>();
            _mockLogger = new Mock<ILogger<MatchedLearnerDataImportService>>();

            _dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 1920,
                    CollectionPeriod = 14,
                    LearningAimPathwayCode = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    EventId = _dataLockEventId,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>
                    {
                        new DataLockEventPriceEpisodeModel
                        {
                            DataLockEventId = _dataLockEventId,
                            TotalNegotiatedPrice1 = 100m,
                            TotalNegotiatedPrice2 = 200m,
                            InstalmentAmount = 2m,
                            NumberOfInstalments = 5,
                            CompletionAmount = 1m,
                            PriceEpisodeIdentifier = "1-1-01/08/2020",
                        },
                        new DataLockEventPriceEpisodeModel
                        {
                            DataLockEventId = _dataLockEventId,
                            TotalNegotiatedPrice1 = 1000m,
                            TotalNegotiatedPrice2 = 2000m,
                            InstalmentAmount = 20m,
                            NumberOfInstalments = 50,
                            CompletionAmount = 10m,
                            PriceEpisodeIdentifier = "2-2-01/08/2020",
                        }
                    },
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
                    {
                        new DataLockEventNonPayablePeriodModel
                        {
                            DataLockEventId = _dataLockEventId,
                            DataLockEventNonPayablePeriodId = _nonPayableEventId1,
                            PriceEpisodeIdentifier = "1-1-01/08/2020",
                            DeliveryPeriod = 2,
                            TransactionType = 1,
                            Amount = 100,
                            Failures = new List<DataLockEventNonPayablePeriodFailureModel>
                            {
                                new DataLockEventNonPayablePeriodFailureModel
                                {
                                    DataLockEventNonPayablePeriodId = _nonPayableEventId1,
                                    ApprenticeshipId = 112,
                                    DataLockFailureId = 2,
                                },
                                new DataLockEventNonPayablePeriodFailureModel
                                {
                                    DataLockEventNonPayablePeriodId = _nonPayableEventId1,
                                    ApprenticeshipId = 112,
                                    DataLockFailureId = 3,
                                },
                            }
                        }
                    },
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>
                    {
                        new DataLockEventPayablePeriodModel
                        {
                            DataLockEventId = _dataLockEventId,
                            PriceEpisodeIdentifier = "2-2-01/08/2020",
                            DeliveryPeriod = 1,
                            TransactionType = 1,
                            Amount = 100,
                            ApprenticeshipId = 114,
                        },
                    }
                }

            };

            _apprenticeships = new List<ApprenticeshipModel>
            {
                new ApprenticeshipModel
                {
                    Id = 112,
                    Uln = 1000112,
                    AccountId = 123,
                    TransferSendingEmployerAccountId = 123,
                    ApprenticeshipEmployerType = 1,
                },
                new ApprenticeshipModel
                {
                    Id = 114,
                    Uln = 1000114,
                    AccountId = 456,
                    TransferSendingEmployerAccountId = 456,
                    ApprenticeshipEmployerType = 1,
                }
            };

            _mockPaymentsRepository.Setup(x => x.GetDataLockEvents(_importMatchedLearnerData))
                .ReturnsAsync(_dataLockEvents);

            _mockPaymentsRepository.Setup(x => x.GetApprenticeships(It.IsAny<List<long>>()))
                .ReturnsAsync(_apprenticeships);

            _sut = new MatchedLearnerDataImportService(_mockMatchedLearnerRepository.Object, _mockPaymentsRepository.Object, new MatchedLearnerDtoMapper(), _mockLogger.Object);

            await _sut.Import(_importMatchedLearnerData, _dataLockEvents);
        }

        [Test]
        public void ThenRemovesPreviousSubmissionDataForCurrentPeriod()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemovePreviousSubmissionsData(_importMatchedLearnerData.Ukprn, _importMatchedLearnerData.AcademicYear, It.Is<IList<byte>>(y => y.Contains(_importMatchedLearnerData.CollectionPeriod))));
        }

        [Test]
        public void ThenRemovesPreviousSubmissionDataForPreviousPeriod()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemovePreviousSubmissionsData(_importMatchedLearnerData.Ukprn, _importMatchedLearnerData.AcademicYear, It.Is<IList<byte>>(y => y.Contains(_importMatchedLearnerData.CollectionPeriod))));
        }

        [Test]
        public void ThenStoresDataLocks()
        {
            _mockMatchedLearnerRepository.Verify(x => x.SaveTrainings(
                It.Is<List<TrainingModel>>(y => y.Count == 1 && y.Any(z => z.EventId == _dataLockEventId))));
        }

        [Test]
        public void ThenStoresApprenticeshipDetails()
        {
            _mockMatchedLearnerRepository.Verify(x => x.SaveTrainings(
                It.Is<List<TrainingModel>>(y => y.Count == 1 && 
                                                y.SelectMany(z => z.PriceEpisodes)
                                                    .SelectMany(p => p.Periods)
                                                    .Select(a => a.ApprenticeshipId)
                                                    .Distinct().Count() == 2)));
        }
    }
}
