using Xunit;
using Survey.Services;
using Survey.Data;
using Survey.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Survey.Repositories;
using Microsoft.Extensions.Logging;
using Survey.Models.Dtos;

namespace Survey.Tests
{
    /// <summary>
    /// Unit tests for SurveyService covering survey CRUD, question management, and response handling.
    /// Uses in-memory database for isolation.
    /// </summary>
    public class SurveyServiceTests
    {
        private readonly SurveyService _service;
        private readonly AppDbContext _context;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ISurveyRepository> _mockSurveyRepository;
        private readonly Mock<ILogger<SurveyService>> _mockLogger;

        public SurveyServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSurveyRepository = new Mock<ISurveyRepository>();
            _mockLogger = new Mock<ILogger<SurveyService>>();
            _mockUnitOfWork.Setup(uow => uow.Surveys).Returns(_mockSurveyRepository.Object);

            _service = new SurveyService(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Verifies that creating a survey adds it to the database and sets the CreatedBy field.
        /// </summary>
        [Fact]
        public async Task Create_AddsSurvey()
        {
            var surveyDto = new SurveyCreateDto("Test", "Desc", DateTime.Now, DateTime.Now.AddDays(7), new List<QuestionCreateDto>());
            var result = await _service.Create(surveyDto, "admin@example.com");
            Assert.Equal("admin@example.com", result.CreatedBy);
            // Since we are mocking the UnitOfWork, we can't assert on _context.Surveys.Count() directly.
            // We should verify that the AddAsync and CompleteAsync methods were called.
            _mockUnitOfWork.Verify(uow => uow.Surveys.AddAsync(It.IsAny<Survey.Models.Survey>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        /// <summary>
        /// Ensures GetAll returns all surveys in the database.
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsAllSurveys()
        {
            var surveys = new List<Survey.Models.Survey>
            {
                new Survey.Models.Survey { Id = 1, Title = "A" },
                new Survey.Models.Survey { Id = 2, Title = "B" }
            };
            _mockSurveyRepository.Setup(r => r.GetAllWithQuestionsAsync()).ReturnsAsync(surveys);

            var all = await _service.GetAll();
            Assert.Equal(2, all.Count());
        }

        /// <summary>
        /// Ensures GetById returns a survey when it exists.
        /// </summary>
        [Fact]
        public async Task GetById_ReturnsSurvey_WhenExists()
        {
            var survey = new Survey.Models.Survey { Id = 1, Title = "A" };
            _mockSurveyRepository.Setup(r => r.GetByIdWithQuestionsAsync(1)).ReturnsAsync(survey);

            var found = await _service.GetById(1);
            Assert.NotNull(found);
            Assert.Equal(survey.Id, found.Id);
        }

        /// <summary>
        /// Ensures GetById returns null when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task GetById_ReturnsNull_WhenMissing()
        {
            var found = await _service.GetById(999);
            Assert.Null(found);
        }

        /// <summary>
        /// Ensures Update modifies an existing survey's properties.
        /// </summary>
        [Fact]
        public async Task Update_UpdatesSurvey_WhenExists()
        {
            var survey = new Survey.Models.Survey { Id = 1, Title = "A", Description = "D", Questions = new List<Question>() };
            var updateDto = new SurveyUpdateDto("B", "D", DateTime.Now, DateTime.Now.AddDays(1), new List<QuestionUpdateDto>());

            _mockSurveyRepository.Setup(r => r.GetByIdWithQuestionsAsync(survey.Id)).ReturnsAsync(survey);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).Returns(Task.CompletedTask);

            var updated = await _service.Update(survey.Id, updateDto);
            Assert.Equal("B", updated.Title);
        }

        /// <summary>
        /// Ensures Update returns null when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task Update_ReturnsNull_WhenMissing()
        {
            var updateDto = new SurveyUpdateDto("B", "D", DateTime.Now, DateTime.Now.AddDays(1), new List<QuestionUpdateDto>());
            _mockSurveyRepository.Setup(r => r.GetByIdWithQuestionsAsync(999)).ReturnsAsync((Survey.Models.Survey?)null);

            var updated = await _service.Update(999, updateDto);
            Assert.Null(updated);
        }

        /// <summary>
        /// Ensures UpdateProperties updates only the basic fields of a survey.
        /// </summary>
        [Fact]
        public async Task UpdateProperties_UpdatesFields_WhenExists()
        {
            var survey = new Survey.Models.Survey { Title = "A", Description = "D" };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var update = new Survey.Models.Survey { Title = "B", Description = "E" };
            var updated = await _service.UpdateProperties(survey.Id, update);
            Assert.Equal("B", updated.Title);
            Assert.Equal("E", updated.Description);
        }

        /// <summary>
        /// Ensures UpdateProperties returns null when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateProperties_ReturnsNull_WhenMissing()
        {
            var update = new Survey.Models.Survey { Title = "B" };
            var updated = await _service.UpdateProperties(999, update);
            Assert.Null(updated);
        }

        /// <summary>
        /// Ensures Delete removes a survey from the database when it exists.
        /// </summary>
        [Fact]
        public async Task Delete_RemovesSurvey_WhenExists()
        {
            var survey = new Survey.Models.Survey { Title = "A" };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var result = await _service.Delete(survey.Id);
            Assert.True(result);
            Assert.Empty(_context.Surveys);
        }

        /// <summary>
        /// Ensures Delete returns false when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task Delete_ReturnsFalse_WhenMissing()
        {
            var result = await _service.Delete(999);
            Assert.False(result);
        }

        /// <summary>
        /// Ensures SubmitResponse saves a valid response to the database.
        /// </summary>
        [Fact]
        public async Task SubmitResponse_Saves_WhenValid()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { Id = 1, QuestionText = "Q", required = true } } };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var response = new SurveyResponse { SurveyId = survey.Id, responses = new List<QuestionResponse> { new QuestionResponse { QuestionId = 1, response = "A" } } };
            var saved = await _service.SubmitResponse(response);
            Assert.Equal(response, saved);
            Assert.Single(_context.SurveyResponses);
        }

