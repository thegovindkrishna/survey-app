using Xunit;
using Moq;
using Survey.Services;
using Survey.Data;
using Survey.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class LoginServiceTests
{
    private readonly LoginService _loginService;
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public LoginServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("LoginServiceTestsDb")
            .Options;

        _context = new AppDbContext(options);

        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "thisisaverysecurekeyforjwttoken"},
            {"Jwt:Issuer", "SurveyAPI"},
            {"Jwt:Audience", "SurveyUsers"}
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value)))
            .Build();

        _loginService = new LoginService(_context, _config);
    }

    [Fact]
    public async Task Register_WithEmptyEmail_ReturnsFalse()
    {
        var result = await _loginService.Register("", "password");
        Assert.False(result);
    }

    [Fact]
    public async Task Register_WithEmptyPassword_ReturnsFalse()
    {
        var result = await _loginService.Register("user@example.com", "");
        Assert.False(result);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsFalse()
    {
        await _loginService.Register("existing@example.com", "password");
        var result = await _loginService.Register("existing@example.com", "newpassword");
        Assert.False(result);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsNull()
    {
        await _loginService.Register("wrongpass@example.com", "rightpassword");
        var token = await _loginService.Login("wrongpass@example.com", "wrongpassword");
        Assert.Null(token);
    }

    [Fact]
    public async Task Login_WithUnregisteredEmail_ReturnsNull()
    {
        var token = await _loginService.Login("notfound@example.com", "any");
        Assert.Null(token);
    }

    [Fact]
    public async Task GetUser_WithValidEmail_ReturnsUser()
    {
        await _loginService.Register("getme@example.com", "secret");
        var user = await _loginService.GetUser("getme@example.com");
        Assert.NotNull(user);
        Assert.Equal("getme@example.com", user.Email);
    }

    [Fact]
    public async Task GetUser_WithInvalidEmail_ReturnsNull()
    {
        var user = await _loginService.GetUser("invalid@example.com");
        Assert.Null(user);
    }
}
