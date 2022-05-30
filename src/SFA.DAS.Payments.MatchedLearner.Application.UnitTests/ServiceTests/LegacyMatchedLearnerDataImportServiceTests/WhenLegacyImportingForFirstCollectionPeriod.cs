﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.ServiceTests.LegacyMatchedLearnerDataImportServiceTests
{
    [TestFixture]
    public class WhenLegacyImportingForFirstCollectionPeriod
    {
        private ImportMatchedLearnerData _importMatchedLearnerData;
        private LegacyMatchedLearnerDataImportService _sut;

        private Mock<ILegacyMatchedLearnerRepository> _mockMatchedLearnerRepository;
        private Mock<IPaymentsRepository> _mockPaymentsRepository;
        private Mock<ILogger<LegacyMatchedLearnerDataImportService>> _mockLogger;

        [SetUp]
        public async Task SetUp()
        {
            _importMatchedLearnerData = new ImportMatchedLearnerData
            {
                Ukprn = 173658,
                CollectionPeriod = 1,
                AcademicYear = 2021,
                JobId = 123,
            };
            
            _mockMatchedLearnerRepository = new Mock<ILegacyMatchedLearnerRepository>();
            _mockPaymentsRepository = new Mock<IPaymentsRepository>();
            _mockLogger = new Mock<ILogger<LegacyMatchedLearnerDataImportService>>();

            _mockPaymentsRepository.Setup(x => x.GetDataLockEvents(_importMatchedLearnerData))
                .ReturnsAsync(new List<DataLockEventModel>());

            _mockPaymentsRepository.Setup(x => x.GetApprenticeships(It.IsAny<List<long>>()))
                .ReturnsAsync(new List<ApprenticeshipModel>());

            _sut = new LegacyMatchedLearnerDataImportService(_mockMatchedLearnerRepository.Object, _mockPaymentsRepository.Object, _mockLogger.Object);

            await _sut.Import(_importMatchedLearnerData, new List<DataLockEventModel>());
        }

        [Test]
        public void ThenOnlyRemovesPreviousSubmissionDataForCurrentPeriod()
        {
            _mockMatchedLearnerRepository.Verify(x => x.RemovePreviousSubmissionsData(_importMatchedLearnerData.Ukprn, _importMatchedLearnerData.AcademicYear, It.Is<IList<byte>>(y => y.Count == 1 && y.Contains(_importMatchedLearnerData.CollectionPeriod))));
        }
    }
}