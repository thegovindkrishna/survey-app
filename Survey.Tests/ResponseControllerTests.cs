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

namespace Survey.Tests
{
    public class ResponseControllerTests
    {
        private readonly Mock<ISurveyService> _mockSurveyService;
        private readonly ResponseController _controller;

        public ResponseControllerTests()
        {
            _mockSurveyService = new Mock<ISurveyService>();
            _controller = new ResponseController(_mockSurveyService.Object);
        }

        private void SetUser(string email, string role = "User")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task SubmitResponse_ReturnsOk_WhenSuccessful()
        {
            SetUser("user@example.com");
            var response = new SurveyResponse { SurveyId = 1, responses = new List<QuestionResponse>() };
            _mockSurveyService.Setup(s => s.SubmitResponse(It.IsAny<SurveyResponse>())).ReturnsAsync(response);

            var result = await _controller.SubmitResponse(1, response);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task SubmitResponse_ReturnsBadRequest_OnValidationError()
        {
            SetUser("user@example.com");
            var response = new SurveyResponse { SurveyId = 1, responses = new List<QuestionResponse>() };
            _mockSurveyService.Setup(s => s.SubmitResponse(It.IsAny<SurveyResponse>())).ThrowsAsync(new ArgumentException("Validation error"));

            var result = await _controller.SubmitResponse(1, response);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Validation error", badRequest.Value);
        }

        [Fact]
        public async Task GetResponses_ReturnsOk_WhenSuccessful()
        {
            SetUser("admin@example.com", "Admin");
            var responses = new List<SurveyResponse> { new SurveyResponse { Id = 1 } };
            _mockSurveyService.Setup(s => s.GetResponses(1)).ReturnsAsync(responses);

            var result = await _controller.GetResponses(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(responses, okResult.Value);
        }

        [Fact]
        public async Task GetResponses_ReturnsBadRequest_WhenSurveyNotFound()
        {
            SetUser("admin@example.com", "Admin");
            _mockSurveyService.Setup(s => s.GetResponses(1)).ThrowsAsync(new ArgumentException("Survey not found"));

            var result = await _controller.GetResponses(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Survey not found", badRequest.Value);
        }

        [Fact]
        public async Task GetResponse_ReturnsOk_WhenFound()
        {
            SetUser("admin@example.com", "Admin");
            var response = new SurveyResponse { Id = 1 };
            _mockSurveyService.Setup(s => s.GetResponse(1, 1)).ReturnsAsync(response);

            var result = await _controller.GetResponse(1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetResponse_ReturnsNotFound_WhenNotFound()
        {
            SetUser("admin@example.com", "Admin");
            _mockSurveyService.Setup(s => s.GetResponse(1, 1)).ReturnsAsync((SurveyResponse?)null);

            var result = await _controller.GetResponse(1, 1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetResponse_ReturnsBadRequest_WhenSurveyNotFound()
        {
            SetUser("admin@example.com", "Admin");
            _mockSurveyService.Setup(s => s.GetResponse(1, 1)).ThrowsAsync(new ArgumentException("Survey not found"));

            var result = await _controller.GetResponse(1, 1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Survey not found", badRequest.Value);
        }
    }
} 