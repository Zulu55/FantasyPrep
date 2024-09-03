using Fantasy.Backend.Data;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Fantasy.Tests.Repositories;

[TestClass]
public class TournamentTeamsRepositoryTests
{
    private Mock<DataContext> _mockContext = null!;
    private TournamentTeamsRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockContext = new Mock<DataContext>(new DbContextOptions<DataContext>());
        _repository = new TournamentTeamsRepository(_mockContext.Object);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsSuccess_WhenTournamentAndTeamExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.Tournaments.Add(new Tournament { Id = 1, Name = "Test Tournament" });
        context.Teams.Add(new Team { Id = 1, Name = "Test Team" });
        await context.SaveChangesAsync();

        var repository = new TournamentTeamsRepository(context);

        var tournamentTeamDTO = new TournamentTeamDTO
        {
            TournamentId = 1,
            TeamId = 1
        };

        // Act
        var result = await repository.AddAsync(tournamentTeamDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(1, result.Result.Tournament.Id);
        Assert.AreEqual(1, result.Result.Team.Id);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenTournamentDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.Teams.Add(new Team { Id = 1, Name = "Test Team" });
        await context.SaveChangesAsync();

        var repository = new TournamentTeamsRepository(context);

        var tournamentTeamDTO = new TournamentTeamDTO
        {
            TournamentId = 99, // Non-existent Tournament
            TeamId = 1
        };

        // Act
        var result = await repository.AddAsync(tournamentTeamDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR009", result.Message);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenTeamDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.Tournaments.Add(new Tournament { Id = 1, Name = "Test Tournament" });
        await context.SaveChangesAsync();

        var repository = new TournamentTeamsRepository(context);

        var tournamentTeamDTO = new TournamentTeamDTO
        {
            TournamentId = 1,
            TeamId = 99 // Non-existent Team
        };

        // Act
        var result = await repository.AddAsync(tournamentTeamDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR005", result.Message);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsTournamentTeams()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.TournamentTeams.AddRange(
            new TournamentTeam { Id = 1, TournamentId = 1, Team = new Team { Id = 1, Name = "Team A" } },
            new TournamentTeam { Id = 2, TournamentId = 1, Team = new Team { Id = 2, Name = "Team B" } }
        );
        await context.SaveChangesAsync();

        var repository = new TournamentTeamsRepository(context);

        // Act
        var result = await repository.GetComboAsync(1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count());
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsTotalRecordCount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.TournamentTeams.AddRange(
            new TournamentTeam { Id = 1, TournamentId = 1, Team = new Team { Id = 1, Name = "Team A" } },
            new TournamentTeam { Id = 2, TournamentId = 1, Team = new Team { Id = 2, Name = "Team B" } }
        );
        await context.SaveChangesAsync();

        var repository = new TournamentTeamsRepository(context);
        var pagination = new PaginationDTO { Id = 1 };

        // Act
        var result = await repository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result);
    }

    [TestMethod]
    public async Task GetAsync_WithPaginationAndFilter_ReturnsFilteredTournamentTeams()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.TournamentTeams.AddRange(
            new TournamentTeam { Id = 1, TournamentId = 1, Team = new Team { Id = 1, Name = "Team Alpha" } },
            new TournamentTeam { Id = 2, TournamentId = 1, Team = new Team { Id = 2, Name = "Team Beta" } }
        );
        await context.SaveChangesAsync();

        var repository = new TournamentTeamsRepository(context);
        var pagination = new PaginationDTO { Id = 1, Filter = "Alpha", Page = 1, RecordsNumber = 10 };

        // Act
        var result = await repository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count()); // Should return only the teams that match the filter
        Assert.AreEqual("Team Alpha", result.Result!.First().Team.Name);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsCorrectCount_WhenFilterIsApplied()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        context.TournamentTeams.AddRange(
            new TournamentTeam { Id = 1, TournamentId = 1, Team = new Team { Id = 1, Name = "Team Alpha" } },
            new TournamentTeam { Id = 2, TournamentId = 1, Team = new Team { Id = 2, Name = "Team Beta" } },
            new TournamentTeam { Id = 3, TournamentId = 1, Team = new Team { Id = 3, Name = "Team Gamma" } }
        );
        await context.SaveChangesAsync();

        var repository = new TournamentTeamsRepository(context);
        var pagination = new PaginationDTO { Id = 1, Filter = "Alpha" };

        // Act
        var result = await repository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result); // Only "Team Alpha" should match the filter
    }
}