        using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Models.Dtos;
using Survey.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

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
        private readonly IMapper _mapper;
        private readonly ILogger<ResponseController> _logger;

        /// <summary>
        /// Initializes a new instance of the ResponseController with the specified survey service.
        /// </summary>
        /// <param name="surveyService">The service for handling survey and response operations</param>
        /// <param name="mapper">The AutoMapper instance for object mapping</param>
        /// <param name="logger">The logger instance</param>
        public ResponseController(ISurveyService surveyService, IMapper mapper, ILogger<ResponseController> logger)
        {
            _surveyService = surveyService;
            _mapper = mapper;
            _logger = logger;
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
        public async Task<IActionResult> SubmitResponse(int surveyId, [FromBody] SubmitResponseDto submitResponseDto)
        {
            _logger.LogInformation("Attempting to submit response for survey {SurveyId}", surveyId);
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Name)!;
                var surveyResponse = _mapper.Map<SurveyResponseModel>(submitResponseDto);
                surveyResponse.RespondentEmail = email;
                surveyResponse.SubmissionDate = DateTime.UtcNow;
                surveyResponse.SurveyId = surveyId;

                var submittedResponse = await _surveyService.SubmitResponse(surveyResponse);
                _logger.LogInformation("Response submitted successfully for survey {SurveyId} by user {Email}", surveyId, email);
                return Ok(_mapper.Map<SurveyResponseDto>(submittedResponse));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to submit response for survey {SurveyId}: {ErrorMessage}", surveyId, ex.Message);
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
            _logger.LogInformation("Attempting to retrieve all responses for survey {SurveyId}", surveyId);
            try
            {
                var responses = await _surveyService.GetResponses(surveyId);
                var responseDtos = _mapper.Map<IEnumerable<SurveyResponseDto>>(responses);
                _logger.LogInformation("Successfully retrieved {Count} responses for survey {SurveyId}", responseDtos.Count(), surveyId);
                
                return Ok(responseDtos);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to retrieve responses for survey {SurveyId}: {ErrorMessage}", surveyId, ex.Message);
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
            _logger.LogInformation("Attempting to retrieve response {ResponseId} for survey {SurveyId}", responseId, surveyId);
            try
            {
                var response = await _surveyService.GetResponse(surveyId, responseId);
                if (response == null)
                {
                    _logger.LogWarning("Response {ResponseId} not found for survey {SurveyId}", responseId, surveyId);
                    return NotFound();
                }
                var responseDto = _mapper.Map<SurveyResponseDto>(response);
                _logger.LogInformation("Successfully retrieved response {ResponseId} for survey {SurveyId}", responseId, surveyId);
                
                return Ok(responseDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to retrieve response {ResponseId} for survey {SurveyId}: {ErrorMessage}", responseId, surveyId, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
} 