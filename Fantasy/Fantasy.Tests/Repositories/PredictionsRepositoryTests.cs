using Fantasy.Backend.Data;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Tests.General;
using Microsoft.EntityFrameworkCore;
using Moq;
using Match = Fantasy.Shared.Entities.Match;

namespace Fantasy.Tests.Repositories;

[TestClass]
public class PredictionsRepositoryTests
{
    private DataContext _context = null!;
    private PredictionsRepository _predictionsRepository = null!;
    private Mock<IUsersRepository> _usersRepositoryMock = null!;

    [TestInitialize]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "PredictionsTestDb")
            .Options;

        _context = new DataContext(options);
        _usersRepositoryMock = new Mock<IUsersRepository>();

        _predictionsRepository = new PredictionsRepository(_context, _usersRepositoryMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetAsync_ByPagination_ShouldReturnFilteredPredictions()
    {
        // Arrange
        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var match1 = new Match
        {
            Id = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now.AddDays(1)
        };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match1
        };

        _context.Groups.Add(group);
        _context.Users.Add(user);
        _context.Predictions.Add(prediction1);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Email = "test@example.com",
            Page = 1,
            RecordsNumber = 10
        };

        // Act
        var result = await _predictionsRepository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetAsync_ById_ShouldReturnPrediction_WhenExists()
    {
        // Arrange
        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var match = new Match
        {
            Id = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now
        };

        var prediction = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match
        };

        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _predictionsRepository.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Id);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        var predictionDTO = new PredictionDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1, MatchId = 1 };
        _usersRepositoryMock.Setup(u => u.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync((User)null!);

        // Act
        var result = await _predictionsRepository.AddAsync(predictionDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR013", result.Message);  // Error for user not found
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenGroupNotFound()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        _usersRepositoryMock.Setup(u => u.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        var predictionDTO = new PredictionDTO { UserId = user.Id.ToString(), GroupId = 999, MatchId = 1 };

        // Act
        var result = await _predictionsRepository.AddAsync(predictionDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR014", result.Message);  // Error for group not found
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenPredictionIsLocked()
    {
        // Arrange
        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var match = new Match
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now
        };

        var prediction = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match
        };

        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var predictionDTO = new PredictionDTO
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1
        };

        // Act
        var result = await _predictionsRepository.UpdateAsync(predictionDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR018", result.Message);  // Error for locked prediction
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var match = new Match
        {
            Id = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now
        };

        var prediction = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match
        };

        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Email = "test@example.com",
            Page = 1,
            RecordsNumber = 10
        };

        // Act
        var result = await _predictionsRepository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result);
    }

    [TestMethod]
    public async Task GetPositionsAsync_ShouldReturnCorrectPositions()
    {
        // Arrange
        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var prediction = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Points = 10
        };

        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Page = 1,
            RecordsNumber = 10
        };

        // Act
        var result = await _predictionsRepository.GetPositionsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
        Assert.AreEqual(10, result.Result!.First().Points);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnFilteredPredictions_WhenFilterIsApplied()
    {
        // Arrange
        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var match1 = new Match
        {
            Id = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now.AddDays(1)
        };

        var match2 = new Match
        {
            Id = 2,
            Local = new Team { Name = "Team C" },
            Visitor = new Team { Name = "Team D" },
            Date = DateTime.Now.AddDays(2)
        };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match1
        };

        var prediction2 = new Prediction
        {
            Id = 2,
            Group = group,
            User = user,
            Match = match2
        };

        _context.Groups.Add(group);
        _context.Users.Add(user);
        _context.Matches.AddRange(match1, match2);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Email = "test@example.com",
            Page = 1,
            RecordsNumber = 10,
            Filter = "Team A"
        };

        // Act
        var result = await _predictionsRepository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
        Assert.AreEqual("Team A", result.Result!.First().Match.Local.Name);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnError_WhenPredictionIsNull()
    {
        // Arrange
        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _context.Groups.Add(group);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _predictionsRepository.GetAsync(999);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR001", result.Message);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenTournamentIsNotFound()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", FirstName = "John", LastName = "Doe" };
        var group = new Group { Id = 1, Name = "Group A", AdminId = Guid.NewGuid().ToString(), Code = "GRP123" };

        _usersRepositoryMock.Setup(u => u.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        var predictionDTO = new PredictionDTO
        {
            UserId = user.Id.ToString(),
            GroupId = 1,
            TournamentId = 999,  // Invalid TournamentId
            MatchId = 1
        };

        // Act
        var result = await _predictionsRepository.AddAsync(predictionDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR009", result.Message);  // Error for missing tournament
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenMatchIsNotFound()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", FirstName = "John", LastName = "Doe" };
        var group = new Group { Id = 1, Name = "Group A", AdminId = Guid.NewGuid().ToString(), Code = "GRP123" };
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };

        _usersRepositoryMock.Setup(u => u.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _context.Groups.Add(group);
        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync();

        var predictionDTO = new PredictionDTO
        {
            UserId = user.Id.ToString(),
            GroupId = 1,
            TournamentId = 1,   // Valid TournamentId
            MatchId = 999       // Invalid MatchId
        };

        // Act
        var result = await _predictionsRepository.AddAsync(predictionDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR012", result.Message);  // Error for missing match
    }

    [TestMethod]
    public async Task AddAsync_ShouldCreatePrediction_WhenAllDataIsValid()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", FirstName = "John", LastName = "Doe" };
        var group = new Group { Id = 1, Name = "Group A", AdminId = Guid.NewGuid().ToString(), Code = "GRP123" };
        var tournament = new Tournament { Id = 1, Name = "Tournament A" };
        var match = new Match { Id = 1, Local = new Team { Name = "Team A" }, Visitor = new Team { Name = "Team B" }, Date = DateTime.Now };

        _usersRepositoryMock.Setup(u => u.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _context.Groups.Add(group);
        _context.Tournaments.Add(tournament);
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        var predictionDTO = new PredictionDTO
        {
            UserId = user.Id.ToString(),
            GroupId = 1,
            TournamentId = 1,
            MatchId = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1
        };

        // Act
        var result = await _predictionsRepository.AddAsync(predictionDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(2, result.Result.GoalsLocal);
        Assert.AreEqual(1, result.Result.GoalsVisitor);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnCorrectCount_WhenFilterIsApplied()
    {
        // Arrange
        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var match1 = new Match
        {
            Id = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now
        };

        var match2 = new Match
        {
            Id = 2,
            Local = new Team { Name = "Team C" },
            Visitor = new Team { Name = "Team D" },
            Date = DateTime.Now
        };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match1
        };

        var prediction2 = new Prediction
        {
            Id = 2,
            Group = group,
            User = user,
            Match = match2
        };

        _context.Groups.Add(group);
        _context.Users.Add(user);
        _context.Matches.AddRange(match1, match2);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Email = "test@example.com",
            Filter = "Team A"  // Applying filter for "Team A"
        };

        // Act
        var result = await _predictionsRepository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result); // Only one prediction should match the filter
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenPredictionIsNotFound()
    {
        // Arrange
        var predictionDTO = new PredictionDTO
        {
            Id = 999,  // Invalid Id
            GoalsLocal = 2,
            GoalsVisitor = 1
        };

        // Act
        var result = await _predictionsRepository.UpdateAsync(predictionDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR016", result.Message);  // Error for missing prediction
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenMatchHasGoalsAlreadySet()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",  // FirstName is required
            LastName = "Doe"     // LastName is required
        };

        var match = new Match
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1  // Goals already set
        };

        var prediction = new Prediction
        {
            Id = 1,
            User = user,
            Match = match
        };

        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var predictionDTO = new PredictionDTO
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1
        };

        // Act
        var result = await _predictionsRepository.UpdateAsync(predictionDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR018", result.Message);  // Error for prediction being locked
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenCanWatchReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var match = new Match
        {
            Id = 1,
            GoalsLocal = null,
            GoalsVisitor = null
        };

        var prediction = new Prediction
        {
            Id = 1,
            User = user,
            Match = match
        };

        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var predictionDTO = new PredictionDTO
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1
        };

        // Create a repository with CanWatch returning true
        var testRepository = new TestablePredictionsRepository(_context, _usersRepositoryMock.Object, true);

        // Act
        var result = await testRepository.UpdateAsync(predictionDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR018", result.Message);  // Error for CanWatch returning true
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdatePrediction_WhenDataIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",  // FirstName is required
            LastName = "Doe"     // LastName is required
        };

        var match = new Match
        {
            Id = 1,
            GoalsLocal = null,
            GoalsVisitor = null  // No goals set
        };

        var prediction = new Prediction
        {
            Id = 1,
            User = user,
            Match = match
        };

        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var predictionDTO = new PredictionDTO
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1,
            Points = 5
        };

        // Create a repository with CanWatch returning false
        var testRepository = new TestablePredictionsRepository(_context, _usersRepositoryMock.Object, false);

        // Act
        var result = await testRepository.UpdateAsync(predictionDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.GoalsLocal);
        Assert.AreEqual(1, result.Result!.GoalsVisitor);
        Assert.AreEqual(5, result.Result!.Points);
    }

    [TestMethod]
    public async Task GetPositionsAsync_ShouldReturnFilteredPositions_WhenFilterIsApplied()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user1,
            Points = 10
        };

        var prediction2 = new Prediction
        {
            Id = 2,
            Group = group,
            User = user2,
            Points = 5
        };

        _context.Groups.Add(group);
        _context.Users.AddRange(user1, user2);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Page = 1,
            RecordsNumber = 10,
            Filter = "John"  // Applying filter for "John"
        };

        // Act
        var result = await _predictionsRepository.GetPositionsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count()); // Only one user should match the filter
        Assert.AreEqual("John", result.Result!.First().User.FirstName); // Ensure the filtered result is correct
    }

    [TestMethod]
    public async Task GetTotalRecordsForPositions2Async_ShouldReturnCorrectCount_WhenFilterIsApplied()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(), // Providing required AdminId
            Code = "GRP123"                      // Providing required Code
        };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user1,
            Points = 10
        };

        var prediction2 = new Prediction
        {
            Id = 2,
            Group = group,
            User = user2,
            Points = 5
        };

        _context.Groups.Add(group);
        _context.Users.AddRange(user1, user2);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Filter = "John"  // Applying filter
        };

        // Act
        var result = await _predictionsRepository.GetTotalRecordsForPositionsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result); // Only one user should match the filter
    }

    [TestMethod]
    public async Task GetTotalRecordsForPositionsAsync_ShouldReturnCorrectCount_WhenFilterIsApplied()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(), // Providing required AdminId
            Code = "GRP123"                      // Providing required Code
        };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user1,
            Points = 10
        };

        var prediction2 = new Prediction
        {
            Id = 2,
            Group = group,
            User = user2,
            Points = 5
        };

        _context.Groups.Add(group);
        _context.Users.AddRange(user1, user2);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Filter = "John"  // Applying filter
        };

        // Act
        var result = await _predictionsRepository.GetTotalRecordsForPositionsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result); // Only one user should match the filter
    }

    [TestMethod]
    public async Task GetAllPredictionsAsync_ShouldReturnAllPredictions_WhenNoFilterIsApplied()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),    // Providing required AdminId
            Code = "GRP123"                         // Providing required Code
        };

        var match = new Match
        {
            Id = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now
        };

        var prediction = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match
        };

        _context.Groups.Add(group);
        _context.Users.Add(user);
        _context.Matches.Add(match);
        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Id2 = 1
        };

        // Act
        var result = await _predictionsRepository.GetAllPredictionsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetAllPredictionsAsync_ShouldReturnFilteredPredictions_WhenFilterIsApplied()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),    // Providing required AdminId
            Code = "GRP123"                         // Providing required Code
        };

        var match = new Match
        {
            Id = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now
        };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user1,
            Match = match
        };

        var prediction2 = new Prediction
        {
            Id = 2,
            Group = group,
            User = user2,
            Match = match
        };

        _context.Groups.Add(group);
        _context.Users.AddRange(user1, user2);
        _context.Matches.Add(match);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Id2 = 1,
            Filter = "John"
        };

        // Act
        var result = await _predictionsRepository.GetAllPredictionsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count()); // Only one user should match the filter
        Assert.AreEqual("John", result.Result!.First().User.FirstName);
    }

    [TestMethod]
    public async Task GetTotalRecordsAllPredictionsAsync_ShouldReturnCorrectCount_WhenNoFilterIsApplied()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",            // Providing required FirstName
            LastName = "Doe"               // Providing required LastName
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),    // Providing required AdminId
            Code = "GRP123"                         // Providing required Code
        };

        var match = new Match { Id = 1 };

        var prediction = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match
        };

        _context.Groups.Add(group);
        _context.Users.Add(user);
        _context.Matches.Add(match);
        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Id2 = 1
        };

        // Act
        var result = await _predictionsRepository.GetTotalRecordsAllPredictionsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result);
    }

    [TestMethod]
    public async Task GetTotalRecordsAllPredictionsAsync_ShouldReturnFilteredCount_WhenFilterIsApplied()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),  // Providing required AdminId
            Code = "GRP123"                       // Providing required Code
        };

        var match = new Match { Id = 1 };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user1,
            Match = match
        };

        var prediction2 = new Prediction
        {
            Id = 2,
            Group = group,
            User = user2,
            Match = match
        };

        _context.Groups.Add(group);
        _context.Users.AddRange(user1, user2);
        _context.Matches.Add(match);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Id2 = 1,
            Filter = "John"
        };

        // Act
        var result = await _predictionsRepository.GetTotalRecordsAllPredictionsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result); // Only one prediction should match the filter
    }

    [TestMethod]
    public async Task GetBalanceAsync_ShouldReturnAllPredictions_WhenNoFilterIsApplied()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),    // Providing required AdminId
            Code = "GRP123"                         // Providing required Code
        };

        var match = new Match
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now
        };

        var prediction = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match
        };

        _context.Groups.Add(group);
        _context.Users.Add(user);
        _context.Matches.Add(match);
        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Email = "test@example.com"
        };

        // Act
        var result = await _predictionsRepository.GetBalanceAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetBalanceAsync_ShouldReturnFilteredPredictions_WhenFilterIsApplied()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),    // Providing required AdminId
            Code = "GRP123"                         // Providing required Code
        };

        var match = new Match
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1,
            Local = new Team { Name = "Team A" },
            Visitor = new Team { Name = "Team B" },
            Date = DateTime.Now
        };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user1,
            Match = match
        };

        var prediction2 = new Prediction
        {
            Id = 2,
            Group = group,
            User = user2,
            Match = match
        };

        _context.Groups.Add(group);
        _context.Users.AddRange(user1, user2);
        _context.Matches.Add(match);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Email = "john@example.com",
            Filter = "Team A"
        };

        // Act
        var result = await _predictionsRepository.GetBalanceAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count()); // Only one match should match the filter
        Assert.AreEqual("Team A", result.Result!.First().Match.Local.Name);
    }

    [TestMethod]
    public async Task GetTotalRecordsBalanceAsync_ShouldReturnCorrectCount_WhenNoFilterIsApplied()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",            // Providing required FirstName
            LastName = "Doe"               // Providing required LastName
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),    // Providing required AdminId
            Code = "GRP123"                         // Providing required Code
        };

        var match = new Match
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1
        };

        var prediction = new Prediction
        {
            Id = 1,
            Group = group,
            User = user,
            Match = match
        };

        _context.Groups.Add(group);
        _context.Users.Add(user);
        _context.Matches.Add(match);
        _context.Predictions.Add(prediction);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Email = "test@example.com"
        };

        // Act
        var result = await _predictionsRepository.GetTotalRecordsBalanceAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result);
    }

    [TestMethod]
    public async Task GetTotalRecordsBalanceAsync_ShouldReturnFilteredCount_WhenFilterIsApplied()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Group A",
            AdminId = Guid.NewGuid().ToString(),
            Code = "GRP123"
        };

        // Set up the teams and the match
        var teamA = new Team { Id = 1, Name = "Team A" };
        var teamB = new Team { Id = 2, Name = "Team B" };

        var match = new Match
        {
            Id = 1,
            GoalsLocal = 2,
            GoalsVisitor = 1,
            Local = teamA,   // Ensure Local team matches the filter "Team A"
            Visitor = teamB
        };

        var prediction1 = new Prediction
        {
            Id = 1,
            Group = group,
            User = user1,
            Match = match
        };

        var prediction2 = new Prediction
        {
            Id = 2,
            Group = group,
            User = user2,
            Match = match
        };

        _context.Groups.Add(group);
        _context.Users.AddRange(user1, user2);
        _context.Teams.AddRange(teamA, teamB);   // Add the teams to the context
        _context.Matches.Add(match);
        _context.Predictions.AddRange(prediction1, prediction2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Id = 1,
            Email = "john@example.com",
            Filter = "Team A"  // Ensure the filter matches the team name
        };

        // Act
        var result = await _predictionsRepository.GetTotalRecordsBalanceAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result); // Only one prediction should match the filter
    }

    [TestMethod]
    public void CanWatch_ShouldReturnTrue_WhenGoalsAreSet()
    {
        // Arrange
        var match = new Match
        {
            GoalsLocal = 2,
            GoalsVisitor = 1
        };
        var prediction = new Prediction
        {
            Match = match
        };

        // Act
        var result = _predictionsRepository.CanWatch(prediction);

        // Assert
        Assert.IsTrue(result); // Goals are set, so the match is completed, should return true.
    }

    [TestMethod]
    public void CanWatch_ShouldReturnTrue_WhenMatchIsAboutToStart()
    {
        // Arrange
        var match = new Match
        {
            Date = DateTime.Now.AddMinutes(5) // Match starting in 5 minutes
        };
        var prediction = new Prediction
        {
            Match = match
        };

        // Act
        var result = _predictionsRepository.CanWatch(prediction);

        // Assert
        Assert.IsTrue(result); // Match is starting within 10 minutes, should return true.
    }

    [TestMethod]
    public void CanWatch_ShouldReturnFalse_WhenMatchIsMoreThan10MinutesAway()
    {
        // Arrange
        var match = new Match
        {
            Date = DateTime.Now.AddMinutes(15) // Match starting in 15 minutes
        };
        var prediction = new Prediction
        {
            Match = match
        };

        // Act
        var result = _predictionsRepository.CanWatch(prediction);

        // Assert
        Assert.IsFalse(result); // Match is more than 10 minutes away, should return false.
    }

    [TestMethod]
    public void CanWatch_ShouldReturnTrue_WhenMatchHasStarted()
    {
        // Arrange
        var match = new Match
        {
            Date = DateTime.Now.AddMinutes(-5) // Match started 5 minutes ago
        };
        var prediction = new Prediction
        {
            Match = match
        };

        // Act
        var result = _predictionsRepository.CanWatch(prediction);

        // Assert
        Assert.IsTrue(result); // Match has already started, should return true.
    }

    [TestMethod]
    public void CanWatch_ShouldReturnTrue_WhenMatchStartedMoreThan10MinutesAgo()
    {
        // Arrange
        var match = new Match
        {
            Date = DateTime.Now.AddMinutes(-15) // Match started 15 minutes ago
        };
        var prediction = new Prediction
        {
            Match = match
        };

        // Act
        var result = _predictionsRepository.CanWatch(prediction);

        // Assert
        Assert.IsTrue(result); // Match was more than 10 minutes ago, but the current logic allows watching even if the match has started.
    }
}