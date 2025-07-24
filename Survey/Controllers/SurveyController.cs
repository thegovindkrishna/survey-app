using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Services;
using System.Security.Claims;
using Survey.Models.Dtos;
using SurveyModel = Survey.Models.Survey;

namespace Survey.Controllers
{
    /// <summary>
    /// Controller for managing surveys.
    /// Provides CRUD operations for surveys. All endpoints require Admin role authorization.
    /// </summary>
    [ApiController]
    [Route("api/surveys")]
    [Authorize(Roles = "Admin")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        /// <summary>
        /// Initializes a new instance of the SurveyController with the specified survey service.
        /// </summary>
        /// <param name="surveyService">The service for handling survey operations</param>
        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        /// <summary>
        /// Creates a new survey in the system.
        /// Sets the CreatedBy property to the authenticated admin's email.
        /// </summary>
        /// <param name="surveyDto">The survey data transfer object</param>
        /// <returns>
        /// 201 Created with the created survey if successful,
        /// 400 Bad Request if input is invalid
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SurveyCreateDto surveyDto)
        {
            var email = User.FindFirstValue(ClaimTypes.Name)!;
            var createdSurvey = await _surveyService.Create(surveyDto, email);
            return CreatedAtAction(nameof(GetById), new { id = createdSurvey.Id }, createdSurvey);
        }

        /// <summary>
        /// Retrieves all surveys from the system with their questions included.
        /// </summary>
        /// <returns>200 OK with a collection of all surveys</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var surveys = await _surveyService.GetAll();
            return Ok(surveys);
        }

        /// <summary>
        /// Retrieves a specific survey by its ID with questions included.
        /// </summary>
        /// <param name="id">The unique identifier of the survey</param>
        /// <returns>
        /// 200 OK with the survey if found,
        /// 404 Not Found if survey doesn't exist
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var survey = await _surveyService.GetById(id);
            return survey != null ? Ok(survey) : NotFound();
        }

        /// <summary>
        /// Updates a survey's properties and questions.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to update</param>
        /// <param name="surveyDto">The updated survey data</param>
        /// <returns>
        /// 200 OK with the updated survey if successful,
        /// 404 Not Found if survey doesn't exist
        /// </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SurveyUpdateDto surveyDto)
        {
            var updatedSurvey = await _surveyService.Update(id, surveyDto);
            return updatedSurvey != null ? Ok(updatedSurvey) : NotFound();
        }

        /// <summary>
        /// Deletes a survey and all its associated questions from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to delete</param>
        /// <returns>
        /// 200 OK with success message if deleted,
        /// 404 Not Found if survey doesn't exist

        
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _surveyService.Delete(id);
            return success ? Ok(new { message = "Deleted successfully." }) : NotFound(new { message = "Survey not found." });
        }
    }
}
