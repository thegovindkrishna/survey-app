using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Services;
using System.Security.Claims;

namespace Survey.Controllers
{
    [ApiController]
    [Route("api/surveys/{surveyId}/responses")]
    public class ResponseController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public ResponseController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        /// <summary>
        /// Submits a survey response.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <param name="response">The survey response to submit.</param>
        /// <returns>The submitted survey response.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitResponse(int surveyId, [FromBody] SurveyResponse response)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Name)!;
                response.RespondentEmail = email;
                response.SubmissionDate = DateTime.UtcNow;
                response.SurveyId = surveyId;

                var submittedResponse = await _surveyService.SubmitResponse(response);
                return Ok(submittedResponse);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all responses for a survey.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <returns>A collection of survey responses.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetResponses(int surveyId)
        {
            try
            {
                var responses = await _surveyService.GetResponses(surveyId);
                return Ok(responses);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets a specific survey response by its ID.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <param name="responseId">The ID of the response.</param>
        /// <returns>The survey response, or null if not found.</returns>
        [HttpGet("{responseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetResponse(int surveyId, int responseId)
        {
            try
            {
                var response = await _surveyService.GetResponse(surveyId, responseId);
                return response != null ? Ok(response) : NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 