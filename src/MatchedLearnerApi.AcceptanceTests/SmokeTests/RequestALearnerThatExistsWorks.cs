using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using MatchedLearnerApi.Types;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace MatchedLearnerApi.AcceptanceTests.SmokeTests
{
    [TestFixture]
    public class RequestALearnerThatExistsWorks
    {
        private readonly string _connectionString;

        public RequestALearnerThatExistsWorks()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");

            var configuration = configurationBuilder.Build();

            _connectionString = configuration.GetConnectionString("DasPayments");

            
        }

        [OneTimeSetUp]
        public void Setup()
        {
            var repository = new TestRepository(_connectionString);
            repository.ClearLearner(-1000, -2000).Wait();
            repository.AddDatalockEvent(-1000, -2000).Wait();
        }

        [OneTimeTearDown]
        public void Teardown()
        {

        }

        [Test]
        public async Task RequestWithGoodData_Should_Return200()
        {
            var request = new TestClient();

            var actual = await request.Handle(-1000, -2000);

            actual.Should().BeAssignableTo<MatchedLearnerResultDto>();
        }

        [Test]
        public async Task RequestWithGoodData_Should_ReturnCorrectValues()
        {
            var request = new TestClient();

            var actual = await request.Handle(-1000, -2000);

            actual.StartDate.Should().Be(new DateTime(2020, 10, 9).ToDateTimeOffset(TimeSpan.FromHours(1)));
            actual.IlrSubmissionDate.Should().Be(new DateTime(2020, 10, 10).ToDateTimeOffset(TimeSpan.FromHours(1)));
            actual.IlrSubmissionWindowPeriod.Should().Be(1);
            actual.AcademicYear.Should().Be(2021);
            actual.Ukprn.Should().Be(-1000);
            actual.Uln.Should().Be(-2000);
            actual.Training.Should().HaveCount(1);

            var training = actual.Training.First();
            training.Reference.Should().Be("ZPROG001");
            training.ProgrammeType.Should().Be(100);
            training.StandardCode.Should().Be(200);
            training.FrameworkCode.Should().Be(300);
            training.PathwayCode.Should().Be(400);
            training.FundingLineType.Should().Be("funding");
            training.StartDate.Should().Be(new DateTime(2020, 10, 9));
            training.PriceEpisodes.Should().HaveCount(1);

            var priceEpisode = training.PriceEpisodes.First();
            priceEpisode.Identifier.Should().Be("TEST");
            priceEpisode.AgreedPrice.Should().Be(3000);
            priceEpisode.StartDate.Should().Be(new DateTime(2020, 10, 7));
            priceEpisode.EndDate.Should().Be(new DateTime(2020, 10, 12));
            priceEpisode.NumberOfInstalments.Should().Be(12);
            priceEpisode.InstalmentAmount.Should().Be(50);
            priceEpisode.CompletionAmount.Should().Be(550);
            priceEpisode.Periods.Should().HaveCount(6);

            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 1,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 2,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 3,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 3,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte>{1, 2, 3},
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 4,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte>{7},
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 5,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte>{9},
            });
        }
    }
}
