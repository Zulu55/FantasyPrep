using Fantasy.Backend.Data;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Moq;
using Match = Fantasy.Shared.Entities.Match;

namespace Fantasy.Tests.UnitsOfWork;

[TestClass]
public class MatchesUnitOfWorkTests
{
    private Mock<IMatchesRepository> _matchesRepositoryMock = null!;
    private MatchesUnitOfWork _matchesUnitOfWork = null!;

    [TestInitialize]
    public void SetUp()
    {
        _matchesRepositoryMock = new Mock<IMatchesRepository>();
        _matchesUnitOfWork = new MatchesUnitOfWork(null!, _matchesRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnMatch_WhenMatchExists()
    {
        // Arrange
        var match = new Match { Id = 1, Local = new Team { Name = "Team A" }, Visitor = new Team { Name = "Team B" } };

        _matchesRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = true, Result = match });

        // Act
        var result = await _matchesUnitOfWork.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(match.Id, result.Result!.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnError_WhenMatchDoesNotExist()
    {
        // Arrange
        _matchesRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = false, Message = "Match not found" });

        // Act
        var result = await _matchesUnitOfWork.GetAsync(1);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Match not found", result.Message);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnMatch_WhenAddedSuccessfully()
    {
        // Arrange
        var matchDTO = new MatchDTO { Id = 1, LocalId = 1, VisitorId = 2 };
        var match = new Match { Id = 1, LocalId = 1, VisitorId = 2 };

        _matchesRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<MatchDTO>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = true, Result = match });

        // Act
        var result = await _matchesUnitOfWork.AddAsync(matchDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(match.Id, result.Result!.Id);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenAddingFails()
    {
        // Arrange
        var matchDTO = new MatchDTO { Id = 1, LocalId = 1, VisitorId = 2 };

        _matchesRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<MatchDTO>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = false, Message = "Error adding match" });

        // Act
        var result = await _matchesUnitOfWork.AddAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Error adding match", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnMatch_WhenUpdatedSuccessfully()
    {
        // Arrange
        var matchDTO = new MatchDTO { Id = 1, LocalId = 1, VisitorId = 2 };
        var match = new Match { Id = 1, LocalId = 1, VisitorId = 2 };

        _matchesRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<MatchDTO>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = true, Result = match });

        // Act
        var result = await _matchesUnitOfWork.UpdateAsync(matchDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(match.Id, result.Result!.Id);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenUpdatingFails()
    {
        // Arrange
        var matchDTO = new MatchDTO { Id = 1, LocalId = 1, VisitorId = 2 };

        _matchesRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<MatchDTO>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = false, Message = "Error updating match" });

        // Act
        var result = await _matchesUnitOfWork.UpdateAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Error updating match", result.Message);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnTotalRecords_WhenSuccessful()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };

        _matchesRepositoryMock.Setup(repo => repo.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = true, Result = 5 });

        // Act
        var result = await _matchesUnitOfWork.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(5, result.Result);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnFilteredCount_WhenFilterIsApplied_InMemoryDb()
    {
        // Arrange: Set up the in-memory database context
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "FantasyTestDb")
            .Options;

        using var context = new DataContext(options);

        // Create sample data
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var match1 = new Match { Id = 1, TournamentId = tournament.Id, Local = team1, Visitor = team2, Date = DateTime.Now };
        var match2 = new Match { Id = 2, TournamentId = tournament.Id, Local = team2, Visitor = team1, Date = DateTime.Now };

        context.Tournaments.Add(tournament);
        context.Teams.AddRange(team1, team2);
        context.Matches.AddRange(match1, match2);
        await context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = tournament.Id,
            Filter = "Team A", // Applying filter for "Team A"
            Page = 1,
            RecordsNumber = 10
        };

        // Create the repository instance
        var matchesRepository = new MatchesRepository(context);

        // Act: Execute the method to be tested
        var result = await matchesRepository.GetTotalRecordsAsync(pagination);

        // Assert: Verify the result
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result); // Both matches involve "Team A"
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnError_WhenRequestFails()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };

        _matchesRepositoryMock.Setup(repo => repo.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = false, Message = "Error retrieving total records" });

        // Act
        var result = await _matchesUnitOfWork.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Error retrieving total records", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnMatches_WhenMatchesExist()
    {
        // Arrange
        var pagination = new PaginationDTO
        {
            Page = 1,
            RecordsNumber = 10
        };

        var matches = new List<Match>
        {
            new() { Id = 1, Local = new Team { Name = "Team A" }, Visitor = new Team { Name = "Team B" } },
            new() { Id = 2, Local = new Team { Name = "Team C" }, Visitor = new Team { Name = "Team D" } }
        };

        _matchesRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<IEnumerable<Match>> { WasSuccess = true, Result = matches });

        // Act
        var response = await _matchesUnitOfWork.GetAsync(pagination);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual(2, response.Result.Count());
        Assert.AreEqual(matches, response.Result);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnError_WhenNoMatchesExist()
    {
        // Arrange
        var pagination = new PaginationDTO
        {
            Page = 1,
            RecordsNumber = 10
        };

        _matchesRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<IEnumerable<Match>> { WasSuccess = false, Message = "No matches found" });

        // Act
        var response = await _matchesUnitOfWork.GetAsync(pagination);

        // Assert
        Assert.IsFalse(response.WasSuccess);
        Assert.AreEqual("No matches found", response.Message);
        Assert.IsNull(response.Result);
    }
}