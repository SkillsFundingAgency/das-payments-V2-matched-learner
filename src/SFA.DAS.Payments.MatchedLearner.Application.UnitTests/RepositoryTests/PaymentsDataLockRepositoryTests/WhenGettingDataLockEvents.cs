using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application.Data;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;
using SFA.DAS.Payments.MatchedLearner.Application.Repositories;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.RepositoryTests.PaymentsDataLockRepositoryTests
{
    [TestFixture]
    public class WhenGettingDataLockEvents
    {
        private IMatchedLearnerRepository _sut;
        private MatchedLearnerContext _context;
        private DataLockEvent _dataLockEvent;
        private DataLockEventNonPayablePeriod _dataLockEventNonPayablePeriod;
        private DataLockEventPayablePeriod _dataLockEventPayablePeriod;
        private DataLockEventPriceEpisode _dataLockEventPriceEpisode;

        private long _ukprn, _uln;

        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();

            _ukprn = fixture.Create<long>();
            _uln = fixture.Create<long>();

            _dataLockEvent = fixture.Create<DataLockEvent>();

            _dataLockEventNonPayablePeriod = fixture.Create<DataLockEventNonPayablePeriod>();
            _dataLockEventPayablePeriod = fixture.Create<DataLockEventPayablePeriod>();
            _dataLockEventPriceEpisode = fixture.Create<DataLockEventPriceEpisode>();

            _context = new MatchedLearnerContext(new DbContextOptionsBuilder<MatchedLearnerContext>()
                    .UseInMemoryDatabase("TestDb", new InMemoryDatabaseRoot())
                    .Options);

            _sut = new MatchedLearnerRepository(_context, fixture.Create<Mock<ILogger<MatchedLearnerRepository>>>().Object);
        }

        [Test]
        public async Task ThenDoesNotRetrieveNonPayablePeriodsWithZeroAmount()
        {
            //Arrange
            _dataLockEventNonPayablePeriod.TransactionType = 1;
            _dataLockEventNonPayablePeriod.Amount = 0;

            await AddPriceEpisodeToDataLock();
            await AddNonPayablePeriodToDataLock();

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

            await AddPriceEpisodeToDataLock();
            await AddNonPayablePeriodToDataLock();

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

            await AddPriceEpisodeToDataLock();
            await AddPayablePeriodToDataLock();

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

            await AddPriceEpisodeToDataLock();
            await AddPayablePeriodToDataLock();

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
            await _context.SaveChangesAsync();
        }

        private async Task AddPriceEpisodeToDataLock()
        {
            _dataLockEventPriceEpisode.DataLockEventId = _dataLockEvent.EventId;
            _context.DataLockEventPriceEpisode.Add(_dataLockEventPriceEpisode);

            await _context.SaveChangesAsync();
        }

        private async Task AddNonPayablePeriodToDataLock()
        {
            _dataLockEventNonPayablePeriod.PriceEpisodeIdentifier = _dataLockEventPriceEpisode.PriceEpisodeIdentifier;
            _dataLockEventNonPayablePeriod.DataLockEventId = _dataLockEvent.EventId;

            _context.DataLockEventNonPayablePeriod.Add(_dataLockEventNonPayablePeriod);

            await _context.SaveChangesAsync();
        }

        private async Task AddPayablePeriodToDataLock()
        {
            _dataLockEventPayablePeriod.DataLockEventId = _dataLockEvent.EventId;
            _dataLockEventPayablePeriod.PriceEpisodeIdentifier = _dataLockEventPriceEpisode.PriceEpisodeIdentifier;

            _context.DataLockEventPayablePeriod.Add(_dataLockEventPayablePeriod);

            await _context.SaveChangesAsync();
        }

        private async Task AddDataLockToDb()
        {
            _dataLockEvent.LearnerUln = _uln;
            _dataLockEvent.Ukprn = _ukprn;
            _dataLockEvent.LearningAimReference = "ZPROG001";
            _dataLockEvent.CollectionPeriod = 11;
            _dataLockEvent.AcademicYear = 2021;

            _context.DataLockEvent.Add(_dataLockEvent);

            await _context.SaveChangesAsync();
        }
    }
}