        /// <summary>
        /// Ensures SubmitResponse throws when required questions are missing.
        /// </summary>
        [Fact]
        public async Task SubmitResponse_Throws_WhenMissingRequired()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { Id = 1, QuestionText = "Q", required = true } } };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var response = new SurveyResponse { SurveyId = survey.Id, responses = new List<QuestionResponse>() };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.SubmitResponse(response));
        }

        /// <summary>
        /// Ensures SubmitResponse throws when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task SubmitResponse_Throws_WhenSurveyNotFound()
        {
            var response = new SurveyResponse { SurveyId = 999, responses = new List<QuestionResponse>() };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.SubmitResponse(response));
        }

        /// <summary>
        /// Ensures GetResponses returns all responses for a survey.
        /// </summary>
        [Fact]
        public async Task GetResponses_ReturnsResponses_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Title = "A" };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            _context.SurveyResponses.Add(new SurveyResponse { SurveyId = survey.Id, RespondentEmail = "a" });
            await _context.SaveChangesAsync();
            var responses = await _service.GetResponses(survey.Id);
            Assert.Single(responses);
        }

        /// <summary>
        /// Ensures GetResponses throws when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task GetResponses_Throws_WhenSurveyNotFound()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetResponses(999));
        }

        /// <summary>
        /// Ensures GetResponse returns a response when found.
        /// </summary>
        [Fact]
        public async Task GetResponse_ReturnsResponse_WhenFound()
        {
            var survey = new Survey.Models.Survey { Title = "A" };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var resp = new SurveyResponse { SurveyId = survey.Id, RespondentEmail = "a" };
            _context.SurveyResponses.Add(resp);
            await _context.SaveChangesAsync();
            var found = await _service.GetResponse(survey.Id, resp.Id);
            Assert.NotNull(found);
        }

        /// <summary>
        /// Ensures GetResponse returns null when the response is not found.
        /// </summary>
        [Fact]
        public async Task GetResponse_ReturnsNull_WhenNotFound()
        {
            var survey = new Survey.Models.Survey { Title = "A" };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var found = await _service.GetResponse(survey.Id, 999);
            Assert.Null(found);
        }

        /// <summary>
        /// Ensures GetResponse throws when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task GetResponse_Throws_WhenSurveyNotFound()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetResponse(999, 1));
        }

        /// <summary>
        /// Ensures AddQuestion adds a question to a survey.
        /// </summary>
        [Fact]
        public async Task AddQuestion_Adds_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Title = "A" };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var question = new Question { QuestionText = "Q", type = "text" };
            var updated = await _service.AddQuestion(survey.Id, question);
            Assert.Single(updated.Questions);
        }

        /// <summary>
        /// Ensures AddQuestion returns null when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task AddQuestion_ReturnsNull_WhenSurveyNotFound()
        {
            var question = new Question { QuestionText = "Q", type = "text" };
            var updated = await _service.AddQuestion(999, question);
            Assert.Null(updated);
        }

        /// <summary>
        /// Ensures UpdateQuestion updates a question when both survey and question exist.
        /// </summary>
        [Fact]
        public async Task UpdateQuestion_Updates_WhenFound()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { Id = 1, QuestionText = "Q" } } };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var updatedQ = new Question { Id = 1, QuestionText = "Q2", type = "text" };
            var updated = await _service.UpdateQuestion(survey.Id, 1, updatedQ);
            Assert.Equal("Q2", updated.Questions.First().QuestionText);
        }

        /// <summary>
        /// Ensures UpdateQuestion returns null when survey or question is not found.
        /// </summary>
        [Fact]
        public async Task UpdateQuestion_ReturnsNull_WhenSurveyOrQuestionNotFound()
        {
            var survey = new Survey.Models.Survey { Title = "A" };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var updatedQ = new Question { Id = 1, QuestionText = "Q2", type = "text" };
            var updated = await _service.UpdateQuestion(survey.Id, 1, updatedQ);
            Assert.Null(updated);
        }

        /// <summary>
        /// Ensures DeleteQuestion removes a question when found.
        /// </summary>
        [Fact]
        public async Task DeleteQuestion_Removes_WhenFound()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { Id = 1, QuestionText = "Q" } } };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var updated = await _service.DeleteQuestion(survey.Id, 1);
            Assert.Empty(updated.Questions);
        }

        /// <summary>
        /// Ensures DeleteQuestion returns null when survey or question is not found.
        /// </summary>
        [Fact]
        public async Task DeleteQuestion_ReturnsNull_WhenSurveyOrQuestionNotFound()
        {
            var survey = new Survey.Models.Survey { Title = "A" };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            _mockSurveyRepository.Setup(r => r.GetByIdWithQuestionsAsync(survey.Id)).ReturnsAsync(survey);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).Returns(Task.CompletedTask);

            var updated = await _service.DeleteQuestion(survey.Id, 1);
            Assert.Null(updated);
        }
    }
} 