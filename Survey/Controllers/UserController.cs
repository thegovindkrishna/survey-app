using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models.Dtos;
using Survey.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using SurveyModel = Survey.Models.SurveyModel;

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
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Initializes a new instance of the UserController with the specified survey service.
        /// </summary>
        /// <param name="surveyService">The service for handling survey operations</param>
        /// <param name="mapper">The AutoMapper instance for object mapping</param>
        /// <param name="logger">The logger instance</param>
        public UserController(ISurveyService surveyService, IMapper mapper, ILogger<UserController> logger)
        {
            _surveyService = surveyService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all active surveys that users can respond to.
        /// Only returns surveys that are currently active (within date range).
        /// </summary>
        /// <returns>200 OK with a collection of active surveys</returns>
        [HttpGet("surveys")]
        public async Task<IActionResult> GetAvailableSurveys()
        {
            _logger.LogInformation("Retrieving available surveys for user.");
            var allSurveys = await _surveyService.GetAll();
            var now = DateTime.UtcNow;
            
            // Filter for active surveys only
            var activeSurveys = allSurveys.Where(survey => 
                survey.StartDate <= now && survey.EndDate >= now
            ).ToList();

            var activeSurveyDtos = _mapper.Map<IEnumerable<SurveyDto>>(activeSurveys);
            _logger.LogInformation("Retrieved {Count} active surveys.", activeSurveyDtos.Count());

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
            _logger.LogInformation("Attempting to retrieve survey {SurveyId} for user.", id);
            var survey = await _surveyService.GetById(id);
            if (survey == null)
            {
                _logger.LogWarning("Survey {SurveyId} not found for user.", id);
                return NotFound("Survey not found");
            }

            var now = DateTime.UtcNow;
            if (survey.StartDate > now || survey.EndDate < now)
            {
                _logger.LogWarning("Survey {SurveyId} is not currently active.", id);
                return NotFound("Survey is not currently active");
            }

            var surveyDto = _mapper.Map<SurveyDto>(survey);
            _logger.LogInformation("Survey {SurveyId} retrieved successfully for user.", id);

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
            _logger.LogInformation("Attempting to retrieve responses for user {Email}", email);
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("User email not found when attempting to retrieve responses.");
                return BadRequest("User email not found");
            }

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
                        var userResponseDto = _mapper.Map<UserResponseDto>(userResponse);

                        var questionDetails = new List<QuestionResponseDto>();
                        foreach (var response in userResponse.responses)
                        {
                            questionDetails.Add(new QuestionResponseDto { QuestionId = response.QuestionId, Response = response.response });
                        }

                        userResponseDto.SurveyTitle = survey.Title;
                        userResponseDto.SurveyDescription = survey.Description;
                        userResponseDto.Responses = questionDetails;
                        userResponses.Add(userResponseDto);
                        _logger.LogInformation("Found response for survey {SurveyId} by user {Email}", survey.Id, email);
                    }
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "Survey {SurveyId} has no responses or an error occurred while retrieving responses: {ErrorMessage}", survey.Id, ex.Message);
                    // Survey has no responses, continue to next
                    continue;
                }
            }
            _logger.LogInformation("Retrieved {Count} responses for user {Email}", userResponses.Count, email);

            return Ok(userResponses);
        }
    }
} 