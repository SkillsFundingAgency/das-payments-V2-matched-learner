using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MatchedLearnerApi.Application.Repositories;
using MatchedLearnerApi.Types;
using Microsoft.AspNetCore.Mvc;

namespace MatchedLearnerApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1")]
    [ApiController]
    public class MatchedLearnerController : ControllerBase
    {
        private readonly IEmployerIncentivesRepository _employerIncentivesRepository;

        public MatchedLearnerController(IEmployerIncentivesRepository employerIncentivesRepository)
        {
            _employerIncentivesRepository = employerIncentivesRepository ?? throw new ArgumentNullException(nameof(employerIncentivesRepository));
        }

        /// <summary>
        /// Gets the learner that matches the given ukprn and uln and returns data lock information about that learner
        /// </summary>
        /// <returns>Data lock information about the matching learner</returns>
        /// <response code="200">Matching learner found</response>
        /// <response code="404">Matching learner not found</response>
        /// <response code="401">The client is not authorized to access this endpoint</response>
        [ProducesResponseType(typeof(MatchedLearnerResultDto),200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [HttpGet()]
        [Route("{ukprn}/{uln}")]
        public async Task<ActionResult> Get(long ukprn, long uln)
        {
            var result = await _employerIncentivesRepository.MatchedLearner(ukprn, uln);
            //var result = new MatchedLearnerResultDto
            //{
            //    Training = new List<TrainingDto>
            //    {
            //        new TrainingDto
            //        {
            //            PriceEpisodes = new List<PriceEpisodeDto>
            //            {
            //                new PriceEpisodeDto
            //                {
            //                    Periods = new List<PeriodDto>
            //                    {
            //                        new PeriodDto
            //                        {
            //                            DataLockFailures = new List<int>
            //                            {
            //                                1, 7
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //};
            return Ok(result);
        }
    }
}