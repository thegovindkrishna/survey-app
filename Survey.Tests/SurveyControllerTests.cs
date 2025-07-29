using Xunit;
using Moq;
using Survey.Controllers;
using Survey.Services;
using Survey.Models;
using Survey.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Survey.Tests
{
    public class SurveyControllerTests
    {
        private readonly Mock<ISurveyService> _mockSurveyService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<SurveyController>> _mockLogger;
        private readonly SurveyController _controller;

        public SurveyControllerTests()
        {
            _mockSurveyService = new Mock<ISurveyService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<SurveyController>>();
            _controller = new SurveyController(_mockSurveyService.Object, _mockMapper.Object, _mockLogger.Object);

            // Setup dummy admin user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "admin@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Create_ReturnsOk_WhenSurveyCreated()
        {
            var createDto = new SurveyCreateDto("Survey", "Test", DateTime.Now, DateTime.Now.AddDays(7), new List<QuestionCreateDto>());
            var surveyDto = new SurveyDto(1, "Survey", "Test", DateTime.Now, DateTime.Now.AddDays(7), "admin@example.com", null, new List<QuestionDto>());

            _mockSurveyService.Setup(s => s.Create(createDto, "admin@example.com")).ReturnsAsync(surveyDto);
            _mockMapper.Setup(m => m.Map<SurveyDto>(It.IsAny<Survey.Models.Survey>())).Returns(surveyDto);

            var result = await _controller.Create(createDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(surveyDto, okResult.Value);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithSurveys()
        {
            var surveyDtos = new List<SurveyDto>
            {
                new SurveyDto(1, "Survey1", "Desc1", DateTime.Now, DateTime.Now.AddDays(1), "user1", null, new List<QuestionDto>()),
                new SurveyDto(2, "Survey2", "Desc2", DateTime.Now, DateTime.Now.AddDays(2), "user2", null, new List<QuestionDto>())
            };

            _mockSurveyService.Setup(s => s.GetAll()).ReturnsAsync(surveyDtos);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(surveyDtos, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenSurveyExists()
        {
            var surveyDto = new SurveyDto(1, "Survey", "Desc", DateTime.Now, DateTime.Now.AddDays(1), "user", null, new List<QuestionDto>());
            _mockSurveyService.Setup(s => s.GetById(1)).ReturnsAsync(surveyDto);

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(surveyDto, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenSurveyMissing()
        {
            _mockSurveyService.Setup(s => s.GetById(999)).ReturnsAsync((SurveyDto?)null);

            var result = await _controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenSuccessful()
        {
            var updateDto = new SurveyUpdateDto("Updated Survey", "Updated Desc", DateTime.Now, DateTime.Now.AddDays(10), new List<QuestionUpdateDto>());
            var surveyDto = new SurveyDto(1, "Updated Survey", "Updated Desc", DateTime.Now, DateTime.Now.AddDays(10), "admin@example.com", null, new List<QuestionDto>());

            _mockSurveyService.Setup(s => s.Update(1, updateDto)).ReturnsAsync(surveyDto);

            var result = await _controller.Update(1, updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(surveyDto, okResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenSurveyMissing()
        {
            var updateDto = new SurveyUpdateDto("Updated Survey", "Updated Desc", DateTime.Now, DateTime.Now.AddDays(10), new List<QuestionUpdateDto>());
            _mockSurveyService.Setup(s => s.Update(1, updateDto)).ReturnsAsync((SurveyDto?)null);

            var result = await _controller.Update(1, updateDto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenSuccessful()
        {
            _mockSurveyService.Setup(s => s.Delete(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Deleted successfully.", okResult.Value);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenSurveyMissing()
        {
            _mockSurveyService.Setup(s => s.Delete(999)).ReturnsAsync(false);

            var result = await _controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
