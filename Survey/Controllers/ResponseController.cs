        using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Models.Dtos;
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
        public async Task<IActionResult> SubmitResponse(int surveyId, [FromBody] SubmitResponseDto submitResponseDto)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Name)!;
                var surveyResponse = new SurveyResponse
                {
                    RespondentEmail = email,
                    SubmissionDate = DateTime.UtcNow,
                    SurveyId = surveyId,
                    responses = submitResponseDto.Responses.Select(r => new QuestionResponse
                    {
                        QuestionId = r.QuestionId,
                        response = r.Response
                    }).ToList()
                };

                var submittedResponse = await _surveyService.SubmitResponse(surveyResponse);
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
                var responseDtos = responses.Select(r => new SurveyResponseDto(
                    Id: r.Id,
                    SurveyId: r.SurveyId,
                    RespondentEmail: r.RespondentEmail,
                    SubmissionDate: r.SubmissionDate,
                    Responses: r.responses.Select(qr => new QuestionResponseDetailDto(
                        QuestionId: qr.QuestionId,
                        Response: qr.response,
                        QuestionText: _surveyService.GetQuestionText(qr.QuestionId)
                    )).ToList()
                )).ToList();
                return Ok(responseDtos);
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
                if (response == null)
                {
                    return NotFound();
                }
                var responseDto = new SurveyResponseDto(
                    Id: response.Id,
                    SurveyId: response.SurveyId,
                    RespondentEmail: response.RespondentEmail,
                    SubmissionDate: response.SubmissionDate,
                    Responses: response.responses.Select(qr => new QuestionResponseDetailDto(
                        QuestionId: qr.QuestionId,
                        Response: qr.response,
                        QuestionText: _surveyService.GetQuestionText(qr.QuestionId)
                    )).ToList()
                );
                return Ok(responseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 