using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Services;
using System.Security.Claims;

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

        /// <summary>
        /// Initializes a new instance of the QuestionController with the specified survey service.
        /// </summary>
        /// <param name="surveyService">The service for handling survey and question operations</param>
        public QuestionController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
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
        public async Task<IActionResult> AddQuestion(int surveyId, [FromBody] Question question)
        {
            var updatedSurvey = await _surveyService.AddQuestion(surveyId, question);
            if (updatedSurvey == null)
                return NotFound("Survey not found");

            return Ok(updatedSurvey);
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
            var survey = await _surveyService.GetById(surveyId);
            if (survey == null)
                return NotFound("Survey not found");

            return Ok(survey.Questions);
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
        public async Task<IActionResult> UpdateQuestion(int surveyId, int questionId, [FromBody] Question question)
        {
            var updatedSurvey = await _surveyService.UpdateQuestion(surveyId, questionId, question);
            if (updatedSurvey == null)
                return NotFound("Survey or question not found");

            return Ok(updatedSurvey);
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
            var updatedSurvey = await _surveyService.DeleteQuestion(surveyId, questionId);
            if (updatedSurvey == null)
                return NotFound("Survey or question not found");

            return Ok(updatedSurvey);
        }
    }
} 