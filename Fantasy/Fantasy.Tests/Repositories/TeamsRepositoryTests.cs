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
public class TeamsRepositoryTests
{
    private TeamsRepository _repository = null!;
    private Mock<IFileStorage> _mockFileStorage = null!;
    private DataContext _context = null!;

    [TestInitialize]
    public void Setup()
    {
        // Set up the In-Memory Database
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mockFileStorage = new Mock<IFileStorage>();

        // Initialize the repository
        _repository = new TeamsRepository(_context, _mockFileStorage.Object);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsSuccess_WhenTeamIsAdded()
    {
        // Arrange
        var country = new Country { Id = 1, Name = "Country A" };
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        var teamDTO = new TeamDTO { Name = "Team A", CountryId = 1 };

        // Act
        var result = await _repository.AddAsync(teamDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual("Team A", result.Result!.Name);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenCountryNotFound()
    {
        // Arrange
        var teamDTO = new TeamDTO { Name = "Team A", CountryId = 999 }; // Non-existent country

        // Act
        var result = await _repository.AddAsync(teamDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR004", result.Message); // Country not found error code
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsTeams_WhenTeamsExist()
    {
        // Arrange
        var country = new Country { Id = 1, Name = "Country A" };
        var team1 = new Team { Id = 1, Name = "Team A", CountryId = 1 };
        var team2 = new Team { Id = 2, Name = "Team B", CountryId = 1 };

        _context.Countries.Add(country);
        _context.Teams.AddRange(team1, team2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetComboAsync(1);

        // Assert
        Assert.AreEqual(2, result.Count());
        Assert.IsTrue(result.Any(t => t.Name == "Team A"));
        Assert.IsTrue(result.Any(t => t.Name == "Team B"));
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsSuccess_WhenTeamIsUpdated()
    {
        // Arrange
        var country = new Country { Id = 1, Name = "Country A" };
        var team = new Team { Id = 1, Name = "Old Team", Country = country };
        _context.Countries.Add(country);
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        var teamDTO = new TeamDTO { Id = 1, Name = "Updated Team", CountryId = 1 };

        // Act
        var result = await _repository.UpdateAsync(teamDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual("Updated Team", result.Result!.Name);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenTeamNotFound()
    {
        // Arrange
        var teamDTO = new TeamDTO { Id = 999, Name = "Non-existent Team", CountryId = 1 };

        // Act
        var result = await _repository.UpdateAsync(teamDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR005", result.Message); // Team not found error code
    }

    [TestMethod]
    public async Task GetAsync_ReturnsTeams_WhenTeamsExist()
    {
        // Arrange
        var country = new Country { Id = 1, Name = "Country A" };
        var team1 = new Team { Id = 1, Name = "Team A", Country = country };
        var team2 = new Team { Id = 2, Name = "Team B", Country = country };

        _context.Countries.Add(country);
        _context.Teams.AddRange(team1, team2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAsync();

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsTeam_WhenTeamExists()
    {
        // Arrange
        var country = new Country { Id = 1, Name = "Country A" };
        var team = new Team { Id = 1, Name = "Team A", Country = country };

        _context.Countries.Add(country);
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual("Team A", result.Result!.Name);
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsError_WhenTeamNotFound()
    {
        // Act
        var result = await _repository.GetAsync(999); // Non-existent team ID

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR001", result.Message); // Team not found error code
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsCount_WhenFilterApplied()
    {
        // Arrange
        var country = new Country { Id = 1, Name = "Country A" };
        var team1 = new Team { Id = 1, Name = "Team A", Country = country };
        var team2 = new Team { Id = 2, Name = "Team B", Country = country };

        _context.Countries.Add(country);
        _context.Teams.AddRange(team1, team2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10, Filter = "Team" };

        // Act
        var result = await _repository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result);
    }

    [TestMethod]
    public async Task GetAsync_Paginated_ReturnsPaginatedTeams_WhenTeamsExist()
    {
        // Arrange
        var country = new Country { Id = 1, Name = "Country A" };
        var team1 = new Team { Id = 1, Name = "Team A", Country = country };
        var team2 = new Team { Id = 2, Name = "Team B", Country = country };

        _context.Countries.Add(country);
        _context.Teams.AddRange(team1, team2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Act
        var result = await _repository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.Count());
    }

    [TestMethod]
    public async Task AddAsync_ReturnsSuccess_WhenTeamIsAddedWithImage()
    {
        // Arrange: Add a country to the in-memory database to avoid "ERR004"
        var country = new Country { Id = 1, Name = "Country A" };
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        // Create a TeamDTO with a Base64 image string
        var imageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }); // Example Base64 image
        var teamDTO = new TeamDTO { Name = "Team A", CountryId = 1, Image = imageBase64 };

        // Mock the SaveFileAsync method to return a fake image URL
        _mockFileStorage.Setup(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "teams"))
            .ReturnsAsync("http://example.com/teamimage.jpg");

        // Act: Call the AddAsync method
        var result = await _repository.AddAsync(teamDTO);

        // Assert: Ensure that the team was added successfully and the image was saved
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual("Team A", result.Result!.Name);
        Assert.AreEqual("http://example.com/teamimage.jpg", result.Result.Image);

        // Verify that SaveFileAsync was called with the correct parameters
        _mockFileStorage.Verify(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "teams"), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenCountryNotFound()
    {
        // Arrange: Add a team to the in-memory database but do not add the country to simulate "ERR004"
        var team = new Team { Id = 1, Name = "Team A", CountryId = 1 };
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        var teamDTO = new TeamDTO { Id = 1, Name = "Updated Team A", CountryId = 999 }; // Non-existent country ID

        // Act: Call the UpdateAsync method
        var result = await _repository.UpdateAsync(teamDTO);

        // Assert: Ensure the response indicates failure and returns the correct error message
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR004", result.Message); // Country not found error code
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsSuccess_WhenTeamIsUpdatedWithImage()
    {
        // Arrange: Add a country and a team to the in-memory database
        var country = new Country { Id = 1, Name = "Country A" };
        var team = new Team { Id = 1, Name = "Team A", Country = country, CountryId = 1 };
        _context.Countries.Add(country);
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        // Create a TeamDTO with a Base64 image string
        var imageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }); // Example Base64 image
        var teamDTO = new TeamDTO { Id = 1, Name = "Updated Team A", CountryId = 1, Image = imageBase64 };

        // Mock the SaveFileAsync method to return a fake image URL
        _mockFileStorage.Setup(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "teams"))
            .ReturnsAsync("http://example.com/teamimage.jpg");

        // Act: Call the UpdateAsync method
        var result = await _repository.UpdateAsync(teamDTO);

        // Assert: Ensure the team was updated successfully and the image was saved
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual("Updated Team A", result.Result!.Name);
        Assert.AreEqual("http://example.com/teamimage.jpg", result.Result.Image);

        // Verify that SaveFileAsync was called with the correct parameters
        _mockFileStorage.Verify(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "teams"), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFilteredTeams_WhenFilterIsApplied()
    {
        // Arrange: Add countries and teams to the in-memory database
        var country1 = new Country { Id = 1, Name = "Country A" };
        var country2 = new Country { Id = 2, Name = "Country B" };
        var team1 = new Team { Id = 1, Name = "Team Alpha", Country = country1 };
        var team2 = new Team { Id = 2, Name = "Team Beta", Country = country2 };
        var team3 = new Team { Id = 3, Name = "Team Gamma", Country = country1 };

        _context.Countries.AddRange(country1, country2);
        _context.Teams.AddRange(team1, team2, team3);
        await _context.SaveChangesAsync();

        // Create a PaginationDTO with a filter for teams with "Alpha" in their name
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10, Filter = "Alpha" };

        // Act: Call the GetAsync method with the filter
        var result = await _repository.GetAsync(pagination);

        // Assert: Ensure only the team with "Alpha" in the name is returned
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
        Assert.AreEqual("Team Alpha", result.Result!.First().Name);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenDbUpdateExceptionOccurs_ForTeam()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Add a team to the in-memory database
        var country = new Country { Id = 1, Name = "Country A" };
        var team = new Team { Id = 1, Name = "Original Team", Country = country };
        context.Countries.Add(country);
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        // Create a fake context to simulate a DbUpdateException
        var fakeContext = new FakeDbContext(options);
        var repository = new TeamsRepository(fakeContext, _mockFileStorage.Object);
        var teamDTO = new TeamDTO { Id = 1, Name = "Updated Team", CountryId = 1 };

        // Act
        var result = await repository.UpdateAsync(teamDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message); // Assert that the error message matches ERR003
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenGeneralExceptionOccurs_ForTeam()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Add a team to the in-memory database
        var country = new Country { Id = 1, Name = "Country A" };
        var team = new Team { Id = 1, Name = "Original Team", Country = country };
        context.Countries.Add(country);
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        // Create a fake context to simulate a general exception
        var fakeContext = new FakeDbContextWithGeneralException(options);
        var repository = new TeamsRepository(fakeContext, _mockFileStorage.Object);
        var teamDTO = new TeamDTO { Id = 1, Name = "Updated Team", CountryId = 1 };

        // Act
        var result = await repository.UpdateAsync(teamDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message); // Assert that the error message matches the simulated general exception message
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenDbUpdateExceptionOccurs_ForTeam()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Add a country to the in-memory database
        var country = new Country { Id = 1, Name = "Country A" };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        // Create a fake context to simulate a DbUpdateException
        var fakeContext = new FakeDbContext(options);
        var repository = new TeamsRepository(fakeContext, _mockFileStorage.Object);
        var teamDTO = new TeamDTO { Name = "New Team", CountryId = 1 };

        // Act
        var result = await repository.AddAsync(teamDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message); // Assert that the error message matches ERR003
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenGeneralExceptionOccurs_ForTeam()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Add a country to the in-memory database
        var country = new Country { Id = 1, Name = "Country A" };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        // Create a fake context to simulate a general exception
        var fakeContext = new FakeDbContextWithGeneralException(options);
        var repository = new TeamsRepository(fakeContext, _mockFileStorage.Object);
        var teamDTO = new TeamDTO { Name = "New Team", CountryId = 1 };

        // Act
        var result = await repository.AddAsync(teamDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message); // Assert that the error message matches the simulated general exception message
    }
}