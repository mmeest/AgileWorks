using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Requests_App.Controllers;
using Requests_App.Models;
using System;
using System.Threading.Tasks;
using Xunit;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using TestASPNetMVC;

namespace Requests_App.UnitTests;

public class Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Tests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Theory]
    [InlineData("/")]
    [InlineData("/Requests/Index")]
    public async Task RequestsApp_GetPageAllowAnonymus_ReturnsResponse(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var contentType = "text/html; charset=utf-8";
        Assert.Equal(contentType, response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Requestspp_CreateAction_AddsRequestToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<RequestsContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        using (var context = new RequestsContext(options))
        {
            var controller = new RequestsController(context);

            // Act
            var request = new Request
            {
                Content = "Test content",
                CreatedAt = DateTime.Now,
                Deadline = DateTime.Now.AddDays(7),
                Resolved = false
            };

            var result = await controller.Create(request) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var savedRequest = await context.Requests.FirstOrDefaultAsync();
            Assert.NotNull(savedRequest);
            Assert.Equal("Test content", savedRequest.Content);
            // Add more assertions as needed
        }
    }

    [Fact]
    public async Task RequestsApp_EditAction_UpdatesResolvedField()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder<RequestsContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .UseInternalServiceProvider(serviceProvider)
            .Options;

        using (var context = new RequestsContext(options))
        {
            // Add a request to the database for testing
            var initialRequest = new Request
            {
                Content = "Initial content",
                CreatedAt = DateTime.Now.AddDays(-10),
                Deadline = DateTime.Now.AddDays(7),
                Resolved = false
            };
            context.Requests.Add(initialRequest);
            await context.SaveChangesAsync();

            var controller = new RequestsController(context);

            // Act
            var result = await controller.Edit(initialRequest.Id, true) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName); // Check if redirected to Index action

            var updatedRequest = await context.Requests.FindAsync(initialRequest.Id);
            Assert.True(updatedRequest.Resolved); // Assert that Resolved field is updated to true
        }
    }

}