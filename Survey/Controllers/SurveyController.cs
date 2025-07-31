using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Services;
using AutoMapper;
using Survey.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Survey.Repositories;
using System.Security.Claims;
using Asp.Versioning;

namespace Survey.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;
        private readonly IMapper _mapper;
        private readonly ILogger<SurveyController> _logger;

        /// <summary>
        /// Initializes a new instance of the SurveyController with the specified survey service.
        /// </summary>
        /// <param name="surveyService">The service for handling survey operations</param>
        /// <param name="mapper">The AutoMapper instance for object mapping</param>
        /// <param name="logger">The logger instance</param>
        public SurveyController(ISurveyService surveyService, IMapper mapper, ILogger<SurveyController> logger)
        {
            _surveyService = surveyService;
            _mapper = mapper;
            _logger = logger;
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
            _logger.LogInformation("Admin user {Email} attempting to create a new survey.", email);
            var createdSurvey = await _surveyService.Create(surveyDto, email);
            var createdSurveyDto = _mapper.Map<SurveyDto>(createdSurvey);
            _logger.LogInformation("Survey {SurveyId} created successfully by {Email}.", createdSurveyDto.Id, email);
            return CreatedAtAction(nameof(GetById), new { id = createdSurveyDto.Id }, createdSurveyDto);
        }

        /// <summary>
        /// Retrieves all surveys from the system with their questions included.
        /// </summary>
        /// <returns>200 OK with a collection of all surveys</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
        {
            _logger.LogInformation("Retrieving all surveys.");
            var surveys = await _surveyService.GetAll(paginationParams);
            _logger.LogInformation("Retrieved {Count} surveys.", surveys.Count());
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
            _logger.LogInformation("Attempting to retrieve survey {SurveyId}.", id);
            var survey = await _surveyService.GetById(id);
            if (survey == null)
            {
                _logger.LogWarning("Survey {SurveyId} not found.", id);
                return NotFound();
            }
            var surveyDto = _mapper.Map<SurveyDto>(survey);
            _logger.LogInformation("Survey {SurveyId} retrieved successfully.", id);
            return Ok(surveyDto);
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
            _logger.LogInformation("Attempting to update survey {SurveyId}.", id);
            var updatedSurvey = await _surveyService.Update(id, surveyDto);
            if (updatedSurvey == null)
            {
                _logger.LogWarning("Update failed: Survey {SurveyId} not found.", id);
                return NotFound();
            }
            var updatedSurveyDto = _mapper.Map<SurveyDto>(updatedSurvey);
            _logger.LogInformation("Survey {SurveyId} updated successfully.", id);
            return Ok(updatedSurveyDto);
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
            _logger.LogInformation("Attempting to delete survey {SurveyId}.", id);
            var success = await _surveyService.Delete(id);
            if (success)
            {
                _logger.LogInformation("Survey {SurveyId} deleted successfully.", id);
                return Ok(new { message = "Deleted successfully." });
            }
            else
            {
                _logger.LogWarning("Delete failed: Survey {SurveyId} not found.", id);
                return NotFound(new { message = "Survey not found." });
            }
        }
    }
}
