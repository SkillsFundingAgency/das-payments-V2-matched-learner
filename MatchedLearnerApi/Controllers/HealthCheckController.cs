using Microsoft.AspNetCore.Mvc;

namespace MatchedLearnerApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        /// <summary>
        /// Gets a status code to indicate the health of the application
        /// </summary>
        /// <returns>A status code to indicate the health of the application</returns>
        /// <response code="200">Health check successful</response>
        /// <response code="401">The client is not authorized to access this endpoint</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [HttpGet()]
        public ActionResult Get()
        {
            return StatusCode(200);
        }
    }
}