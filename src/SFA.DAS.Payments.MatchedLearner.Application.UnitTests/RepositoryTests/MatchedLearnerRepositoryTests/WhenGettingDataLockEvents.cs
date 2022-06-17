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
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.RepositoryTests.MatchedLearnerRepositoryTests
{
    [TestFixture]
    public class WhenGettingDataLockEvents
    {
        private IMatchedLearnerRepository _sut;
        private MatchedLearnerDataContext _dataDataContext;
        private DataLockEventModel _dataLockEvent;
        private SubmissionJobModel _submissionJob;
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
            _dataLockEvent.LearnerUln = _uln;
            _dataLockEvent.Ukprn = _ukprn;
            _dataLockEvent.EventId = Guid.NewGuid();
            _dataLockEvent.LearningAimReference = "ZPROG001";
            _dataLockEvent.CollectionPeriod = 11;
            _dataLockEvent.AcademicYear = 2021;

            _submissionJob = fixture.Create<SubmissionJobModel>();
            _submissionJob.Ukprn = _ukprn;

            _dataLockEventNonPayablePeriod = fixture.Freeze<DataLockEventNonPayablePeriodModel>();
            _dataLockEventPayablePeriod = fixture.Freeze<DataLockEventPayablePeriodModel>();
            _dataLockEventPriceEpisode = fixture.Freeze<DataLockEventPriceEpisodeModel>();
            _dataLockEventPriceEpisode.PriceEpisodeIdentifier = Guid.NewGuid().ToString();

            var dbOptions = new DbContextOptionsBuilder<MatchedLearnerDataContext>()
                .UseInMemoryDatabase($"TestDb-{Guid.NewGuid()}", new InMemoryDatabaseRoot())
                .Options;

            _dataDataContext = new MatchedLearnerDataContext(dbOptions);

            var contextFactory = new MatchedLearnerDataContextFactory(dbOptions);

            _sut = new MatchedLearnerRepository(_dataDataContext, contextFactory, fixture.Create<Mock<ILogger<MatchedLearnerRepository>>>().Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dataDataContext.Database.EnsureDeleted();
            _dataDataContext.Dispose();
        }

        [Test]
        public async Task ThenDoesNotRetrieveNonPayablePeriodsWithZeroAmount()
        {
            //Arrange
            _dataLockEventNonPayablePeriod.TransactionType = 1;
            _dataLockEventNonPayablePeriod.Amount = 0;

            AttachPriceEpisodeToDataLock();
            AttachNonPayablePeriodToDataLock();

            await AddDataLockToDb(_dataLockEvent);

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

            await AddDataLockToDb(_dataLockEvent);

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

            await AddDataLockToDb(_dataLockEvent);

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

            await AddDataLockToDb(_dataLockEvent);

            //Act
            var result = await _sut.GetDataLockEvents(_ukprn, _uln);

            //Assert
            result.DataLockEvents.Count.Should().Be(1);
            result.DataLockEventPayablePeriods.Count.Should().Be(1);
        }

        [Test]
        public async Task AndThereIsOnlyOneProviderSubmissionJob_ThenReturnsIt()
        {
            //Arrange
            AttachPriceEpisodeToDataLock();
            AttachPayablePeriodToDataLock();

            await AddDataLockToDb(_dataLockEvent);
            await AddSubmissionJobToDb(_submissionJob);

            //Act
            var result = await _sut.GetDataLockEvents(_ukprn, _uln);

            //Assert
            result.LatestProviderSubmissionJob.Should().NotBeNull();
            result.LatestProviderSubmissionJob.Should().Be(_submissionJob);
        }

        [Test]
        public async Task AndThereAreMultipleProviderSubmissionJobs_ThenReturnsLatest()
        {
            //Arrange
            AttachPriceEpisodeToDataLock();
            AttachPayablePeriodToDataLock();

            var expectedLatestSubmissionJob = new SubmissionJobModel
            {
                AcademicYear = short.MaxValue,
                CollectionPeriod = byte.MaxValue,
                IlrSubmissionDateTime = DateTime.MaxValue,
                EventTime = DateTimeOffset.MaxValue,
                Ukprn = _ukprn
            };

            await AddDataLockToDb(_dataLockEvent);
            await AddSubmissionJobToDb(_submissionJob);
            await AddSubmissionJobToDb(expectedLatestSubmissionJob);

            //Act
            var result = await _sut.GetDataLockEvents(_ukprn, _uln);

            //Assert
            result.LatestProviderSubmissionJob.Should().NotBeNull();
            result.LatestProviderSubmissionJob.Should().Be(expectedLatestSubmissionJob);
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

        private async Task AddDataLockToDb(DataLockEventModel dataLockEvent)
        {
            _dataDataContext.DataLockEvent.Add(dataLockEvent);

            await _dataDataContext.SaveChangesAsync();
        }

        private async Task AddSubmissionJobToDb(SubmissionJobModel submissionJob)
        {
            _dataDataContext.SubmissionJobs.Add(submissionJob);

            await _dataDataContext.SaveChangesAsync();
        }
    }
}