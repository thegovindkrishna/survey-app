using Xunit;
using Moq;
using Survey.Controllers;
using Survey.Services;
using Survey.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Survey.Tests
{
    public class SurveyResultsControllerTests
    {
        private readonly Mock<ISurveyResultsService> _mockResultsService;
        private readonly SurveyResultsController _controller;

        public SurveyResultsControllerTests()
        {
            _mockResultsService = new Mock<ISurveyResultsService>();
            _controller = new SurveyResultsController(_mockResultsService.Object);

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
        public async Task GetResults_ReturnsOk_WhenSuccessful()
        {
            var results = new SurveyResults { SurveyId = 1 };
            _mockResultsService.Setup(s => s.GetSurveyResults(1)).ReturnsAsync(results);

            var result = await _controller.GetResults(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(results, okResult.Value);
        }

        [Fact]
        public async Task GetResults_ReturnsBadRequest_WhenSurveyNotFound()
        {
            _mockResultsService.Setup(s => s.GetSurveyResults(1)).ThrowsAsync(new ArgumentException("Survey not found"));

            var result = await _controller.GetResults(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var messageProp = badRequest.Value.GetType().GetProperty("message");
            if (messageProp != null)
                Assert.Equal("Survey not found", messageProp.GetValue(badRequest.Value, null));
            else
                Assert.Equal("Survey not found", badRequest.Value);
        }

        [Fact]
        public async Task ExportToCsv_ReturnsFile_WhenSuccessful()
        {
            var csvBytes = new byte[] { 1, 2, 3 };
            _mockResultsService.Setup(s => s.ExportToCsv(1)).ReturnsAsync(csvBytes);

            var result = await _controller.ExportToCsv(1);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal(csvBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportToCsv_ReturnsBadRequest_WhenSurveyNotFound()
        {
            _mockResultsService.Setup(s => s.ExportToCsv(1)).ThrowsAsync(new ArgumentException("Survey not found"));

            var result = await _controller.ExportToCsv(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var messageProp = badRequest.Value.GetType().GetProperty("message");
            if (messageProp != null)
                Assert.Equal("Survey not found", messageProp.GetValue(badRequest.Value, null));
            else
                Assert.Equal("Survey not found", badRequest.Value);
        }

        [Fact]
        public async Task ExportToPdf_ReturnsFile_WhenSuccessful()
        {
            var pdfBytes = new byte[] { 4, 5, 6 };
            _mockResultsService.Setup(s => s.ExportToPdf(1)).ReturnsAsync(pdfBytes);

            var result = await _controller.ExportToPdf(1);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal(pdfBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportToPdf_ReturnsBadRequest_WhenSurveyNotFound()
        {
            _mockResultsService.Setup(s => s.ExportToPdf(1)).ThrowsAsync(new ArgumentException("Survey not found"));

            var result = await _controller.ExportToPdf(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var messageProp = badRequest.Value.GetType().GetProperty("message");
            if (messageProp != null)
                Assert.Equal("Survey not found", messageProp.GetValue(badRequest.Value, null));
            else
                Assert.Equal("Survey not found", badRequest.Value);
        }

        [Fact]
        public async Task GenerateShareLink_ReturnsOk_WhenSuccessful()
        {
            var link = "http://localhost:4200/survey/1";
            _mockResultsService.Setup(s => s.GenerateShareLink(1)).ReturnsAsync(link);

            var result = await _controller.GenerateShareLink(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new { shareLink = link }, okResult.Value);
        }

        [Fact]
        public async Task GenerateShareLink_ReturnsBadRequest_WhenSurveyNotFound()
        {
            _mockResultsService.Setup(s => s.GenerateShareLink(1)).ThrowsAsync(new ArgumentException("Survey not found"));

            var result = await _controller.GenerateShareLink(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var messageProp = badRequest.Value.GetType().GetProperty("message");
            if (messageProp != null)
                Assert.Equal("Survey not found", messageProp.GetValue(badRequest.Value, null));
            else
                Assert.Equal("Survey not found", badRequest.Value);
        }
    }
} 