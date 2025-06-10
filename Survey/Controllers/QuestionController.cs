using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Services;
using System.Security.Claims;

namespace Survey.Controllers
{
    [ApiController]
    [Route("api/surveys/{surveyId}/questions")]
    [Authorize(Roles = "Admin")]
    public class QuestionController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public QuestionController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        /// <summary>
        /// Adds a new question to a survey.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <param name="question">The question to add.</param>
        /// <returns>The updated survey.</returns>
        [HttpPost]
        public async Task<IActionResult> AddQuestion(int surveyId, [FromBody] Question question)
        {
            var survey = await _surveyService.GetById(surveyId);
            if (survey == null)
                return NotFound("Survey not found");

            survey.Questions.Add(question);
            var updatedSurvey = await _surveyService.Update(surveyId, survey);
            return Ok(updatedSurvey);
        }

        /// <summary>
        /// Gets all questions for a survey.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <returns>A list of questions.</returns>
        [HttpGet]
        public async Task<IActionResult> GetQuestions(int surveyId)
        {
            var survey = await _surveyService.GetById(surveyId);
            if (survey == null)
                return NotFound("Survey not found");

            return Ok(survey.Questions);
        }

        /// <summary>
        /// Updates a question in a survey.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <param name="questionId">The ID of the question.</param>
        /// <param name="question">The updated question.</param>
        /// <returns>The updated survey.</returns>
        [HttpPost("{questionId}")]
        public async Task<IActionResult> UpdateQuestion(int surveyId, int questionId, [FromBody] Question question)
        {
            var survey = await _surveyService.GetById(surveyId);
            if (survey == null)
                return NotFound("Survey not found");

            var existingQuestion = survey.Questions.FirstOrDefault(q => q.Id == questionId);
            if (existingQuestion == null)
                return NotFound("Question not found");

            existingQuestion.QuestionText = question.QuestionText;
            existingQuestion.type = question.type;
            existingQuestion.required = question.required;
            existingQuestion.options = question.options;
            existingQuestion.maxRating = question.maxRating;

            var updatedSurvey = await _surveyService.Update(surveyId, survey);
            return Ok(updatedSurvey);
        }

        /// <summary>
        /// Deletes a question from a survey.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <param name="questionId">The ID of the question.</param>
        /// <returns>The updated survey.</returns>
        [HttpDelete("{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int surveyId, int questionId)
        {
            var survey = await _surveyService.GetById(surveyId);
            if (survey == null)
                return NotFound("Survey not found");

            var question = survey.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null)
                return NotFound("Question not found");

            survey.Questions.Remove(question);
            var updatedSurvey = await _surveyService.Update(surveyId, survey);
            return Ok(updatedSurvey);
        }
    }
} 