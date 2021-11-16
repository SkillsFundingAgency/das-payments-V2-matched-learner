using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.RepositoryTests.MatchedLearnerRepositoryTests
{
    [TestFixture]
    public class WhenGettingDataLockEventsAcrossMultipleAcademicYears
    {
        private IMatchedLearnerRepository _sut;
        private MatchedLearnerDataContext _dataDataContext;
        private DataLockEventModel _dataLockEventAy1;
        private DataLockEventPayablePeriodModel _dataLockEventPayablePeriodAy1;
        private DataLockEventPriceEpisodeModel _dataLockEventPriceEpisodeAy1;
        private DataLockEventModel _dataLockEventAy2;
        private DataLockEventPayablePeriodModel _dataLockEventPayablePeriodAy2;
        private DataLockEventPriceEpisodeModel _dataLockEventPriceEpisodeAy2;

        private long _ukprn, _uln;
        private short _academicYear1;
        private short _academicYear2;

        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();

            _ukprn = fixture.Create<long>();
            _uln = fixture.Create<long>();
            _academicYear1 = fixture.Create<short>();
            _academicYear2 = fixture.Create<short>();

            _dataLockEventAy1 = fixture.Create<DataLockEventModel>();
            _dataLockEventAy2 = fixture.Create<DataLockEventModel>();

            _dataLockEventPayablePeriodAy1 = fixture.Create<DataLockEventPayablePeriodModel>();
            _dataLockEventPayablePeriodAy2 = fixture.Create<DataLockEventPayablePeriodModel>();
            _dataLockEventPriceEpisodeAy1 = fixture.Create<DataLockEventPriceEpisodeModel>();
            _dataLockEventPriceEpisodeAy2 = fixture.Create<DataLockEventPriceEpisodeModel>();

            var bsContextOption = new DbContextOptionsBuilder<MatchedLearnerDataContext>()
                .UseInMemoryDatabase($"TestDb-{Guid.NewGuid()}", new InMemoryDatabaseRoot())
                .Options;

            _dataDataContext = new MatchedLearnerDataContext(bsContextOption);

            var matchLearnerContextFactory = new MatchedLearnerDataContextFactory(bsContextOption);

            _sut = new MatchedLearnerRepository(_dataDataContext, matchLearnerContextFactory, fixture.Create<Mock<ILogger<MatchedLearnerRepository>>>().Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dataDataContext.Database.EnsureDeleted();
            _dataDataContext.Dispose();
        }

        [Test]
        public async Task ThenRetrievesPayablePeriodsWithNonZeroAmountForMultipleAcademicYears()
        {
            //Arrange
            _dataLockEventPayablePeriodAy1.TransactionType = 1;
            _dataLockEventPayablePeriodAy1.Amount = 100;
            _dataLockEventPayablePeriodAy2.TransactionType = 1;
            _dataLockEventPayablePeriodAy2.Amount = 100;

            await AddPriceEpisodesToDataLocks();
            await AddPayablePeriodsToDataLocks();

            await AddDataLocksToDb();

            //Act
            var result = await _sut.GetDataLockEvents(_ukprn, _uln);

            //Assert
            result.DataLockEvents.Count.Should().Be(2);
            result.DataLockEventPayablePeriods.Count.Should().Be(2);
        }

        private async Task AddPriceEpisodesToDataLocks()
        {
            _dataLockEventPriceEpisodeAy1.DataLockEventId = _dataLockEventAy1.EventId;
            _dataDataContext.DataLockEventPriceEpisode.Add(_dataLockEventPriceEpisodeAy1);

            _dataLockEventPriceEpisodeAy2.DataLockEventId = _dataLockEventAy2.EventId;
            _dataDataContext.DataLockEventPriceEpisode.Add(_dataLockEventPriceEpisodeAy2);

            await _dataDataContext.SaveChangesAsync();
        }

        private async Task AddPayablePeriodsToDataLocks()
        {
            _dataLockEventPayablePeriodAy1.DataLockEventId = _dataLockEventAy1.EventId;
            _dataLockEventPayablePeriodAy1.PriceEpisodeIdentifier = _dataLockEventPriceEpisodeAy1.PriceEpisodeIdentifier;

            _dataDataContext.DataLockEventPayablePeriod.Add(_dataLockEventPayablePeriodAy1);


            _dataLockEventPayablePeriodAy2.DataLockEventId = _dataLockEventAy2.EventId;
            _dataLockEventPayablePeriodAy2.PriceEpisodeIdentifier = _dataLockEventPriceEpisodeAy2.PriceEpisodeIdentifier;

            _dataDataContext.DataLockEventPayablePeriod.Add(_dataLockEventPayablePeriodAy2);

            await _dataDataContext.SaveChangesAsync();
        }

        private async Task AddDataLocksToDb()
        {
            _dataLockEventAy1.LearnerUln = _uln;
            _dataLockEventAy1.Ukprn = _ukprn;
            _dataLockEventAy1.LearningAimReference = "ZPROG001";
            _dataLockEventAy1.CollectionPeriod = 14;
            _dataLockEventAy1.AcademicYear = _academicYear1;

            _dataDataContext.DataLockEvent.Add(_dataLockEventAy1);


            _dataLockEventAy2.LearnerUln = _uln;
            _dataLockEventAy2.Ukprn = _ukprn;
            _dataLockEventAy2.LearningAimReference = "ZPROG001";
            _dataLockEventAy2.CollectionPeriod = 1;
            _dataLockEventAy2.AcademicYear = _academicYear2;

            _dataDataContext.DataLockEvent.Add(_dataLockEventAy2);

            await _dataDataContext.SaveChangesAsync();
        }
    }
}