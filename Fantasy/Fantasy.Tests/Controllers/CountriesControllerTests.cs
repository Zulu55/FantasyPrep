using Fantasy.Backend.Controllers;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Fantasy.Tests.Controllers;

[TestClass]
public class CountriesControllerTests
{
    private Mock<ICountriesUnitOfWork> _mockCountriesUnitOfWork = null!;
    private CountriesController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockCountriesUnitOfWork = new Mock<ICountriesUnitOfWork>();
        _controller = new CountriesController(null!, _mockCountriesUnitOfWork.Object);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsOkResult_WithListOfCountries()
    {
        // Arrange
        var mockData = new List<Country>
        {
            new() { Id = 1, Name = "Country 1" },
            new() { Id = 2, Name = "Country 2" }
        };

        _mockCountriesUnitOfWork.Setup(uow => uow.GetComboAsync()).ReturnsAsync(mockData);

        // Act
        var result = await _controller.GetComboAsync();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(List<Country>));
        Assert.AreEqual(2, ((List<Country>)okResult.Value).Count);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<IEnumerable<Country>>
        {
            WasSuccess = true,
            Result = [new() { Id = 1, Name = "Country 1" }]
        };

        _mockCountriesUnitOfWork.Setup(uow => uow.GetAsync()).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<Country>));
    }

    [TestMethod]
    public async Task GetAsync_ReturnsBadRequest_WhenNotSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<IEnumerable<Country>> { WasSuccess = false };
        _mockCountriesUnitOfWork.Setup(uow => uow.GetAsync()).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO();
        var mockResponse = new ActionResponse<IEnumerable<Country>>
        {
            WasSuccess = true,
            Result = new List<Country> { new Country { Id = 1, Name = "Country 1" } }
        };

        _mockCountriesUnitOfWork.Setup(uow => uow.GetAsync(pagination)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(List<Country>));
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsBadRequest_WhenNotSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO();
        var mockResponse = new ActionResponse<IEnumerable<Country>> { WasSuccess = false };
        _mockCountriesUnitOfWork.Setup(uow => uow.GetAsync(pagination)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO();
        var mockResponse = new ActionResponse<int>
        {
            WasSuccess = true,
            Result = 10
        };

        _mockCountriesUnitOfWork.Setup(uow => uow.GetTotalRecordsAsync(pagination)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetTotalRecordsAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(10, okResult.Value);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsBadRequest_WhenNotSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO();
        var mockResponse = new ActionResponse<int> { WasSuccess = false };
        _mockCountriesUnitOfWork.Setup(uow => uow.GetTotalRecordsAsync(pagination)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<Country>
        {
            WasSuccess = true,
            Result = new Country { Id = 1, Name = "Country 1" }
        };

        _mockCountriesUnitOfWork.Setup(uow => uow.GetAsync(1)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync(1);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(Country));
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsNotFound_WhenNotSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<Country>
        {
            WasSuccess = false,
            Message = "Country not found"
        };

        _mockCountriesUnitOfWork.Setup(uow => uow.GetAsync(1)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync(1);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual("Country not found", notFoundResult.Value);
    }
}