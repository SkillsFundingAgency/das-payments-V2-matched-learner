using AutoFixture;
using MatchedLearnerApi.Application.Repositories;
using MatchedLearnerApi.Controllers;
using MatchedLearnerApi.Types;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MatchedLearnerApi.Application.Tests.ControllerTests.MatchedLearnerControllerTests
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
        private readonly Mock<IEmployerIncentivesRepository> _mockRepository;
        private readonly MatchedLearnerController _sut;
        private readonly long _ukprn;
        private readonly long _uln;
        private MatchedLearnerResultDto _result;

        public WhenGettingMatchedLearnerFixture()
        {
            _fixture = new Fixture();
            new SupportMutableValueTypesCustomization().Customize(_fixture);

            _ukprn = _fixture.Create<long>();
            _uln = _fixture.Create<long>();
            _mockRepository = _fixture.Create<Mock<IEmployerIncentivesRepository>>();
            _sut = new MatchedLearnerController(_mockRepository.Object);
        }

        public async Task<IActionResult> Act() => await _sut.Get(_ukprn, _uln);

        public WhenGettingMatchedLearnerFixture WithRepositoryReturningNull()
        {
            _result = null;

            _mockRepository
                .Setup(x => x.GetMatchedLearnerResults(_ukprn, _uln))
                .ReturnsAsync(_result);

            return this;
        }

        public WhenGettingMatchedLearnerFixture WithRepositoryReturningResult()
        {
            _result = _fixture.Create<MatchedLearnerResultDto>();

            _mockRepository
                .Setup(x => x.GetMatchedLearnerResults(_ukprn, _uln))
                .ReturnsAsync(_result);

            return this;
        }

        public void Assert_MatchedLearner_IsReturnedInResult<T>(T resultObject)
        {
            Assert.True(resultObject.Equals(_result));
        }
    }
}