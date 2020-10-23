using System;
using System.Threading.Tasks;
using MatchedLearnerApi.Application;
using MatchedLearnerApi.Types;
using Microsoft.AspNetCore.Mvc;

namespace MatchedLearnerApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1")]
    [ApiController]
    public class MatchedLearnerController : ControllerBase
    {
        private readonly IMatchedLearnerService _matchedLearnerService;

        public MatchedLearnerController(IMatchedLearnerService matchedLearnerService)
        {
            _matchedLearnerService = matchedLearnerService ?? throw new ArgumentNullException(nameof(matchedLearnerService));
        }

        /// <summary>
        /// Gets the learner that matches the given ukprn and uln and returns data lock information about that learner
        /// </summary>
        /// <returns>Data lock information about the matching learner</returns>
        /// <response code="200">Matching learner found</response>
        /// <response code="404">Matching learner not found</response>
        /// <response code="401">The client is not authorized to access this endpoint</response>
        [ProducesResponseType(typeof(MatchedLearnerDto),200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [HttpGet()]
        [Route("{ukprn}/{uln}")]
        public async Task<ActionResult> Get(long ukprn, long uln)
        {
            var result = await _matchedLearnerService.GetMatchedLearner(ukprn, uln);

            if (result == null)
                return NotFound();
            
            return Ok(result);
        }
    }
}