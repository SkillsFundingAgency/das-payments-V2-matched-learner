using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application.Data;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;
using SFA.DAS.Payments.MatchedLearner.Application.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.RepositoryTests.PaymentsDataLockRepositoryTests
{
    [TestFixture]
    public class WhenGettingDataLockEvents
    {
        private IPaymentsDataLockRepository _sut;
        private PaymentsContext _context;
        private DatalockEvent _datalockEvent;
        private DatalockEventNonPayablePeriod _datalockEventNonPayablePeriod;
        private DatalockEventPayablePeriod _datalockEventPayablePeriod;
        private DatalockEventPriceEpisode _datalockEventPriceEpisode;
        private LatestSuccessfulJobModel _latestSuccessfulJob;

        private long _ukprn, _uln, _jobId;
        private short _academicYear;
        private byte _collectionPeriod;

        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();

            _ukprn = fixture.Create<long>();
            _uln = fixture.Create<long>();
            _jobId = fixture.Create<long>();
            _academicYear = fixture.Create<short>();
            _collectionPeriod = fixture.Create<byte>();

            _datalockEvent = fixture.Create<DatalockEvent>();
            _datalockEvent.PayablePeriods.Clear();
            _datalockEvent.NonPayablePeriods.Clear();
            _datalockEvent.PriceEpisodes.Clear();

            _latestSuccessfulJob = fixture.Create<LatestSuccessfulJobModel>();
            _latestSuccessfulJob.Ukprn = _ukprn;
            _latestSuccessfulJob.AcademicYear = _academicYear;
            _latestSuccessfulJob.CollectionPeriod = _collectionPeriod;
            _latestSuccessfulJob.DcJobId = _jobId;

            _datalockEventNonPayablePeriod = fixture.Create<DatalockEventNonPayablePeriod>();
            _datalockEventPayablePeriod = fixture.Create<DatalockEventPayablePeriod>();
            _datalockEventPriceEpisode = fixture.Create<DatalockEventPriceEpisode>();

            _context = new PaymentsContext(new DbContextOptionsBuilder<PaymentsContext>()
                    .UseInMemoryDatabase("TestDb", new InMemoryDatabaseRoot())
                    .Options);

            _sut = new PaymentsDataLockRepository(_context);
        }

        [Test]
        public async Task ThenDoesNotRetrieveNonPayablePeriodsWithZeroAmount()
        {
            //Arrange
            _datalockEventNonPayablePeriod.TransactionType = 1;
            _datalockEventNonPayablePeriod.Amount = 0;

            AddPriceEpisodeToDataLock();
            AddNonPayablePeriodToDataLock();

            await AddLatestSuccessfulJobToDb();
            await AddDataLockToDb();

            //Act
            var result = await _sut.GetDatalockEvents(_ukprn, _uln);

            //Assert
            result.Count.Should().Be(1);
            result.First().NonPayablePeriods.Count.Should().Be(0);
        }

        [Test]
        public async Task ThenRetrievesNonPayablePeriodsWithNonZeroAmount()
        {
            //Arrange
            _datalockEventNonPayablePeriod.TransactionType = 1;
            _datalockEventNonPayablePeriod.Amount = 100;

            AddPriceEpisodeToDataLock();
            AddNonPayablePeriodToDataLock();

            await AddLatestSuccessfulJobToDb();
            await AddDataLockToDb();

            //Act
            var result = await _sut.GetDatalockEvents(_ukprn, _uln);

            //Assert
            result.Count.Should().Be(1);
            result.First().NonPayablePeriods.Count.Should().Be(1);
        }

        [Test]
        public async Task ThenDoesNotRetrievePayablePeriodsWithZeroAmount()
        {
            //Arrange
            _datalockEventPayablePeriod.TransactionType = 1;
            _datalockEventPayablePeriod.Amount = 0;

            AddPriceEpisodeToDataLock();
            AddPayablePeriodToDataLock();
            AddNonPayablePeriodToDataLock();

            await AddLatestSuccessfulJobToDb();
            await AddDataLockToDb();

            //Act
            var result = await _sut.GetDatalockEvents(_ukprn, _uln);

            //Assert
            result.Count.Should().Be(1);
            result.First().PayablePeriods.Count.Should().Be(0);
        }

        [Test]
        public async Task ThenRetrievesPayablePeriodsWithNonZeroAmount()
        {
            //Arrange
            _datalockEventPayablePeriod.TransactionType = 1;
            _datalockEventPayablePeriod.Amount = 100;

            AddPriceEpisodeToDataLock();
            AddPayablePeriodToDataLock();
            AddNonPayablePeriodToDataLock();

            await AddLatestSuccessfulJobToDb();
            await AddDataLockToDb();

            //Act
            var result = await _sut.GetDatalockEvents(_ukprn, _uln);

            //Assert
            result.Count.Should().Be(1);
            result.First().PayablePeriods.Count.Should().Be(1);
        }

        private async Task AddLatestSuccessfulJobToDb()
        {
            _context.LatestSuccessfulJobs.Add(_latestSuccessfulJob);
            await _context.SaveChangesAsync();
        }

        private void AddPriceEpisodeToDataLock()
        {
            _datalockEventPriceEpisode.DataLockEventId = _datalockEvent.EventId;
            _datalockEvent.PriceEpisodes.Add(_datalockEventPriceEpisode);
        }

        private void AddNonPayablePeriodToDataLock()
        {
            _datalockEventNonPayablePeriod.PriceEpisodeIdentifier = _datalockEventPriceEpisode.PriceEpisodeIdentifier;
            _datalockEventNonPayablePeriod.DataLockEventId = _datalockEvent.EventId;
            _datalockEvent.NonPayablePeriods.Add(_datalockEventNonPayablePeriod);
        }

        private void AddPayablePeriodToDataLock()
        {
            _datalockEventPayablePeriod.DataLockEventId = _datalockEvent.EventId;
            _datalockEventPayablePeriod.PriceEpisodeIdentifier = _datalockEventPriceEpisode.PriceEpisodeIdentifier;
            _datalockEvent.PayablePeriods.Add(_datalockEventPayablePeriod);
        }

        private async Task AddDataLockToDb()
        {
            _datalockEvent.LearnerUln = _uln;
            _datalockEvent.Ukprn = _ukprn;
            _datalockEvent.JobId = _latestSuccessfulJob.DcJobId;
            _datalockEvent.LearningAimReference = "ZPROG001";
            _datalockEvent.CollectionPeriod = _latestSuccessfulJob.CollectionPeriod;
            _datalockEvent.AcademicYear = _latestSuccessfulJob.AcademicYear;

            _context.DatalockEvents.Add(_datalockEvent);

            await _context.SaveChangesAsync();
        }
    }
}