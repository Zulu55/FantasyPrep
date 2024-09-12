using Fantasy.Backend.Data;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fantasy.Tests.Repositories;

[TestClass]
public class CountriesRepositoryTests
{
    private DataContext _context = null!;
    private CountriesRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new DataContext(options);
        _repository = new CountriesRepository(_context);

        // Seed the in-memory database
        _context.Countries.AddRange(new List<Country>
            {
                new Country { Id = 1, Name = "Country B", Teams = [], Users = [] },
                new Country { Id = 2, Name = "Country A", Teams = [], Users = [] }
            });
        _context.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetAsync_ReturnsCountriesOrderedByName()
    {
        // Act
        var result = await _repository.GetAsync();

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.Count());
        Assert.AreEqual("Country A", result.Result!.First().Name);
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsPaginatedCountries()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 1 };

        // Act
        var result = await _repository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
        Assert.AreEqual("Country A", result.Result!.First().Name);
    }

    [TestMethod]
    public async Task GetAsync_WithPaginationAndFilter_ReturnsFilteredCountries()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 1, Filter = "B" };

        // Act
        var result = await _repository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
        Assert.AreEqual("Country B", result.Result!.First().Name);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsTotalRecordCount()
    {
        // Arrange
        var pagination = new PaginationDTO();

        // Act
        var result = await _repository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_WithFilter_ReturnsFilteredRecordCount()
    {
        // Arrange
        var pagination = new PaginationDTO { Filter = "A" };

        // Act
        var result = await _repository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result);
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsCountry_WhenFound()
    {
        // Act
        var result = await _repository.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual("Country B", result.Result!.Name);
        Assert.AreEqual(0, result.Result!.TeamsCount);
        Assert.AreEqual(0, result.Result!.UsersCount);
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsNotFound_WhenCountryNotFound()
    {
        // Act
        var result = await _repository.GetAsync(999); // ID that doesn't exist

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR001", result.Message);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsCountriesOrderedByName()
    {
        // Act
        var result = await _repository.GetComboAsync();

        // Assert
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual("Country A", result.First().Name);
    }
}