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

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.RepositoryTests.MatchedLearnerRepositoryTests
{
    [TestFixture]
    public class WhenGettingMatchedLearnerTrainings
    {
        private IMatchedLearnerRepository _sut;
        private MatchedLearnerDataContext _dataDataContext;
        private TrainingModel _trainingModel;
        private PeriodModel _payablePeriod;
        private PeriodModel _nonPayablePeriod;
        private PriceEpisodeModel _priceEpisode;

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

            _trainingModel = fixture.Create<TrainingModel>();

            _payablePeriod = fixture.Freeze<PeriodModel>();
            _payablePeriod.IsPayable = true;

            _nonPayablePeriod = fixture.Freeze<PeriodModel>();
            _nonPayablePeriod.IsPayable = false;
            
            _priceEpisode = fixture.Freeze<PriceEpisodeModel>();
            _priceEpisode.Identifier = Guid.NewGuid().ToString();

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
        public async Task ThenRetrievesNonPayablePeriodsWithNonZeroAmount()
        {
            //Arrange
            _payablePeriod.TransactionType = 1;
            _payablePeriod.Amount = 100;

            await AddTrainingToDb();

            await AddPriceEpisode();
            await AddNonPayablePeriod();

            //Act
            var result = await _sut.GetMatchedLearnerTrainings(_ukprn, _uln);

            //Assert
            result.Count.Should().Be(1);
            result.First().PriceEpisodes.First().Periods.Count(x => !x.IsPayable).Should().Be(1);
        }

        [Test]
        public async Task ThenRetrievesPayablePeriodsWithNonZeroAmount()
        {
            //Arrange
            _nonPayablePeriod.TransactionType = 1;
            _nonPayablePeriod.Amount = 100;

            await AddTrainingToDb();

            await AddPriceEpisode();
            await AddPayablePeriod();

            //Act
            var result = await _sut.GetMatchedLearnerTrainings(_ukprn, _uln);

            //Assert
            result.Count.Should().Be(1);
            result.First().PriceEpisodes.First().Periods.Count(x => x.IsPayable).Should().Be(1);
        }

        private async Task AddPriceEpisode()
        {
            _priceEpisode.TrainingId = _trainingModel.Id;
            _priceEpisode.Periods.Clear();

            _dataDataContext.PriceEpisodes.Add(_priceEpisode);

            await _dataDataContext.SaveChangesAsync();
        }

        private async Task AddNonPayablePeriod()
        {
            _nonPayablePeriod.PriceEpisodeId = _priceEpisode.Id;
            _nonPayablePeriod.IsPayable = false;
            
            _dataDataContext.Periods.Add(_nonPayablePeriod);
            
            await _dataDataContext.SaveChangesAsync();
        }

        private async Task AddPayablePeriod()
        {
            _payablePeriod.PriceEpisodeId = _priceEpisode.Id;
            _payablePeriod.IsPayable = true;

            _dataDataContext.Periods.Add(_payablePeriod); 
            
            await _dataDataContext.SaveChangesAsync();
        }

        private async Task AddTrainingToDb()
        {
            _trainingModel.Uln = _uln;
            _trainingModel.Ukprn = _ukprn;
            _trainingModel.EventId = Guid.NewGuid();
            _trainingModel.Reference = "ZPROG001";
            _trainingModel.IlrSubmissionWindowPeriod = 11;
            _trainingModel.AcademicYear = 2021;
            _trainingModel.PriceEpisodes.Clear();

            _dataDataContext.Trainings.Add(_trainingModel);

            await _dataDataContext.SaveChangesAsync();
        }
    }
}