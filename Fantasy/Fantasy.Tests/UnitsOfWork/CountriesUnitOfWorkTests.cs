using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Moq;

namespace Fantasy.Tests.UnitsOfWork;

[TestClass]
public class CountriesUnitOfWorkTests
{
    private Mock<ICountriesRepository> _mockCountriesRepository = null!;
    private CountriesUnitOfWork _unitOfWork = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockCountriesRepository = new Mock<ICountriesRepository>();
        _unitOfWork = new CountriesUnitOfWork(null!, _mockCountriesRepository.Object);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsActionResponse_WithListOfCountries()
    {
        // Arrange
        var mockResponse = new ActionResponse<IEnumerable<Country>>
        {
            WasSuccess = true,
            Result = new List<Country> { new Country { Id = 1, Name = "Country 1" } }
        };

        _mockCountriesRepository.Setup(repo => repo.GetAsync()).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.GetAsync();

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsInstanceOfType(result.Result, typeof(IEnumerable<Country>));
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsActionResponse_WithListOfCountries()
    {
        // Arrange
        var pagination = new PaginationDTO();
        var mockResponse = new ActionResponse<IEnumerable<Country>>
        {
            WasSuccess = true,
            Result = new List<Country> { new Country { Id = 1, Name = "Country 1" } }
        };

        _mockCountriesRepository.Setup(repo => repo.GetAsync(pagination)).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsInstanceOfType(result.Result, typeof(IEnumerable<Country>));
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsActionResponse_WithTotalRecords()
    {
        // Arrange
        var pagination = new PaginationDTO();
        var mockResponse = new ActionResponse<int>
        {
            WasSuccess = true,
            Result = 10
        };

        _mockCountriesRepository.Setup(repo => repo.GetTotalRecordsAsync(pagination)).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(10, result.Result);
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsActionResponse_WithCountry()
    {
        // Arrange
        var mockResponse = new ActionResponse<Country>
        {
            WasSuccess = true,
            Result = new Country { Id = 1, Name = "Country 1" }
        };

        _mockCountriesRepository.Setup(repo => repo.GetAsync(1)).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsInstanceOfType(result.Result, typeof(Country));
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsListOfCountries()
    {
        // Arrange
        var mockData = new List<Country>
    {
        new Country { Id = 1, Name = "Country 1" },
        new Country { Id = 2, Name = "Country 2" }
    };

        _mockCountriesRepository.Setup(repo => repo.GetComboAsync()).ReturnsAsync(mockData);

        // Act
        var result = await _unitOfWork.GetComboAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(IEnumerable<Country>));
        Assert.AreEqual(2, ((List<Country>)result).Count);
    }
}