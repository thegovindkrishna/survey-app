using Xunit;
using Moq;
using Survey.Controllers;
using Survey.Services;
using Survey.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using SurveyModel = Survey.Models.Survey;

namespace Survey.Tests
{
    public class SurveyControllerTests
    {
        private readonly Mock<ISurveyService> _mockSurveyService;
        private readonly SurveyController _controller;

        public SurveyControllerTests()
        {
            _mockSurveyService = new Mock<ISurveyService>();
            _controller = new SurveyController(_mockSurveyService.Object);

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
            var survey = new SurveyModel { Title = "Survey", Description = "Test" };
            _mockSurveyService.Setup(s => s.Create(survey, "admin@example.com")).ReturnsAsync(survey);

            var result = await _controller.Create(survey);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(survey, okResult.Value);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithSurveys()
        {
            var surveys = new List<SurveyModel>
            {
                new SurveyModel { Id = 1, Title = "Survey1" },
                new SurveyModel { Id = 2, Title = "Survey2" }
            };

            _mockSurveyService.Setup(s => s.GetAll()).ReturnsAsync(surveys);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(surveys, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenSurveyExists()
        {
            var survey = new SurveyModel { Id = 1, Title = "Survey" };
            _mockSurveyService.Setup(s => s.GetById(1)).ReturnsAsync(survey);

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(survey, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenSurveyMissing()
        {
            _mockSurveyService.Setup(s => s.GetById(999)).ReturnsAsync((SurveyModel?)null);

            var result = await _controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenSuccessful()
        {
            var survey = new SurveyModel { Id = 1, Title = "Updated" };
            _mockSurveyService.Setup(s => s.Update(1, survey)).ReturnsAsync(survey);

            var result = await _controller.Update(1, survey);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(survey, okResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenSurveyMissing()
        {
            var survey = new SurveyModel { Id = 1, Title = "Updated" };
            _mockSurveyService.Setup(s => s.Update(1, survey)).ReturnsAsync((SurveyModel?)null);

            var result = await _controller.Update(1, survey);

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
