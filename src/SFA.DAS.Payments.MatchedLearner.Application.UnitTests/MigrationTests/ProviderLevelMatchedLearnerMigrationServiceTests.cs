using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.MigrationTests
{
    [TestFixture]
    public class ProviderLevelMatchedLearnerMigrationServiceTests
    {
        private Mock<IProviderMigrationRepository> _providerMigrationRepositoryMock;
        private Mock<IMatchedLearnerRepository> _matchedLearnerRepositoryMock;
        private Mock<IMatchedLearnerDtoMapper> _matchedLearnerDtoMapperMock;
        private Mock<ILogger<ProviderLevelMatchedLearnerMigrationService>> _loggerMock;
        private ProviderLevelMatchedLearnerMigrationService _sut;

        private Guid _migrationRunId;
        private long _ukprn;

        private List<MigrationRunAttemptModel> _existingMigrationAttempts;
        private List<DataLockEventModel> _dataLockEventsForMigration;
        private List<ApprenticeshipModel> _apprenticeshipsForMigration;
        private List<TrainingModel> _mappedTrainingModels;


        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();

            _migrationRunId = fixture.Create<Guid>();
            _ukprn = fixture.Create<long>();
            
            _existingMigrationAttempts = fixture.Create<List<MigrationRunAttemptModel>>();
            _existingMigrationAttempts.ForEach(x => x.Ukprn = _ukprn);

            _dataLockEventsForMigration = fixture.Create<List<DataLockEventModel>>();

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
                .ReturnsAsync(_apprenticeshipsForMigration);

            _matchedLearnerDtoMapperMock = new Mock<IMatchedLearnerDtoMapper>();
            _matchedLearnerDtoMapperMock
                .Setup(x => x.MapToModel(_dataLockEventsForMigration, _apprenticeshipsForMigration))
                .Returns(_mappedTrainingModels);

            _loggerMock = new Mock<ILogger<ProviderLevelMatchedLearnerMigrationService>>();

            _sut = new ProviderLevelMatchedLearnerMigrationService(_providerMigrationRepositoryMock.Object, _matchedLearnerRepositoryMock.Object, _matchedLearnerDtoMapperMock.Object, _loggerMock.Object, 0);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndMigrationIsCompleted_ThenReturns()
        {
            //Arrange
            _existingMigrationAttempts.ForEach(x => x.Status = MigrationStatus.Completed);

            //Act
            await _sut.MigrateProviderScopedData(_migrationRunId, _ukprn);

            //Assert
            _providerMigrationRepositoryMock
                .Verify(x => x.CreateMigrationAttempt(It.IsAny<MigrationRunAttemptModel>()), Times.Never);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndIsNewMigration_ThenCreatesNewMigrationAttempt()
        {
            //Arrange
            _existingMigrationAttempts.Clear();

            //Act
            await _sut.MigrateProviderScopedData(_migrationRunId, _ukprn);

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
            await _sut.MigrateProviderScopedData(_migrationRunId, _ukprn);

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
            await _sut.MigrateProviderScopedData(_migrationRunId, _ukprn);

            //Assert
            _providerMigrationRepositoryMock
                .Verify(x => x.CreateMigrationAttempt(It.IsAny<MigrationRunAttemptModel>()), Times.Once);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndMigrationFailedPreviously_ThenSavesIndividually()
        {
            //Arrange
            _existingMigrationAttempts.ForEach(x => x.Status = MigrationStatus.Failed);

            //Act
            await _sut.MigrateProviderScopedData(_migrationRunId, _ukprn);

            //Assert
            _matchedLearnerRepositoryMock.Verify(x => x.SaveTrainingsIndividually(_mappedTrainingModels, It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_AndIsNewMigration_ThenDoesBulkInsert()
        {
            //Arrange
            _existingMigrationAttempts.Clear();

            //Act
            await _sut.MigrateProviderScopedData(_migrationRunId, _ukprn);

            //Assert
            _matchedLearnerRepositoryMock.Verify(x => x.StoreSubmissionsData(_mappedTrainingModels, It.IsAny<CancellationToken>()));
        }

        [Test]
        public void WhenMigratingProviderScopedData_AndExceptionThrownWhileStoring_ThenTransactionRolledBack()
        {
            //Arrange
            _existingMigrationAttempts.Clear();
            _matchedLearnerRepositoryMock
                .Setup(x => x.StoreSubmissionsData(_mappedTrainingModels, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            //Act
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.MigrateProviderScopedData(_migrationRunId, _ukprn));

            //Assert
            _matchedLearnerRepositoryMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _providerMigrationRepositoryMock.Verify(x => x.UpdateMigrationRunAttemptStatus(_ukprn, _migrationRunId, MigrationStatus.Failed), Times.Once);
        }

        [Test]
        public async Task WhenMigratingProviderScopedData_ThenUpdatesMigrationStatusCorrectly()
        {
            //Arrange
            _existingMigrationAttempts.Clear();

            //Act
            await _sut.MigrateProviderScopedData(_migrationRunId, _ukprn);

            //Assert
            _providerMigrationRepositoryMock.Verify(x => x.UpdateMigrationRunAttemptStatus(_ukprn, _migrationRunId, MigrationStatus.Completed), Times.Once);
        }
    }
}
