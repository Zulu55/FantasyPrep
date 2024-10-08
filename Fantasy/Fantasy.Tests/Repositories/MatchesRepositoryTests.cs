﻿using Fantasy.Backend.Data;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Enums;
using Fantasy.Tests.General;
using Microsoft.EntityFrameworkCore;
using Match = Fantasy.Shared.Entities.Match;

namespace Fantasy.Tests.Repositories;

[TestClass]
public class MatchesRepositoryTests
{
    private DataContext _context = null!;
    private MatchesRepository _matchesRepository = null!;

    [TestInitialize]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "FantasyTestDb")
            .Options;
        _context = new DataContext(options);
        _matchesRepository = new MatchesRepository(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnFilteredMatches_WhenFilterIsApplied_InMemoryDb()
    {
        // Arrange: Set up the in-memory database context
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "FantasyTestDb")
            .Options;

        using var context = new DataContext(options);

        // Create sample data: tournament, teams, and matches
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var match1 = new Match { Id = 1, Tournament = tournament, Local = team1, Visitor = team2, Date = DateTime.Now };
        var match2 = new Match { Id = 2, Tournament = tournament, Local = team2, Visitor = team1, Date = DateTime.Now.AddDays(1) };

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
        var result = await matchesRepository.GetAsync(pagination);

        // Assert: Verify the result
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.Count()); // Both matches involve "Team A"
        Assert.IsTrue(result.Result!.All(m => m.Local.Name == "Team A" || m.Visitor.Name == "Team A")); // Matches should have "Team A"
        Assert.IsTrue(result.Result!.First().Date < result.Result!.Last().Date); // Matches should be ordered by Date
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnAllMatches_WhenNoFilterIsApplied()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var match1 = new Match { Id = 1, Tournament = tournament, Local = team1, Visitor = team2, Date = DateTime.Now };
        var match2 = new Match { Id = 2, Tournament = tournament, Local = team2, Visitor = team1, Date = DateTime.Now };

        _context.Tournaments.Add(tournament);
        _context.Teams.AddRange(team1, team2);
        _context.Matches.AddRange(match1, match2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1, // Tournament Id
            Page = 1,
            RecordsNumber = 10
        };

        // Act
        var result = await _matchesRepository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.Count());
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddMatch_WhenValidDataIsProvided()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };

        _context.Tournaments.Add(tournament);
        _context.Teams.AddRange(team1, team2);
        await _context.SaveChangesAsync();

        var matchDTO = new MatchDTO
        {
            TournamentId = tournament.Id,
            LocalId = team1.Id,
            VisitorId = team2.Id,
            Date = DateTime.Now,
            IsActive = true,
            DoublePoints = false
        };

        // Act
        var result = await _matchesRepository.AddAsync(matchDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(team1.Id, result.Result.Local.Id);
        Assert.AreEqual(team2.Id, result.Result.Visitor.Id);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenLocalTeamNotFound()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team2 = new Team { Id = 2, Name = "Team B" }; // Only the visitor team is provided

        _context.Tournaments.Add(tournament);
        _context.Teams.Add(team2); // Only adding visitor team, no local team
        await _context.SaveChangesAsync();

        var matchDTO = new MatchDTO
        {
            TournamentId = tournament.Id,
            LocalId = 999, // Invalid LocalId (local team not found)
            VisitorId = team2.Id,
            Date = DateTime.Now,
            IsActive = true,
            DoublePoints = false
        };

        // Act
        var result = await _matchesRepository.AddAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR010", result.Message); // Error for missing local team
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenVisitorTeamNotFound()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" }; // Only the local team is provided

        _context.Tournaments.Add(tournament);
        _context.Teams.Add(team1); // Only adding local team, no visitor team
        await _context.SaveChangesAsync();

        var matchDTO = new MatchDTO
        {
            TournamentId = tournament.Id,
            LocalId = team1.Id,
            VisitorId = 999, // Invalid VisitorId (visitor team not found)
            Date = DateTime.Now,
            IsActive = true,
            DoublePoints = false
        };

        // Act
        var result = await _matchesRepository.AddAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR011", result.Message); // Error for missing visitor team
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenTournamentIsNotFound()
    {
        // Arrange
        var matchDTO = new MatchDTO
        {
            TournamentId = 999, // Invalid TournamentId
            LocalId = 1,
            VisitorId = 2,
            Date = DateTime.Now,
            IsActive = true
        };

        // Act
        var result = await _matchesRepository.AddAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR009", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateMatch_WhenValidDataIsProvided()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var match = new Match { Id = 1, Tournament = tournament, Local = team1, Visitor = team2, Date = DateTime.Now };

        _context.Tournaments.Add(tournament);
        _context.Teams.AddRange(team1, team2);
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        var matchDTO = new MatchDTO
        {
            Id = match.Id,
            TournamentId = tournament.Id,
            LocalId = team1.Id,
            VisitorId = team2.Id,
            Date = DateTime.Now.AddDays(1),
            IsActive = false,
            DoublePoints = true
        };

        // Act
        var result = await _matchesRepository.UpdateAsync(matchDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(false, result.Result!.IsActive);
        Assert.AreEqual(true, result.Result.DoublePoints);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenMatchIsNotFound()
    {
        // Arrange
        var matchDTO = new MatchDTO
        {
            Id = 999, // Invalid MatchId
            TournamentId = 1,
            LocalId = 1,
            VisitorId = 2,
            Date = DateTime.Now
        };

        // Act
        var result = await _matchesRepository.UpdateAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR012", result.Message);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnTotalRecords_WhenMatchesExist()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var match1 = new Match { Id = 1, Tournament = tournament, Local = team1, Visitor = team2, Date = DateTime.Now };
        var match2 = new Match { Id = 2, Tournament = tournament, Local = team2, Visitor = team1, Date = DateTime.Now };

        _context.Tournaments.Add(tournament);
        _context.Teams.AddRange(team1, team2);
        _context.Matches.AddRange(match1, match2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Page = 1,
            RecordsNumber = 10
        };

        // Act
        var result = await _matchesRepository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnError_WhenMatchDoesNotExist()
    {
        // Act
        var result = await _matchesRepository.GetAsync(999); // Non-existent match id

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR001", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenMatchNotFound()
    {
        // Arrange
        var matchDTO = new MatchDTO
        {
            Id = 999, // Non-existent match
            TournamentId = 1,
            LocalId = 1,
            VisitorId = 2,
            Date = DateTime.Now
        };

        // Act
        var result = await _matchesRepository.UpdateAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR012", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenTournamentNotFound()
    {
        // Arrange
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var match = new Match { Id = 1, Local = team1, Visitor = team2, Date = DateTime.Now };

        _context.Teams.AddRange(team1, team2);
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        var matchDTO = new MatchDTO
        {
            Id = match.Id,
            TournamentId = 999, // Non-existent tournament
            LocalId = team1.Id,
            VisitorId = team2.Id,
            Date = DateTime.Now
        };

        // Act
        var result = await _matchesRepository.UpdateAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR009", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenLocalTeamNotFound()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var match = new Match { Id = 1, Tournament = tournament, Local = team2, Visitor = team2, Date = DateTime.Now };

        _context.Tournaments.Add(tournament);
        _context.Teams.Add(team2);
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        var matchDTO = new MatchDTO
        {
            Id = match.Id,
            TournamentId = tournament.Id,
            LocalId = 999, // Non-existent local team
            VisitorId = team2.Id,
            Date = DateTime.Now
        };

        // Act
        var result = await _matchesRepository.UpdateAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR010", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenVisitorTeamNotFound()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var match = new Match { Id = 1, Tournament = tournament, Local = team1, Visitor = team1, Date = DateTime.Now };

        _context.Tournaments.Add(tournament);
        _context.Teams.Add(team1);
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        var matchDTO = new MatchDTO
        {
            Id = match.Id,
            TournamentId = tournament.Id,
            LocalId = team1.Id,
            VisitorId = 999, // Non-existent visitor team
            Date = DateTime.Now
        };

        // Act
        var result = await _matchesRepository.UpdateAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR011", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnSuccess_WhenUpdateIsValid()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var match = new Match { Id = 1, Tournament = tournament, Local = team1, Visitor = team2, Date = DateTime.Now };

        _context.Tournaments.Add(tournament);
        _context.Teams.AddRange(team1, team2);
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        var matchDTO = new MatchDTO
        {
            Id = match.Id,
            TournamentId = tournament.Id,
            LocalId = team1.Id,
            VisitorId = team2.Id,
            Date = DateTime.Now.AddDays(1),
            GoalsLocal = 2,
            GoalsVisitor = 1,
            IsActive = false,
            DoublePoints = true
        };

        // Act
        var result = await _matchesRepository.UpdateAsync(matchDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(false, result.Result!.IsActive);
        Assert.AreEqual(true, result.Result.DoublePoints);
        Assert.AreEqual(2, result.Result.GoalsLocal);
        Assert.AreEqual(1, result.Result.GoalsVisitor);
    }

    [TestMethod]
    public async Task CloseMatchAsync_ShouldUpdatePredictions_WhenMatchIsClosed()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var match = new Match { Id = 1, GoalsLocal = 2, GoalsVisitor = 1 };
        var prediction1 = new Prediction
        {
            Id = 1,
            TournamentId = 1,
            GroupId = 1,
            UserId = userId,
            MatchId = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1
        }; // Exact prediction
        var prediction2 = new Prediction
        {
            Id = 2,
            TournamentId = 1,
            GroupId = 1,
            UserId = userId,
            MatchId = 1,
            GoalsLocal = 2,
            GoalsVisitor = 0
        }; // Partially correct

        _context.Matches.Add(match);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        // Act
        await _matchesRepository.CloseMatchAsync(match);

        // Assert
        var updatedPrediction1 = await _context.Predictions.FindAsync(1);
        var updatedPrediction2 = await _context.Predictions.FindAsync(2);

        Assert.AreEqual(10, updatedPrediction1!.Points); // Exact match should have more points
        Assert.AreEqual(7, updatedPrediction2!.Points);  // Partially correct prediction
    }

    [TestMethod]
    public void CalculatePoints_ShouldReturnCorrectPoints_WhenPredictionMatchesExactScore()
    {
        // Arrange
        var match = new Match { GoalsLocal = 3, GoalsVisitor = 1, DoublePoints = false };
        var prediction = new Prediction { GoalsLocal = 3, GoalsVisitor = 1 };

        // Act
        var points = _matchesRepository.CalculatePoints(match, prediction);

        // Assert
        Assert.AreEqual(10, points); // 5 points for correct outcome, 2 + 2 for exact score
    }

    [TestMethod]
    public void CalculatePoints_ShouldReturnHalfPoints_WhenPredictionMatchesOutcomeOnly()
    {
        // Arrange
        var match = new Match { GoalsLocal = 3, GoalsVisitor = 1, DoublePoints = false };
        var prediction = new Prediction { GoalsLocal = 2, GoalsVisitor = 0 }; // Correct outcome but different score

        // Act
        var points = _matchesRepository.CalculatePoints(match, prediction);

        // Assert
        Assert.AreEqual(6, points); // 5 points for correct outcome
    }

    [TestMethod]
    public void CalculatePoints_ShouldReturnZeroPoints_WhenPredictionIsIncorrect()
    {
        // Arrange
        var match = new Match { GoalsLocal = 1, GoalsVisitor = 3, DoublePoints = false };
        var prediction = new Prediction { GoalsLocal = 2, GoalsVisitor = 1 }; // Completely incorrect

        // Act
        var points = _matchesRepository.CalculatePoints(match, prediction);

        // Assert
        Assert.AreEqual(0, points); // No points for incorrect prediction
    }

    [TestMethod]
    public void CalculatePoints_ShouldDoublePoints_WhenDoublePointsIsEnabled()
    {
        // Arrange
        var match = new Match { GoalsLocal = 3, GoalsVisitor = 1, DoublePoints = true };
        var prediction = new Prediction { GoalsLocal = 3, GoalsVisitor = 1 }; // Exact match

        // Act
        var points = _matchesRepository.CalculatePoints(match, prediction);

        // Assert
        Assert.AreEqual(20, points); // 10 points doubled
    }

    [TestMethod]
    public void GetMatchStatus_ShouldReturnLocalWin_WhenLocalTeamScoresMoreGoals()
    {
        // Arrange
        var goalsLocal = 3;
        var goalsVisitor = 1;

        // Act
        var status = _matchesRepository.GetMatchStatus(goalsLocal, goalsVisitor);

        // Assert
        Assert.AreEqual(MatchStatus.LocalWin, status);
    }

    [TestMethod]
    public void GetMatchStatus_ShouldReturnVisitorWin_WhenVisitorTeamScoresMoreGoals()
    {
        // Arrange
        var goalsLocal = 1;
        var goalsVisitor = 3;

        // Act
        var status = _matchesRepository.GetMatchStatus(goalsLocal, goalsVisitor);

        // Assert
        Assert.AreEqual(MatchStatus.VisitorWin, status);
    }

    [TestMethod]
    public void GetMatchStatus_ShouldReturnTie_WhenBothTeamsScoreEqualGoals()
    {
        // Arrange
        var goalsLocal = 2;
        var goalsVisitor = 2;

        // Act
        var status = _matchesRepository.GetMatchStatus(goalsLocal, goalsVisitor);

        // Assert
        Assert.AreEqual(MatchStatus.Tie, status);
    }

    [TestMethod]
    public void CalculatePoints_ShouldReturnZero_WhenGoalsInPredictionAreNull()
    {
        // Arrange
        var match = new Match { GoalsLocal = 2, GoalsVisitor = 1, DoublePoints = false };
        var prediction = new Prediction { GoalsLocal = null, GoalsVisitor = null }; // Both goals are null

        // Act
        var points = _matchesRepository.CalculatePoints(match, prediction);

        // Assert
        Assert.AreEqual(0, points); // Should return 0 because goals in the prediction are null
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenDbUpdateExceptionOccurs_ForMatch()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Add related entities to ensure the UpdateAsync process doesn't fail early
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var localTeam = new Team { Id = 1, Name = "Team A" };
        var visitorTeam = new Team { Id = 2, Name = "Team B" };

        context.Tournaments.Add(tournament);
        context.Teams.Add(localTeam);
        context.Teams.Add(visitorTeam);

        var match = new Match { Id = 1, Tournament = tournament, Local = localTeam, Visitor = visitorTeam, Date = DateTime.Now };
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        // Use FakeDbContext to simulate DbUpdateException
        var fakeContext = new FakeDbContext(options);
        var repository = new MatchesRepository(fakeContext);

        var matchDTO = new MatchDTO
        {
            Id = 1,
            TournamentId = tournament.Id,
            LocalId = localTeam.Id,
            VisitorId = visitorTeam.Id,
            Date = DateTime.Now.AddDays(1),
            GoalsLocal = 2,
            GoalsVisitor = 1
        };

        // Act
        var result = await repository.UpdateAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message);  // Check for the correct error message for DbUpdateException
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenGeneralExceptionOccurs_ForMatch()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Create and add entities directly to the context (no need to re-add them later).
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var localTeam = new Team { Id = 1, Name = "Team A" };
        var visitorTeam = new Team { Id = 2, Name = "Team B" };
        var match = new Match
        {
            Id = 1,
            Date = DateTime.Now,
            Local = localTeam,
            Visitor = visitorTeam,
            Tournament = tournament
        };

        context.Tournaments.Add(tournament);
        context.Teams.AddRange(localTeam, visitorTeam);
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        // Use the FakeDbContextWithGeneralException to simulate an exception.
        var fakeContext = new FakeDbContextWithGeneralException(options);
        var repository = new MatchesRepository(fakeContext);
        var matchDTO = new MatchDTO
        {
            Id = 1,
            TournamentId = 1,
            LocalId = 1,
            VisitorId = 2,
            GoalsLocal = 2,
            GoalsVisitor = 1,
            Date = DateTime.Now,
            IsActive = true,
            DoublePoints = false
        };

        // Act
        var result = await repository.UpdateAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenDbUpdateExceptionOccurs_ForMatch()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Add related entities to ensure the AddAsync process doesn't fail early
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var localTeam = new Team { Id = 1, Name = "Team A" };
        var visitorTeam = new Team { Id = 2, Name = "Team B" };

        context.Tournaments.Add(tournament);
        context.Teams.Add(localTeam);
        context.Teams.Add(visitorTeam);
        await context.SaveChangesAsync();

        // Use FakeDbContext to simulate DbUpdateException
        var fakeContext = new FakeDbContext(options);
        var repository = new MatchesRepository(fakeContext);

        var matchDTO = new MatchDTO
        {
            TournamentId = tournament.Id,
            LocalId = localTeam.Id,
            VisitorId = visitorTeam.Id,
            Date = DateTime.Now.AddDays(1),
            DoublePoints = true
        };

        // Act
        var result = await repository.AddAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message);  // Check for the correct error message for DbUpdateException
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenGeneralExceptionOccurs_ForMatch()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Add related entities to ensure the AddAsync process doesn't fail early
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var localTeam = new Team { Id = 1, Name = "Team A" };
        var visitorTeam = new Team { Id = 2, Name = "Team B" };

        context.Tournaments.Add(tournament);
        context.Teams.Add(localTeam);
        context.Teams.Add(visitorTeam);
        await context.SaveChangesAsync();

        // Use FakeDbContextWithGeneralException to simulate a general exception
        var fakeContext = new FakeDbContextWithGeneralException(options);
        var repository = new MatchesRepository(fakeContext);

        var matchDTO = new MatchDTO
        {
            TournamentId = tournament.Id,
            LocalId = localTeam.Id,
            VisitorId = visitorTeam.Id,
            Date = DateTime.Now.AddDays(1),
            DoublePoints = true
        };

        // Act
        var result = await repository.AddAsync(matchDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message);  // Check for the correct error message for general exception
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFilteredMatches_WhenFilterIsApplied()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Create and add some test data
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var localTeam1 = new Team { Id = 1, Name = "Team A" };
        var visitorTeam1 = new Team { Id = 2, Name = "Team B" };
        var localTeam2 = new Team { Id = 3, Name = "Team C" };
        var visitorTeam2 = new Team { Id = 4, Name = "Team D" };

        context.Tournaments.Add(tournament);
        context.Teams.AddRange(localTeam1, visitorTeam1, localTeam2, visitorTeam2);

        var match1 = new Match
        {
            Id = 1,
            TournamentId = 1,
            Local = localTeam1,
            Visitor = visitorTeam1,
            Date = DateTime.Now.AddDays(1),
            IsActive = true
        };
        var match2 = new Match
        {
            Id = 2,
            TournamentId = 1,
            Local = localTeam2,
            Visitor = visitorTeam2,
            Date = DateTime.Now.AddDays(2),
            IsActive = true
        };

        context.Matches.AddRange(match1, match2);
        await context.SaveChangesAsync();

        var repository = new MatchesRepository(context);

        // Create a PaginationDTO with a filter that matches "Team A" (local) or "Team B" (visitor)
        var pagination = new PaginationDTO
        {
            Id = 1,  // Tournament ID to filter by
            Filter = "Team A",
            Page = 1,
            RecordsNumber = 10
        };

        // Act
        var result = await repository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());  // Only match1 should match the filter
        Assert.AreEqual("Team A", result.Result!.First().Local.Name);  // Ensure match1 is returned
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnMatch_WhenMatchExists()
    {
        // Create a unique in-memory database for this test
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique database name
            .Options;

        // Create the data context
        using var context = new DataContext(options);
        var matchesRepository = new MatchesRepository(context);

        // Arrange: Set up the data
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var match = new Match
        {
            Id = 1,
            Tournament = tournament,
            Local = team1,
            Visitor = team2,
            Date = DateTime.Now
        };

        // Add the data to the context and save changes
        context.Tournaments.Add(tournament);
        context.Teams.AddRange(team1, team2);
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        // Act: Execute the method being tested
        var result = await matchesRepository.GetAsync(match.Id);

        // Assert: Verify the result
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(match.Id, result.Result!.Id);
        Assert.AreEqual(match.Local.Name, result.Result.Local.Name);
        Assert.AreEqual(match.Visitor.Name, result.Result.Visitor.Name);
        Assert.AreEqual(0, result.Result.PredictionsCount);
        Assert.AreEqual(match.Date.ToLocalTime(), result.Result.DateLocal);
    }
}