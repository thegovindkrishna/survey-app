using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Models.Dtos;
using Survey.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using SurveyModel = Survey.Models.SurveyModel;

namespace Survey.Controllers
{
    /// <summary>
    /// Controller for managing questions within surveys.
    /// Provides CRUD operations for questions. All endpoints require Admin role authorization.
    /// </summary>
    [ApiController]
    [Route("api/surveys/{surveyId}/questions")]
    [Authorize(Roles = "Admin")]
    public class QuestionController : ControllerBase
    {
        private readonly ISurveyService _surveyService;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestionController> _logger;

        /// <summary>
        /// Initializes a new instance of the QuestionController with the specified survey service.
        /// </summary>
        /// <param name="surveyService">The service for handling survey and question operations</param>
        /// <param name="mapper">The AutoMapper instance for object mapping</param>
        /// <param name="logger">The logger instance</param>
        public QuestionController(ISurveyService surveyService, IMapper mapper, ILogger<QuestionController> logger)
        {
            _surveyService = surveyService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new question to an existing survey.
        /// Sets the SurveyId on the question and adds it to the survey's question collection.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="question">The question object to add</param>
        /// <returns>
        /// 200 OK with the updated survey if successful,
        /// 404 Not Found if survey doesn't exist
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> AddQuestion(int surveyId, [FromBody] QuestionCreateDto questionDto)
        {
            _logger.LogInformation("Attempting to add question to survey {SurveyId}", surveyId);
            var updatedSurvey = await _surveyService.AddQuestion(surveyId, questionDto);
            if (updatedSurvey == null)
            {
                _logger.LogWarning("Add question failed: Survey {SurveyId} not found", surveyId);
                return NotFound("Survey not found");
            }

            _logger.LogInformation("Question added successfully to survey {SurveyId}", surveyId);
            return Ok(_mapper.Map<SurveyDto>(updatedSurvey));
        }

        /// <summary>
        /// Retrieves all questions for a specific survey.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>
        /// 200 OK with the list of questions if survey exists,
        /// 404 Not Found if survey doesn't exist
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetQuestions(int surveyId)
        {
            _logger.LogInformation("Attempting to retrieve questions for survey {SurveyId}", surveyId);
            var survey = await _surveyService.GetById(surveyId);
            if (survey == null)
            {
                _logger.LogWarning("Get questions failed: Survey {SurveyId} not found", surveyId);
                return NotFound("Survey not found");
            }

            _logger.LogInformation("Questions retrieved successfully for survey {SurveyId}", surveyId);
            return Ok(_mapper.Map<IEnumerable<QuestionDto>>(survey.Questions));
        }

        /// <summary>
        /// Updates a specific question within a survey.
        /// Modifies the existing question's properties without affecting other questions.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="questionId">The unique identifier of the question to update</param>
        /// <param name="question">The updated question data</param>
        /// <returns>
        /// 200 OK with the updated survey if successful,
        /// 404 Not Found if survey or question doesn't exist
        /// </returns>
        [HttpPost("{questionId}")]
        public async Task<IActionResult> UpdateQuestion(int surveyId, int questionId, [FromBody] QuestionUpdateDto questionDto)
        {
            _logger.LogInformation("Attempting to update question {QuestionId} in survey {SurveyId}", questionId, surveyId);
            var updatedSurvey = await _surveyService.UpdateQuestion(surveyId, questionId, questionDto);
            if (updatedSurvey == null)
            {
                _logger.LogWarning("Update question failed: Survey {SurveyId} or question {QuestionId} not found", surveyId, questionId);
                return NotFound("Survey or question not found");
            }

            _logger.LogInformation("Question {QuestionId} updated successfully in survey {SurveyId}", questionId, surveyId);
            return Ok(_mapper.Map<SurveyDto>(updatedSurvey));
        }

        /// <summary>
        /// Removes a specific question from a survey.
        /// Deletes the question from the survey's question collection.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="questionId">The unique identifier of the question to delete</param>
        /// <returns>
        /// 200 OK with the updated survey if successful,
        /// 404 Not Found if survey or question doesn't exist
        /// </returns>
        [HttpDelete("{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int surveyId, int questionId)
        {
            _logger.LogInformation("Attempting to delete question {QuestionId} from survey {SurveyId}", questionId, surveyId);
            var updatedSurvey = await _surveyService.DeleteQuestion(surveyId, questionId);
            if (updatedSurvey == null)
            {
                _logger.LogWarning("Delete question failed: Survey {SurveyId} or question {QuestionId} not found", surveyId, questionId);
                return NotFound("Survey or question not found");
            }

            _logger.LogInformation("Question {QuestionId} deleted successfully from survey {SurveyId}", questionId, surveyId);
            return Ok(_mapper.Map<SurveyDto>(updatedSurvey));
        }
    }
} 