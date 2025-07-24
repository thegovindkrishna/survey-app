using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models.Dtos;
using Survey.Services;
using System.Security.Claims;
using SurveyModel = Survey.Models.Survey;

namespace Survey.Controllers
{
    /// <summary>
    /// Controller for user-specific operations.
    /// Provides endpoints for users to view available surveys and their responses.
    /// </summary>
    [ApiController]
    [Route("api/user")]
    [Authorize(Roles = "User")]
    public class UserController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        /// <summary>
        /// Initializes a new instance of the UserController with the specified survey service.
        /// </summary>
        /// <param name="surveyService">The service for handling survey operations</param>
        public UserController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        /// <summary>
        /// Retrieves all active surveys that users can respond to.
        /// Only returns surveys that are currently active (within date range).
        /// </summary>
        /// <returns>200 OK with a collection of active surveys</returns>
        [HttpGet("surveys")]
        public async Task<IActionResult> GetAvailableSurveys()
        {
            var allSurveys = await _surveyService.GetAll();
            var now = DateTime.UtcNow;
            
            // Filter for active surveys only
            var activeSurveys = allSurveys.Where(survey => 
                survey.StartDate <= now && survey.EndDate >= now
            ).ToList();

            var activeSurveyDtos = activeSurveys.Select(s => new SurveyDto(
                s.Id,
                s.Title,
                s.Description,
                s.StartDate,
                s.EndDate,
                s.CreatedBy,
                s.ShareLink,
                s.Questions.Select(q => new QuestionDto(
                    q.Id,
                    q.QuestionText,
                    q.Type,
                    q.Required,
                    q.Options,
                    q.MaxRating
                )).ToList()
            ));

            return Ok(activeSurveyDtos);
        }

        /// <summary>
        /// Retrieves a specific survey by its ID for user viewing.
        /// Only returns surveys that are currently active.
        /// </summary>
        /// <param name="id">The unique identifier of the survey</param>
        /// <returns>
        /// 200 OK with the survey if found and active,
        /// 404 Not Found if survey doesn't exist or is not active
        /// </returns>
        [HttpGet("surveys/{id}")]
        public async Task<IActionResult> GetSurvey(int id)
        {
            var survey = await _surveyService.GetById(id);
            if (survey == null)
                return NotFound("Survey not found");

            var now = DateTime.UtcNow;
            if (survey.StartDate > now || survey.EndDate < now)
                return NotFound("Survey is not currently active");

            var surveyDto = new SurveyDto(
                survey.Id,
                survey.Title,
                survey.Description,
                survey.StartDate,
                survey.EndDate,
                survey.CreatedBy,
                survey.ShareLink,
                survey.Questions.Select(q => new QuestionDto(
                    q.Id,
                    q.QuestionText,
                    q.Type,
                    q.Required,
                    q.Options,
                    q.MaxRating
                )).ToList()
            );

            return Ok(surveyDto);
        }

        /// <summary>
        /// Retrieves responses submitted by the current user.
        /// </summary>
        /// <returns>200 OK with user's responses</returns>
        [HttpGet("responses")]
        public async Task<IActionResult> GetUserResponses()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(email))
                return BadRequest("User email not found");

            // Get all surveys first
            var allSurveys = await _surveyService.GetAll();
            var userResponses = new List<UserResponseDto>();

            // For each survey, check if user has responded
            foreach (var survey in allSurveys)
            {
                try
                {
                    var responses = await _surveyService.GetResponses(survey.Id);
                    var userResponse = responses.FirstOrDefault(r => r.RespondentEmail == email);
                    
                    if (userResponse != null)
                    {
                        userResponses.Add(new UserResponseDto(
                            SurveyId: survey.Id,
                            SurveyTitle: survey.Title,
                            SurveyDescription: survey.Description,
                            SubmissionDate: userResponse.SubmissionDate,
                            ResponseId: userResponse.Id,
                            Responses: userResponse.responses.Select(qr => new QuestionResponseDto(
                                QuestionId: qr.QuestionId,
                                Response: qr.response
                            )).ToList()
                        ));
                    }
                }
                catch (ArgumentException)
                {
                    // Survey has no responses, continue to next
                    continue;
                }
            }

            return Ok(userResponses);
        }
    }
} 