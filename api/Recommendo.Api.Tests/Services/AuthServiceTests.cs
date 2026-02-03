using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Recommendo.Api.Configuration;
using Recommendo.Api.Data;
using Recommendo.Api.DTOs;
using Recommendo.Api.Models;
using Recommendo.Api.Services;
using Xunit;

namespace Recommendo.Api.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly RecommendoContext _context;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<RecommendoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RecommendoContext(options);
        
        var jwtSettings = new JwtSettings
        {
            Secret = "test-jwt-secret-key-minimum-32-characters-long",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60
        };
        
        var jwtOptions = Options.Create(jwtSettings);
        _authService = new AuthService(_context, jwtOptions);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateNewUser()
    {
        var request = new RegisterRequest("test@test.com", "testuser", "TestPass123!");
        var result = await _authService.RegisterAsync(request);
        
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email);
        result.Username.Should().Be(request.Username);
        result.Token.Should().NotBeNullOrEmpty();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        user.Should().NotBeNull();
        user!.PasswordHash.Should().NotBe(request.Password);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailExists()
    {
        var existingUser = new User
        {
            Email = "existing@test.com",
            Username = "existing",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest("existing@test.com", "newuser", "Password123!");
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _authService.RegisterAsync(request));
        exception.Message.Should().Contain("email");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WithValidCredentials()
    {
        var password = "TestPass123!";
        var user = new User
        {
            Email = "login@test.com",
            Username = "loginuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("login@test.com", password);
        var result = await _authService.LoginAsync(request);
        
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email);
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WithInvalidPassword()
    {
        var user = new User
        {
            Email = "login@test.com",
            Username = "loginuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!")
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("login@test.com", "WrongPassword123!");
        var result = await _authService.LoginAsync(request);
        
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WithNonexistentEmail()
    {
        var request = new LoginRequest("nonexistent@test.com", "password");
        var result = await _authService.LoginAsync(request);
        
        result.Should().BeNull();
    }
}
