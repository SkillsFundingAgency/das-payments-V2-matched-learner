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
    public class WhenGettingMatchedLearnerTrainingsAcrossMultipleAcademicYears
    {
        private IMatchedLearnerRepository _sut;
        private MatchedLearnerDataContext _dataDataContext;
        private TrainingModel _trainingAy1;
        private PeriodModel _payablePeriodAy1;
        private PriceEpisodeModel _priceEpisodeAy1;
        private TrainingModel _trainingAy2;
        private PeriodModel _payablePeriodAy2;
        private PriceEpisodeModel _priceEpisodeAy2;

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

            _trainingAy1 = fixture.Create<TrainingModel>();
            _trainingAy2 = fixture.Create<TrainingModel>();

            _payablePeriodAy1 = fixture.Create<PeriodModel>();
            _payablePeriodAy1.IsPayable = true;
            _payablePeriodAy2 = fixture.Create<PeriodModel>();
            _payablePeriodAy2.IsPayable = true;

            _priceEpisodeAy1 = fixture.Create<PriceEpisodeModel>();
            _priceEpisodeAy2 = fixture.Create<PriceEpisodeModel>();

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
            _payablePeriodAy1.TransactionType = 1;
            _payablePeriodAy1.Amount = 100;
            _payablePeriodAy2.TransactionType = 1;
            _payablePeriodAy2.Amount = 100;

            await AddPriceEpisodes();
            await AddPayablePeriods();

            await AddTrainingToDb();

            //Act
            var result = await _sut.GetMatchedLearnerTrainings(_ukprn, _uln);

            //Assert
            result.Count.Should().Be(2);
            result.SelectMany(p => p.PriceEpisodes).SelectMany(p => p.Periods.Where(x => x.IsPayable)).Count().Should().Be(2);
        }

        private async Task AddPriceEpisodes()
        {
            _priceEpisodeAy1.TrainingId = _trainingAy1.Id;
            _priceEpisodeAy1.Periods.Clear();

            _dataDataContext.PriceEpisodes.Add(_priceEpisodeAy1);

            _priceEpisodeAy2.TrainingId = _trainingAy2.Id;
            _priceEpisodeAy2.Periods.Clear();

            _dataDataContext.PriceEpisodes.Add(_priceEpisodeAy2);

            await _dataDataContext.SaveChangesAsync();
        }

        private async Task AddPayablePeriods()
        {
            _payablePeriodAy1.PriceEpisodeId = _priceEpisodeAy1.Id;
            _dataDataContext.Periods.Add(_payablePeriodAy1);

            _payablePeriodAy2.PriceEpisodeId = _priceEpisodeAy2.Id;
            _dataDataContext.Periods.Add(_payablePeriodAy2);

            await _dataDataContext.SaveChangesAsync();
        }

        private async Task AddTrainingToDb()
        {
            _trainingAy1.Uln = _uln;
            _trainingAy1.Ukprn = _ukprn;
            _trainingAy1.Reference = "ZPROG001";
            _trainingAy1.IlrSubmissionWindowPeriod = 14;
            _trainingAy1.AcademicYear = _academicYear1;
            _trainingAy1.PriceEpisodes.Clear();

            _dataDataContext.Trainings.Add(_trainingAy1);


            _trainingAy2.Uln = _uln;
            _trainingAy2.Ukprn = _ukprn;
            _trainingAy2.Reference = "ZPROG001";
            _trainingAy2.IlrSubmissionWindowPeriod = 1;
            _trainingAy2.AcademicYear = _academicYear2;
            _trainingAy2.PriceEpisodes.Clear();

            _dataDataContext.Trainings.Add(_trainingAy2);

            await _dataDataContext.SaveChangesAsync();
        }
    }
}