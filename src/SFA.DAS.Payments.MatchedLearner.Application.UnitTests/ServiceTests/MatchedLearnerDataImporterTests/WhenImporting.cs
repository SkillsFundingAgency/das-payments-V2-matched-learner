using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.ServiceTests.MatchedLearnerDataImporterTests
{
    [TestFixture]
    public class WhenImporting
    {
        private ImportMatchedLearnerData _importMatchedLearnerData;
        private Mock<IMatchedLearnerRepository> _mockMatchedLearnerRepository;
        private Mock<IPaymentsRepository> _mockPaymentsRepository;
        private Mock<IMatchedLearnerDataImportService> _mockMatchedLearnerDataImportService;
        private Mock<ILegacyMatchedLearnerDataImportService> _mockLegacyMatchedLearnerDataImportService;
        private MatchedLearnerDataImporter _sut;
        private List<DataLockEventModel> _dataLockEvents;
        private readonly Guid _dataLockEventId = Guid.NewGuid();
        private readonly Guid _nonPayableEventId1 = Guid.NewGuid();

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
            _mockMatchedLearnerDataImportService = new Mock<IMatchedLearnerDataImportService>();
            _mockLegacyMatchedLearnerDataImportService = new Mock<ILegacyMatchedLearnerDataImportService>();

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

            _mockPaymentsRepository.Setup(x => x.GetDataLockEvents(_importMatchedLearnerData))
                .ReturnsAsync(_dataLockEvents);

            _sut = new MatchedLearnerDataImporter(_mockMatchedLearnerRepository.Object, _mockPaymentsRepository.Object, _mockMatchedLearnerDataImportService.Object, _mockLegacyMatchedLearnerDataImportService.Object);

            await _sut.Import(_importMatchedLearnerData);
        }

        [Test]
        public void ThenCallsMatchedLearnerDataImportService()
        {
            _mockMatchedLearnerDataImportService.Verify(x => x.Import(_importMatchedLearnerData, 
                It.Is<List<DataLockEventModel>>(d => d.Count == 1 && d.First().EventId == _dataLockEventId)));
        }

        [Test]
        public void ThenCallsLegacyMatchedLearnerDataImportService()
        {
            _mockLegacyMatchedLearnerDataImportService.Verify(x => x.Import(_importMatchedLearnerData, 
                It.Is<List<DataLockEventModel>>(d => d.Count == 1 && d.First().EventId == _dataLockEventId)));
        }

        [Test]
        public void ThenGetsDataLockEventsFromPaymentsRepository()
        {
            _mockPaymentsRepository.Verify(x => x.GetDataLockEvents(_importMatchedLearnerData));
        }

        [Test]
        public void ThenSaveSubmissionJobToDb()
        {
            _mockMatchedLearnerRepository.Verify(x => x.SaveSubmissionJob(It.IsAny<SubmissionJobModel>()));
        }
    }
}
