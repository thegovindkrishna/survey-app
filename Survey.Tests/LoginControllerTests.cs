using Xunit;
using Moq;
using Survey.Controllers;
using Survey.Services;
using Survey.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Reflection;

/// <summary>
/// Unit tests for LoginController covering registration and login scenarios using mocked ILoginService.
/// </summary>
public class LoginControllerTests
{
    private readonly Mock<ILoginService> _mockLoginService;//fake loginservice
    private readonly LoginController _controller;

    public LoginControllerTests()
    {
        _mockLoginService = new Mock<ILoginService>();
        _controller = new LoginController(_mockLoginService.Object);
    }

    /// <summary>
    /// Ensures Register returns Ok when registration is successful.
    /// </summary>
    [Fact]
    public async Task Register_ReturnsOk_WhenSuccess()
    {
        _mockLoginService.Setup(x => x.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(true);

        var result = await _controller.Register(new AuthRequest { Email = "admin@test.com", Password = "admin" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var messageProp = okResult.Value.GetType().GetProperty("message");
        Assert.NotNull(messageProp);
        Assert.Equal("Registration successful.", messageProp.GetValue(okResult.Value, null));
    }

    /// <summary>
    /// Ensures Register returns Conflict when the email already exists.
    /// </summary>
    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailExists()
    {
        _mockLoginService.Setup(x => x.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(false);
        _mockLoginService.Setup(x => x.GetUser(It.IsAny<string>()))
                         .ReturnsAsync(new User { Email = "taken@test.com", Password = "admin", Role = "User" });

        var result = await _controller.Register(new AuthRequest { Email = "taken@test.com", Password = "admin" });

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        var messageProp = conflictResult.Value.GetType().GetProperty("message");
        Assert.NotNull(messageProp);
        Assert.Equal("A user with this email already exists.", messageProp.GetValue(conflictResult.Value, null));
    }

    /// <summary>
    /// Ensures Login returns a token when credentials are valid.
    /// </summary>
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

    /// <summary>
    /// Ensures Login returns Unauthorized when credentials are invalid.
    /// </summary>
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenInvalid()
    {
        _mockLoginService.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync((string?)null);

        var result = await _controller.Login(new AuthRequest { Email = "invalid@test.com", Password = "wrong" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
