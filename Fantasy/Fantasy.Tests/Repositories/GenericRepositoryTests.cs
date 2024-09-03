using Fantasy.Backend.Data;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Fantasy.Tests.Repositories;

[TestClass]
public class GenericRepositoryTests
{
    private DataContext _context = null!;
    private GenericRepository<Country> _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new DataContext(options);
        _repository = new GenericRepository<Country>(_context);

        _context.Countries.AddRange(new List<Country>
            {
                new Country { Id = 1, Name = "Country 1" },
                new Country { Id = 2, Name = "Country 2" }
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
    public async Task AddAsync_ReturnsAddedEntity_WhenSuccess()
    {
        // Arrange
        var newCountry = new Country { Id = 3, Name = "New Country" };

        // Act
        var result = await _repository.AddAsync(newCountry);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(3, result.Result.Id);
    }

    [TestMethod]
    public async Task DeleteAsync_ReturnsSuccess_WhenEntityExists()
    {
        // Act
        var result = await _repository.DeleteAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNull(await _context.Countries.FindAsync(1));
    }

    [TestMethod]
    public async Task DeleteAsync_ReturnsError_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _repository.DeleteAsync(999); // Non-existent ID

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR001", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsEntity_WhenEntityExists()
    {
        // Act
        var result = await _repository.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(1, result.Result.Id);
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsError_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _repository.GetAsync(999); // Non-existent ID

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR001", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsAllEntities()
    {
        // Act
        var result = await _repository.GetAsync();

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result.Count());
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsUpdatedEntity_WhenSuccess()
    {
        // Arrange
        var countryToUpdate = await _context.Countries.FindAsync(1);
        countryToUpdate!.Name = "Updated Country";

        // Act
        var result = await _repository.UpdateAsync(countryToUpdate);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual("Updated Country", result.Result!.Name);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        var countryToUpdate = new Country { Id = 999, Name = "Non-existent Country" };

        // Act
        var result = await _repository.UpdateAsync(countryToUpdate);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsPaginatedEntities()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 1 };

        // Act
        var result = await _repository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsTotalRecordsCount()
    {
        // Act
        var result = await _repository.GetTotalRecordsAsync();

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        var newCountry = new Country { Id = 3, Name = "New Country" };

        // Mock the DbContext and simulate DbUpdateException when SaveChangesAsync is called
        var mockContext = new Mock<DataContext>(
            new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new DbUpdateException());

        _repository = new GenericRepository<Country>(mockContext.Object);

        // Act
        var result = await _repository.AddAsync(newCountry);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenGeneralExceptionOccurs()
    {
        // Arrange
        var newCountry = new Country { Id = 3, Name = "New Country" };

        // Mock the DbContext and simulate a general exception when SaveChangesAsync is called
        var mockContext = new Mock<DataContext>(
            new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("General exception occurred"));

        _repository = new GenericRepository<Country>(mockContext.Object);

        // Act
        var result = await _repository.AddAsync(newCountry);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message);
    }

    [TestMethod]
    public async Task DeleteAsync_ReturnsError_WhenGeneralExceptionOccurs()
    {
        // Arrange
        var mockContext = new Mock<DataContext>(
            new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options);

        // Simulate the entity to be deleted
        var countryToDelete = new Country { Id = 1, Name = "Country 1" };

        // Configure the DbSet to return the simulated entity
        var mockDbSet = new Mock<DbSet<Country>>();
        mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(countryToDelete);

        mockContext.Setup(c => c.Set<Country>()).Returns(mockDbSet.Object);

        // Simulate a general exception when trying to save changes
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("General exception occurred"));

        _repository = new GenericRepository<Country>(mockContext.Object);

        // Act
        var result = await _repository.DeleteAsync(1);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR002", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenGeneralExceptionOccurs()
    {
        // Arrange
        var mockContext = new Mock<DataContext>(
            new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options);

        // Simulate the entity to be updated
        var countryToUpdate = new Country { Id = 1, Name = "Country 1" };

        // Configure the DbSet to simulate the Update operation
        var mockDbSet = new Mock<DbSet<Country>>();
        mockDbSet.Setup(m => m.Update(It.IsAny<Country>()));

        mockContext.Setup(c => c.Set<Country>()).Returns(mockDbSet.Object);

        // Simulate a general exception when trying to save changes
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("General exception occurred"));

        _repository = new GenericRepository<Country>(mockContext.Object);

        // Act
        var result = await _repository.UpdateAsync(countryToUpdate);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message);
    }
}