using System;
using System.Linq;
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

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.RepositoryTests.PaymentsDataLockRepositoryTests
{
    [TestFixture]
    public class WhenGettingDataLockEvents
    {
        private IMatchedLearnerRepository _sut;
        private MatchedLearnerDataContext _dataDataContext;
        private DataLockEventModel _dataLockEvent;
        private DataLockEventNonPayablePeriodModel _dataLockEventNonPayablePeriod;
        private DataLockEventPayablePeriodModel _dataLockEventPayablePeriod;
        private DataLockEventPriceEpisodeModel _dataLockEventPriceEpisode;

        private long _ukprn, _uln;

        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();

            fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _ukprn = fixture.Create<long>();
            _uln = fixture.Create<long>();

            _dataLockEvent = fixture.Create<DataLockEventModel>();

            _dataLockEventNonPayablePeriod = fixture.Freeze<DataLockEventNonPayablePeriodModel>();
            _dataLockEventPayablePeriod = fixture.Freeze<DataLockEventPayablePeriodModel>();
            _dataLockEventPriceEpisode = fixture.Freeze<DataLockEventPriceEpisodeModel>();
            _dataLockEventPriceEpisode.PriceEpisodeIdentifier = Guid.NewGuid().ToString();

            var dbOptions = new DbContextOptionsBuilder<MatchedLearnerDataContext>()
                .UseInMemoryDatabase("TestDb", new InMemoryDatabaseRoot())
                .Options;

            _dataDataContext = new MatchedLearnerDataContext(dbOptions);

            var contextFactory = new MatchedLearnerDataContextFactory(dbOptions);

            _sut = new MatchedLearnerRepository(_dataDataContext, contextFactory, fixture.Create<Mock<ILogger<MatchedLearnerRepository>>>().Object);
        }

        [Test]
        public async Task ThenDoesNotRetrieveNonPayablePeriodsWithZeroAmount()
        {
            //Arrange
            _dataLockEventNonPayablePeriod.TransactionType = 1;
            _dataLockEventNonPayablePeriod.Amount = 0;

            AttachPriceEpisodeToDataLock();
            AttachNonPayablePeriodToDataLock();

            await AddLatestSuccessfulJobToDb();
            await AddDataLockToDb();

            //Act
            var result = await _sut.GetDataLockEvents(_ukprn, _uln);

            //Assert
            result.DataLockEvents.Count.Should().Be(1);
            result.DataLockEventNonPayablePeriods.Count.Should().Be(0);
        }

        [Test]
        public async Task ThenRetrievesNonPayablePeriodsWithNonZeroAmount()
        {
            //Arrange
            _dataLockEventNonPayablePeriod.TransactionType = 1;
            _dataLockEventNonPayablePeriod.Amount = 100;

            AttachPriceEpisodeToDataLock();
            AttachNonPayablePeriodToDataLock();

            await AddLatestSuccessfulJobToDb();
            await AddDataLockToDb();

            //Act
            var result = await _sut.GetDataLockEvents(_ukprn, _uln);

            //Assert
            result.DataLockEvents.Count.Should().Be(1);
            result.DataLockEventNonPayablePeriods.Count.Should().Be(1);
        }

        [Test]
        public async Task ThenDoesNotRetrievePayablePeriodsWithZeroAmount()
        {
            //Arrange
            _dataLockEventPayablePeriod.TransactionType = 1;
            _dataLockEventPayablePeriod.Amount = 0;

            AttachPriceEpisodeToDataLock();
            AttachPayablePeriodToDataLock();

            await AddLatestSuccessfulJobToDb();
            await AddDataLockToDb();

            //Act
            var result = await _sut.GetDataLockEvents(_ukprn, _uln);

            //Assert
            result.DataLockEvents.Count.Should().Be(1);
            result.DataLockEventPayablePeriods.Count.Should().Be(0);
        }

        [Test]
        public async Task ThenRetrievesPayablePeriodsWithNonZeroAmount()
        {
            //Arrange
            _dataLockEventPayablePeriod.TransactionType = 1;
            _dataLockEventPayablePeriod.Amount = 100;

            AttachPriceEpisodeToDataLock();
            AttachPayablePeriodToDataLock();

            await AddLatestSuccessfulJobToDb();
            await AddDataLockToDb();

            //Act
            var result = await _sut.GetDataLockEvents(_ukprn, _uln);

            //Assert
            result.DataLockEvents.Count.Should().Be(1);
            result.DataLockEventPayablePeriods.Count.Should().Be(1);
        }

        private async Task AddLatestSuccessfulJobToDb()
        {
            await _dataDataContext.SaveChangesAsync();
        }

        private void AttachPriceEpisodeToDataLock()
        {
            _dataLockEventPriceEpisode.DataLockEventId = _dataLockEvent.EventId;
            _dataLockEvent.PriceEpisodes.Add(_dataLockEventPriceEpisode);
        }

        private void AttachNonPayablePeriodToDataLock()
        {
            _dataLockEventNonPayablePeriod.PriceEpisodeIdentifier = _dataLockEventPriceEpisode.PriceEpisodeIdentifier;
            _dataLockEventNonPayablePeriod.DataLockEventId = _dataLockEvent.EventId;

            _dataLockEvent.NonPayablePeriods.Add(_dataLockEventNonPayablePeriod);
        }

        private void AttachPayablePeriodToDataLock()
        {
            _dataLockEventPayablePeriod.DataLockEventId = _dataLockEvent.EventId;
            _dataLockEventPayablePeriod.PriceEpisodeIdentifier = _dataLockEventPriceEpisode.PriceEpisodeIdentifier;

            _dataLockEvent.PayablePeriods.Add(_dataLockEventPayablePeriod);
        }

        private async Task AddDataLockToDb()
        {
            _dataLockEvent.LearnerUln = _uln;
            _dataLockEvent.Ukprn = _ukprn;
            _dataLockEvent.LearningAimReference = "ZPROG001";
            _dataLockEvent.CollectionPeriod = 11;
            _dataLockEvent.AcademicYear = 2021;

            _dataDataContext.DataLockEvent.Add(_dataLockEvent);

            await _dataDataContext.SaveChangesAsync();
        }
    }
}