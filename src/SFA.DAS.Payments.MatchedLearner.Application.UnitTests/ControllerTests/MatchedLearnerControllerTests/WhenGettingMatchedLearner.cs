using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Api.Controllers;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.ControllerTests.MatchedLearnerControllerTests
{
    [TestFixture]
    public class WhenGettingMatchedLearner
    {
        private WhenGettingMatchedLearnerFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new WhenGettingMatchedLearnerFixture();
        }

        [Test]
        public async Task AndMatchedLearnerIsNotFound_ThenReturnsNotFound()
        {
            _fixture.WithRepositoryReturningNull();

            var result = await _fixture.Act() as NotFoundResult;

            Assert.NotNull(result);
            Assert.True(result.StatusCode == StatusCodes.Status404NotFound);
        }

        [Test]
        public async Task AndMatchedLearnerIsFound_ThenReturnsOk()
        {
            _fixture.WithRepositoryReturningResult();

            var result = await _fixture.Act() as OkObjectResult;

            Assert.NotNull(result);
            Assert.True(result.StatusCode == StatusCodes.Status200OK);
            _fixture.Assert_MatchedLearner_IsReturnedInResult(result.Value);
            
        }
    }

    public class WhenGettingMatchedLearnerFixture
    {
        private readonly Fixture _fixture;
        private readonly Mock<IMatchedLearnerService> _mockService;
        private readonly MatchedLearnerController _sut;
        private readonly long _ukprn;
        private readonly long _uln;
        private MatchedLearnerDto _result;

        public WhenGettingMatchedLearnerFixture()
        {
            _fixture = new Fixture();
            new SupportMutableValueTypesCustomization().Customize(_fixture);

            _ukprn = _fixture.Create<long>();
            _uln = _fixture.Create<long>();
            _mockService = _fixture.Create<Mock<IMatchedLearnerService>>();
            _sut = new MatchedLearnerController(_mockService.Object);
        }

        public async Task<IActionResult> Act() => await _sut.Get(_ukprn, _uln);

        public WhenGettingMatchedLearnerFixture WithRepositoryReturningNull()
        {
            _result = null;

            _mockService
                .Setup(x => x.GetMatchedLearner(_ukprn, _uln))
                .ReturnsAsync(_result);

            return this;
        }

        public WhenGettingMatchedLearnerFixture WithRepositoryReturningResult()
        {
            _result = _fixture.Create<MatchedLearnerDto>();

            _mockService
                .Setup(x => x.GetMatchedLearner(_ukprn, _uln))
                .ReturnsAsync(_result);

            return this;
        }

        public void Assert_MatchedLearner_IsReturnedInResult<T>(T resultObject)
        {
            Assert.True(resultObject.Equals(_result));
        }
    }
}