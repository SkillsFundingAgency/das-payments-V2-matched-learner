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
    public class WhenGettingDataLockEventsAcrossMultipleAcademicYears
    {
        private IPaymentsDataLockRepository _sut;
        private PaymentsContext _context;
        private DataLockEvent _dataLockEventAy1;
        private DataLockEventPayablePeriod _dataLockEventPayablePeriodAy1;
        private DataLockEventPriceEpisode _dataLockEventPriceEpisodeAy1;
        private LatestSuccessfulJobModel _latestSuccessfulJobAy1;
        private DataLockEvent _dataLockEventAy2;
        private DataLockEventPayablePeriod _dataLockEventPayablePeriodAy2;
        private DataLockEventPriceEpisode _dataLockEventPriceEpisodeAy2;
        private LatestSuccessfulJobModel _latestSuccessfulJobAy2;

        private long _ukprn, _uln;
        private long _jobIdAy1;
        private long _jobIdAy2;
        private short _academicYear1;
        private byte _collectionPeriodAy1;
        private short _academicYear2;
        private byte _collectionPeriodAy2;

        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();

            _ukprn = fixture.Create<long>();
            _uln = fixture.Create<long>();

            _jobIdAy1 = fixture.Create<long>();
            _academicYear1 = fixture.Create<short>();
            _collectionPeriodAy1 = fixture.Create<byte>();
            _jobIdAy2 = fixture.Create<long>();
            _academicYear2 = fixture.Create<short>();
            _collectionPeriodAy2 = fixture.Create<byte>();

            _dataLockEventAy1 = fixture.Create<DataLockEvent>();
            _dataLockEventAy1.AcademicYear = _academicYear1;
            _dataLockEventAy2 = fixture.Create<DataLockEvent>();
            _dataLockEventAy2.AcademicYear = _academicYear2;

            _latestSuccessfulJobAy1 = fixture.Create<LatestSuccessfulJobModel>();
            _latestSuccessfulJobAy1.Ukprn = _ukprn;
            _latestSuccessfulJobAy1.AcademicYear = _academicYear1;
            _latestSuccessfulJobAy1.CollectionPeriod = _collectionPeriodAy1;
            _latestSuccessfulJobAy1.DcJobId = _jobIdAy1;

            _latestSuccessfulJobAy2 = fixture.Create<LatestSuccessfulJobModel>();
            _latestSuccessfulJobAy2.Ukprn = _ukprn;
            _latestSuccessfulJobAy2.AcademicYear = _academicYear2;
            _latestSuccessfulJobAy2.CollectionPeriod = _collectionPeriodAy2;
            _latestSuccessfulJobAy2.DcJobId = _jobIdAy2;

            _dataLockEventPayablePeriodAy1 = fixture.Create<DataLockEventPayablePeriod>();
            _dataLockEventPayablePeriodAy2 = fixture.Create<DataLockEventPayablePeriod>();
            _dataLockEventPriceEpisodeAy1 = fixture.Create<DataLockEventPriceEpisode>();
            _dataLockEventPriceEpisodeAy2 = fixture.Create<DataLockEventPriceEpisode>();

            _context = new PaymentsContext(new DbContextOptionsBuilder<PaymentsContext>()
                    .UseInMemoryDatabase("TestDb", new InMemoryDatabaseRoot())
                    .Options);

            _sut = new PaymentsDataLockRepository(_context, fixture.Create<Mock<ILogger<PaymentsDataLockRepository>>>().Object);
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

            await AddLatestSuccessfulJobsToDb();
            await AddDataLocksToDb();

            //Act
            var result = await _sut.GetDataLockEvents(_ukprn, _uln);

            //Assert
            result.DataLockEvents.Count.Should().Be(2);
            result.DataLockEventPayablePeriods.Count.Should().Be(2);
        }

        private async Task AddLatestSuccessfulJobsToDb()
        {
            _context.LatestSuccessfulJobs.Add(_latestSuccessfulJobAy1);
            _context.LatestSuccessfulJobs.Add(_latestSuccessfulJobAy2);
            await _context.SaveChangesAsync();
        }

        private async Task AddPriceEpisodesToDataLocks()
        {
            _dataLockEventPriceEpisodeAy1.DataLockEventId = _dataLockEventAy1.EventId;
            _context.DataLockEventPriceEpisode.Add(_dataLockEventPriceEpisodeAy1);

            _dataLockEventPriceEpisodeAy2.DataLockEventId = _dataLockEventAy2.EventId;
            _context.DataLockEventPriceEpisode.Add(_dataLockEventPriceEpisodeAy2);

            await _context.SaveChangesAsync();
        }

        private async Task AddPayablePeriodsToDataLocks()
        {
            _dataLockEventPayablePeriodAy1.DataLockEventId = _dataLockEventAy1.EventId;
            _dataLockEventPayablePeriodAy1.PriceEpisodeIdentifier = _dataLockEventPriceEpisodeAy1.PriceEpisodeIdentifier;

            _context.DataLockEventPayablePeriod.Add(_dataLockEventPayablePeriodAy1);


            _dataLockEventPayablePeriodAy2.DataLockEventId = _dataLockEventAy2.EventId;
            _dataLockEventPayablePeriodAy2.PriceEpisodeIdentifier = _dataLockEventPriceEpisodeAy2.PriceEpisodeIdentifier;

            _context.DataLockEventPayablePeriod.Add(_dataLockEventPayablePeriodAy2);

            await _context.SaveChangesAsync();
        }

        private async Task AddDataLocksToDb()
        {
            _dataLockEventAy1.LearnerUln = _uln;
            _dataLockEventAy1.Ukprn = _ukprn;
            _dataLockEventAy1.JobId = _latestSuccessfulJobAy1.DcJobId;
            _dataLockEventAy1.LearningAimReference = "ZPROG001";
            _dataLockEventAy1.CollectionPeriod = _latestSuccessfulJobAy1.CollectionPeriod;
            _dataLockEventAy1.AcademicYear = _latestSuccessfulJobAy1.AcademicYear;

            _context.DataLockEvent.Add(_dataLockEventAy1);


            _dataLockEventAy2.LearnerUln = _uln;
            _dataLockEventAy2.Ukprn = _ukprn;
            _dataLockEventAy2.JobId = _latestSuccessfulJobAy2.DcJobId;
            _dataLockEventAy2.LearningAimReference = "ZPROG001";
            _dataLockEventAy2.CollectionPeriod = _latestSuccessfulJobAy2.CollectionPeriod;
            _dataLockEventAy2.AcademicYear = _latestSuccessfulJobAy2.AcademicYear;

            _context.DataLockEvent.Add(_dataLockEventAy2);

            await _context.SaveChangesAsync();
        }
    }
}