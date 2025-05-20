using Xunit;
using Moq;
using Survey.Controllers;
using Survey.Services;
using Survey.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class LoginControllerTests
{
    private readonly Mock<ILoginService> _mockLoginService;
    private readonly LoginController _controller;

    public LoginControllerTests()
    {
        _mockLoginService = new Mock<ILoginService>();
        _controller = new LoginController(_mockLoginService.Object);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenSuccess()
    {
        _mockLoginService.Setup(x => x.Register(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(true);

        var result = await _controller.Register(new AuthRequest { Email = "admin@test.com", Password = "admin" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Registration successful.", okResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailExists()
    {
        _mockLoginService.Setup(x => x.Register(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(false);

        var result = await _controller.Register(new AuthRequest { Email = "taken@test.com", Password = "admin" });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsToken_WhenValid()
    {
        _mockLoginService.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync("valid-token");
        _mockLoginService.Setup(x => x.GetUser(It.IsAny<string>()))
                         .ReturnsAsync(new User { Role = "Admin" });

        var result = await _controller.Login(new AuthRequest { Email = "admin@test.com", Password = "admin" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("token", okResult.Value.ToString());
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenInvalid()
    {
        _mockLoginService.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync((string?)null);

        var result = await _controller.Login(new AuthRequest { Email = "invalid@test.com", Password = "wrong" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
