using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Tests.General;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Fantasy.Tests.Repositories;

[TestClass]
public class TournamentsRepositoryTests
{
    private Mock<IFileStorage> _mockFileStorage = null!;
    private TournamentsRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockFileStorage = new Mock<IFileStorage>();
    }

    [TestMethod]
    public async Task AddAsync_ReturnsActionResponse_WhenSuccess_WithoutImage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);
        _repository = new TournamentsRepository(context, _mockFileStorage.Object);

        var tournamentDTO = new TournamentDTO { Name = "Test Tournament", Image = null };
        _mockFileStorage.Setup(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "tournaments"))
                        .ReturnsAsync("imagePath");

        // Act
        var result = await _repository.AddAsync(tournamentDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("Test Tournament", result.Result.Name);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsActionResponse_WhenSuccess_WithImage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);
        _repository = new TournamentsRepository(context, _mockFileStorage.Object);

        var validBase64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/wcAAwAB/ebQjH0AAAAASUVORK5CYII=";
        var tournamentDTO = new TournamentDTO { Name = "Test Tournament", Image = validBase64Image };
        _mockFileStorage.Setup(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "tournaments"))
                        .ReturnsAsync("imagePath");

        // Act
        var result = await _repository.AddAsync(tournamentDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("Test Tournament", result.Result.Name);
        Assert.AreEqual("imagePath", result.Result.Image);
        _mockFileStorage.Verify(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "tournaments"), Times.Once);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);
        var mockContext = new Mock<DataContext>(options);
        _repository = new TournamentsRepository(mockContext.Object, _mockFileStorage.Object);

        var tournamentDTO = new TournamentDTO { Name = "Test Tournament" };
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new DbUpdateException());

        // Act
        var result = await _repository.AddAsync(tournamentDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenGeneralExceptionOccurs()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);
        var mockContext = new Mock<DataContext>(options);
        _repository = new TournamentsRepository(mockContext.Object, _mockFileStorage.Object);

        var tournamentDTO = new TournamentDTO { Name = "Test Tournament" };
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("General exception occurred"));

        // Act
        var result = await _repository.AddAsync(tournamentDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsActiveTournaments()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.Tournaments.AddRange(
            new Tournament { Id = 1, Name = "Tournament 1", IsActive = true },
            new Tournament { Id = 2, Name = "Tournament 2", IsActive = false }
        );
        await context.SaveChangesAsync();

        var repository = new TournamentsRepository(context, _mockFileStorage.Object);

        // Act
        var result = await repository.GetComboAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Tournament 1", result.First().Name);
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsPaginatedTournaments()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.Tournaments.AddRange(
            new Tournament { Id = 1, Name = "Tournament 1" },
            new Tournament { Id = 2, Name = "Tournament 2" }
        );
        await context.SaveChangesAsync();

        var repository = new TournamentsRepository(context, _mockFileStorage.Object);

        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Act
        var result = await repository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsTournament_WhenExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        var tournament = new Tournament { Id = 1, Name = "Tournament 1" };
        context.Tournaments.Add(tournament);
        await context.SaveChangesAsync();

        var repository = new TournamentsRepository(context, _mockFileStorage.Object);

        // Act
        var result = await repository.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(1, result.Result.Id);
        Assert.AreEqual("Tournament 1", result.Result.Name);
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsError_WhenNotExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        var repository = new TournamentsRepository(context, _mockFileStorage.Object);

        // Act
        var result = await repository.GetAsync(1);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR001", result.Message);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsTotalRecordCount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.Tournaments.AddRange(
            new Tournament { Id = 1, Name = "Test Tournament 1" },
            new Tournament { Id = 2, Name = "Other Tournament 2" }
        );
        await context.SaveChangesAsync();

        var repository = new TournamentsRepository(context, _mockFileStorage.Object);
        var pagination = new PaginationDTO { Filter = "Test" };

        // Act
        var result = await repository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsActionResponse_WhenSuccess()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        var tournament = new Tournament { Id = 1, Name = "Original Tournament", IsActive = false };
        context.Tournaments.Add(tournament);
        await context.SaveChangesAsync();

        var repository = new TournamentsRepository(context, _mockFileStorage.Object);

        var tournamentDTO = new TournamentDTO { Id = 1, Name = "Updated Tournament", IsActive = true };

        // Act
        var result = await repository.UpdateAsync(tournamentDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual("Updated Tournament", result.Result!.Name);
        Assert.IsTrue(result.Result.IsActive);
        context.Entry(result.Result).Reload();
        Assert.AreEqual("Updated Tournament", context.Tournaments.Find(1)!.Name);
        Assert.IsTrue(context.Tournaments.Find(1)!.IsActive);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenNotExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        var repository = new TournamentsRepository(context, _mockFileStorage.Object);

        var tournamentDTO = new TournamentDTO { Id = 1, Name = "Updated Tournament" };

        // Act
        var result = await repository.UpdateAsync(tournamentDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR005", result.Message);
    }

    [TestMethod]
    public async Task
        UpdateAsync_ReturnsError_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        var tournament = new Tournament { Id = 1, Name = "Original Tournament" };
        context.Tournaments.Add(tournament);
        await context.SaveChangesAsync();

        var fakeContext = new FakeDbContext(options);
        var repository = new TournamentsRepository(fakeContext, _mockFileStorage.Object);
        var tournamentDTO = new TournamentDTO { Id = 1, Name = "Updated Tournament" };

        // Act
        var result = await repository.UpdateAsync(tournamentDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenGeneralExceptionOccurs()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        var tournament = new Tournament { Id = 1, Name = "Original Tournament" };
        context.Tournaments.Add(tournament);
        await context.SaveChangesAsync();

        var fakeContext = new FakeDbContextWithGeneralException(options);
        var repository = new TournamentsRepository(fakeContext, _mockFileStorage.Object);
        var tournamentDTO = new TournamentDTO { Id = 1, Name = "Updated Tournament" };

        // Act
        var result = await repository.UpdateAsync(tournamentDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_WithPaginationAndFilter_ReturnsFilteredTournaments()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.Tournaments.AddRange(
            new Tournament { Id = 1, Name = "Test Tournament 1" },
            new Tournament { Id = 2, Name = "Another Tournament" },
            new Tournament { Id = 3, Name = "Test Tournament 2" }
        );
        await context.SaveChangesAsync();

        var repository = new TournamentsRepository(context, _mockFileStorage.Object);

        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10, Filter = "Test" };

        // Act
        var result = await repository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.Count());
        Assert.IsTrue(result.Result!.All(t => t.Name.Contains("Test")));
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsSuccess_WhenTournamentDTOHasImage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        var tournament = new Tournament { Id = 1, Name = "Original Tournament", IsActive = false, Remarks = "Original Remarks" };
        context.Tournaments.Add(tournament);
        await context.SaveChangesAsync();

        var repository = new TournamentsRepository(context, _mockFileStorage.Object);

        var validBase64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/wcAAwAB/ebQjH0AAAAASUVORK5CYII=";
        var tournamentDTO = new TournamentDTO
        {
            Id = 1,
            Name = "Updated Tournament",
            IsActive = true,
            Remarks = "Updated Remarks",
            Image = validBase64Image
        };

        _mockFileStorage.Setup(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "tournaments"))
                        .ReturnsAsync("newImagePath");

        // Act
        var result = await repository.UpdateAsync(tournamentDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("Updated Tournament", result.Result.Name);
        Assert.AreEqual(true, result.Result.IsActive);
        Assert.AreEqual("Updated Remarks", result.Result.Remarks);
        Assert.AreEqual("newImagePath", result.Result.Image);
        _mockFileStorage.Verify(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "tournaments"), Times.Once);
    }
}