using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.MigrationTests
{
    [TestFixture]
    public class MigrateProviderMatchedLearnerDataServiceTests
    {
        private Mock<IProviderMigrationRepository> _providerMigrationRepositoryMock;
        private Mock<IMatchedLearnerRepository> _matchedLearnerRepositoryMock;
        private Mock<IMatchedLearnerDtoMapper> _matchedLearnerDtoMapperMock;
        private Mock<ILogger<MigrateProviderMatchedLearnerDataService>> _loggerMock;
        private TestableMessageHandlerContext _testableMessageHandlerContext;
        private MigrateProviderMatchedLearnerDataService _sut;

        private Guid _migrationRunId;
        private long _ukprn;

        private List<MigrationRunAttemptModel> _existingMigrationAttempts;
        private List<DataLockEventModel> _dataLockEventsForMigration;
        private List<TrainingModel> _mappedTrainingModels;

        private ApplicationSettings _applicationSettings;

        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();

            _migrationRunId = fixture.Create<Guid>();
            _ukprn = fixture.Create<long>();

            _existingMigrationAttempts = fixture.Create<List<MigrationRunAttemptModel>>();
            _existingMigrationAttempts.ForEach(x => x.Ukprn = _ukprn);

            _dataLockEventsForMigration = fixture.Create<List<DataLockEventModel>>();
            _mappedTrainingModels = fixture.Create<List<TrainingModel>>();

            _providerMigrationRepositoryMock = new Mock<IProviderMigrationRepository>();
            _providerMigrationRepositoryMock
                .Setup(x => x.GetProviderMigrationAttempts(_ukprn))
                .ReturnsAsync(_existingMigrationAttempts);

            _matchedLearnerRepositoryMock = new Mock<IMatchedLearnerRepository>();
            _matchedLearnerRepositoryMock
                .Setup(x => x.GetDataLockEventsForMigration(_ukprn))
                .ReturnsAsync(_dataLockEventsForMigration);
            _matchedLearnerRepositoryMock
                .Setup(x => x.GetApprenticeshipsForMigration(It.IsAny<List<long>>()))
                .ReturnsAsync((List<ApprenticeshipModel>)null);

            _matchedLearnerDtoMapperMock = new Mock<IMatchedLearnerDtoMapper>();
            _matchedLearnerDtoMapperMock
                .Setup(x => x.MapToModel(_dataLockEventsForMigration, null))
                .Returns(_mappedTrainingModels);

            _loggerMock = new Mock<ILogger<MigrateProviderMatchedLearnerDataService>>();

            _testableMessageHandlerContext = new TestableMessageHandlerContext();
            _applicationSettings = new ApplicationSettings { MigrationBatchSize = 2 };

            _sut = new MigrateProviderMatchedLearnerDataService(_applicationSettings, _providerMigrationRepositoryMock.Object, _matchedLearnerRepositoryMock.Object, _matchedLearnerDtoMapperMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndIsNewMigration_ThenCreatesNewMigrationAttempt()
        {
            //Arrange
            _existingMigrationAttempts.Clear();

            //Act
            await _sut.MigrateProviderScopedData(new MigrateProviderMatchedLearnerData { MigrationRunId = _migrationRunId, Ukprn = _ukprn }, _testableMessageHandlerContext);

            //Assert
            _providerMigrationRepositoryMock
                .Verify(x => x.CreateMigrationAttempt(It.IsAny<MigrationRunAttemptModel>()), Times.Once);
        }

        [TestCase(MigrationStatus.Failed)]
        [TestCase(MigrationStatus.InProgress)]
        public async Task WhenMigratingProviderScopedData_AndHasFailedOrInProgressMigrations_ThenCreatesNewMigrationAttempt(MigrationStatus status)
        {
            //Arrange
            _existingMigrationAttempts.ForEach(x => x.Status = status);

            //Act
            await _sut.MigrateProviderScopedData(new MigrateProviderMatchedLearnerData { MigrationRunId = _migrationRunId, Ukprn = _ukprn }, _testableMessageHandlerContext);

            //Assert
            _providerMigrationRepositoryMock
                .Verify(x => x.CreateMigrationAttempt(It.IsAny<MigrationRunAttemptModel>()), Times.Once);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_ThenMaps()
        {
            //Arrange
            _existingMigrationAttempts.ForEach(x => x.Status = MigrationStatus.InProgress);

            //Act
            await _sut.MigrateProviderScopedData(new MigrateProviderMatchedLearnerData { MigrationRunId = _migrationRunId, Ukprn = _ukprn }, _testableMessageHandlerContext);

            //Assert
            _providerMigrationRepositoryMock
                .Verify(x => x.CreateMigrationAttempt(It.IsAny<MigrationRunAttemptModel>()), Times.Once);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndIsNewMigration_ThenDoesBulkInsert()
        {
            //Arrange
            _existingMigrationAttempts.Clear();

            //Act
            await _sut.MigrateProviderScopedData(new MigrateProviderMatchedLearnerData { MigrationRunId = _migrationRunId, Ukprn = _ukprn }, _testableMessageHandlerContext);

            //Assert
            _matchedLearnerRepositoryMock.Verify(x => x.SaveTrainings(_mappedTrainingModels));
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndExceptionThrownWhileStoring_ThenTransactionRolledBack()
        {
            //Arrange
            _existingMigrationAttempts.Clear();
            _matchedLearnerRepositoryMock
                .Setup(x => x.SaveTrainings(_mappedTrainingModels))
                .ThrowsAsync(new InvalidOperationException());

            //Act
            await _sut.MigrateProviderScopedData(new MigrateProviderMatchedLearnerData { MigrationRunId = _migrationRunId, Ukprn = _ukprn }, _testableMessageHandlerContext);

            //Assert
            _matchedLearnerRepositoryMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndExceptionThrownWhileStoring_ThenTrainingItemsAreSavedIndividually()
        {
            //Arrange
            _existingMigrationAttempts.Clear();
            _matchedLearnerRepositoryMock
                .Setup(x => x.SaveTrainings(_mappedTrainingModels))
                .ThrowsAsync(new InvalidOperationException());

            //Act
            await _sut.MigrateProviderScopedData(new MigrateProviderMatchedLearnerData { MigrationRunId = _migrationRunId, Ukprn = _ukprn }, _testableMessageHandlerContext);

            //Assert
            _matchedLearnerRepositoryMock.Verify(x => x.SaveTrainingsIndividually(It.IsAny<List<TrainingModel>>()), Times.Once);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_ThenUpdatesMigrationStatusCorrectly()
        {
            //Arrange
            _existingMigrationAttempts.Clear();

            //Act
            await _sut.MigrateProviderScopedData(new MigrateProviderMatchedLearnerData { MigrationRunId = _migrationRunId, Ukprn = _ukprn }, _testableMessageHandlerContext);

            //Assert
            _providerMigrationRepositoryMock.Verify(x => x.UpdateMigrationRunAttemptStatus(It.Is<MigrationRunAttemptModel>(m => m.Ukprn == _ukprn && m.MigrationRunId == _migrationRunId && m.BatchNumber == null), MigrationStatus.Completed), Times.Once);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndBothBulkAndIndividualSaveFails_ThenBatchesAreCreated()
        {
            //Arrange
            _existingMigrationAttempts.Clear();
            _matchedLearnerRepositoryMock
                .Setup(x => x.SaveTrainings(_mappedTrainingModels))
                .ThrowsAsync(new InvalidOperationException());
            _matchedLearnerRepositoryMock
                .Setup(x => x.SaveTrainingsIndividually(It.IsAny<List<TrainingModel>>()))
                .ThrowsAsync(new InvalidOperationException());

            _sut = new MigrateProviderMatchedLearnerDataService(_applicationSettings, _providerMigrationRepositoryMock.Object, _matchedLearnerRepositoryMock.Object, _matchedLearnerDtoMapperMock.Object, _loggerMock.Object);

            //Act
            await _sut.MigrateProviderScopedData(new MigrateProviderMatchedLearnerData { MigrationRunId = _migrationRunId, Ukprn = _ukprn }, _testableMessageHandlerContext);

            //Assert
            var sentMessages = _testableMessageHandlerContext.SentMessages.Select(x => (MigrateProviderMatchedLearnerData)x.Message).ToList();

            sentMessages.Count.Should().Be(2);

            sentMessages.Count(request =>
                request.TrainingData.Length == 2
                && request.Ukprn == _ukprn
                && request.BatchNumber == 0
                && request.TotalBatches == 2
                && request.MigrationRunId == _migrationRunId
            ).Should().Be(1);

            sentMessages.Count(request =>
                request.TrainingData.Length == 1
                && request.Ukprn == _ukprn
                && request.BatchNumber == 1
                && request.TotalBatches == 2
                && request.MigrationRunId == _migrationRunId
            ).Should().Be(1);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndBothBulkAndIndividualSaveFails_ThenFirstRunIsSetToCompletedWithErrors()
        {
            //Arrange
            _existingMigrationAttempts.Clear();
            _matchedLearnerRepositoryMock
                .Setup(x => x.SaveTrainings(_mappedTrainingModels))
                .ThrowsAsync(new InvalidOperationException());
            _matchedLearnerRepositoryMock
                .Setup(x => x.SaveTrainingsIndividually(It.IsAny<List<TrainingModel>>()))
                .ThrowsAsync(new InvalidOperationException());

            _sut = new MigrateProviderMatchedLearnerDataService(_applicationSettings, _providerMigrationRepositoryMock.Object, _matchedLearnerRepositoryMock.Object, _matchedLearnerDtoMapperMock.Object, _loggerMock.Object);

            //Act
            await _sut.MigrateProviderScopedData(new MigrateProviderMatchedLearnerData { MigrationRunId = _migrationRunId, Ukprn = _ukprn }, _testableMessageHandlerContext);

            //Assert
            _providerMigrationRepositoryMock.Verify(x => x.UpdateMigrationRunAttemptStatus(It.Is<MigrationRunAttemptModel>(m => m.Ukprn == _ukprn && m.MigrationRunId == _migrationRunId && m.BatchNumber == null), MigrationStatus.CompletedWithErrors), Times.Once);
        }
    }
}
