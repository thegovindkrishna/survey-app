        using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Services;
using System.Security.Claims;

namespace Survey.Controllers
{
    /// <summary>
    /// Controller for managing survey responses.
    /// Provides endpoints for submitting responses and retrieving response data.
    /// </summary>
    [ApiController]
    [Route("api/surveys/{surveyId}/responses")]
    public class ResponseController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        /// <summary>
        /// Initializes a new instance of the ResponseController with the specified survey service.
        /// </summary>
        /// <param name="surveyService">The service for handling survey and response operations</param>
        public ResponseController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        /// <summary>
        /// Submits a survey response from an authenticated user.
        /// Validates that all required questions are answered before saving the response.
        /// Sets the respondent email and submission date automatically.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="response">The survey response object containing answers</param>
        /// <returns>
        /// 200 OK with the submitted response if successful,
        /// 400 Bad Request if validation fails or required questions are missing
        /// </returns>
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
        /// Retrieves all responses for a specific survey.
        /// Requires Admin role authorization.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>
        /// 200 OK with all responses for the survey if successful,
        /// 400 Bad Request if survey doesn't exist
        /// </returns>
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
        /// Retrieves a specific survey response by its ID.
        /// Requires Admin role authorization.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="responseId">The unique identifier of the response</param>
        /// <returns>
        /// 200 OK with the specific response if found,
        /// 404 Not Found if response doesn't exist,
        /// 400 Bad Request if survey doesn't exist
        /// </returns>
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