using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Match = Fantasy.Shared.Entities.Match;

namespace Fantasy.Tests.Repositories;

[TestClass]
public class GroupsRepositoryTests
{
    private DataContext _context = null!;
    private IFileStorage _fileStorageMock = null!;
    private IUsersRepository _usersRepositoryMock = null!;
    private GroupsRepository _groupsRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "GroupsTestDb")
            .Options;

        _context = new DataContext(options);
        _fileStorageMock = Mock.Of<IFileStorage>();
        _usersRepositoryMock = Mock.Of<IUsersRepository>();

        _groupsRepository = new GroupsRepository(_context, _fileStorageMock, _usersRepositoryMock);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddGroupSuccessfully()
    {
        // Arrange
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var tournament = new Tournament { Id = 1, Name = "Test Tournament" };
        var groupDTO = new GroupDTO
        {
            AdminId = admin.Id,
            TournamentId = tournament.Id,
            Name = "Test Group",
            Remarks = "Test Remarks",
            Image = null
        };

        // Add the admin to the in-memory context (this is necessary to simulate the real DB behavior)
        _context.Users.Add(admin);
        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync();

        // Mock the admin retrieval
        Mock.Get(_usersRepositoryMock)
            .Setup(repo => repo.GetUserAsync(admin.Id))
            .ReturnsAsync(admin);

        // Verify that the admin and tournament were added correctly
        var adminInDb = await _context.Users.FirstOrDefaultAsync(x => x.Id == admin.Id);
        Assert.IsNotNull(adminInDb);

        var tournamentInDb = await _context.Tournaments.FirstOrDefaultAsync(x => x.Id == tournament.Id);
        Assert.IsNotNull(tournamentInDb);

        // Act
        var response = await _groupsRepository.AddAsync(groupDTO);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual("Test Group", response.Result.Name);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnErrorWhenTournamentNotFound()
    {
        // Arrange
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var groupDTO = new GroupDTO
        {
            AdminId = admin.Id,
            TournamentId = 999, // ID de torneo inexistente
            Name = "Test Group",
            Remarks = "Test Remarks",
            Image = null
        };

        // Add the admin to the in-memory context
        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        // Mock the admin retrieval
        Mock.Get(_usersRepositoryMock)
            .Setup(repo => repo.GetUserAsync(admin.Id))
            .ReturnsAsync(admin);

        // Act
        var response = await _groupsRepository.AddAsync(groupDTO);

        // Assert
        Assert.IsFalse(response.WasSuccess);
        Assert.AreEqual("ERR009", response.Message);
        Assert.IsNull(response.Result);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnErrorWhenAdminNotFound()
    {
        // Arrange
        var groupDTO = new GroupDTO
        {
            AdminId = Guid.NewGuid().ToString(),
            TournamentId = 1,
            Name = "Test Group",
            Remarks = "Test Remarks",
            Image = null
        };

        Mock.Get(_usersRepositoryMock)
            .Setup(repo => repo.GetUserAsync(groupDTO.AdminId))
            .ReturnsAsync((User)null!);

        // Act
        var response = await _groupsRepository.AddAsync(groupDTO);

        // Assert
        Assert.IsFalse(response.WasSuccess);
        Assert.AreEqual("ERR013", response.Message);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnGroup_WhenGroupExists()
    {
        // Arrange
        // Create an admin user
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Create a tournament
        var tournament = new Tournament
        {
            Id = 1,
            Name = "Test Tournament"
        };

        // Create a group with required fields (Admin, Code, and Tournament)
        var group = new Group
        {
            Id = 1,
            Name = "Test Group",
            Admin = admin, // Assign the required Admin
            Code = "ABC123", // Provide a unique code for the group
            IsActive = true,
            Tournament = tournament, // Assign the required Tournament
            Members = new List<UserGroup> { new UserGroup { User = admin } }
        };

        // Add the admin, tournament, and group to the in-memory database
        _context.Users.Add(admin);
        _context.Tournaments.Add(tournament);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Act
        // Call the method to retrieve the group by its ID
        var response = await _groupsRepository.GetAsync(group.Id);

        // Assert
        // Check if the result was successful and the group ID matches
        Assert.IsTrue(response.WasSuccess);
        Assert.AreEqual(group.Id, response.Result!.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnError_WhenGroupDoesNotExist()
    {
        // Arrange
        var nonExistentGroupId = 999; // This ID does not exist in the in-memory database

        // Act
        var response = await _groupsRepository.GetAsync(nonExistentGroupId);

        // Assert
        Assert.IsFalse(response.WasSuccess);
        Assert.AreEqual("ERR001", response.Message);
        Assert.IsNull(response.Result); // The result should be null since the group doesn't exist
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateGroupSuccessfully()
    {
        // Arrange
        // Create an admin user
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Create a group with the required fields (AdminId and Code)
        var group = new Group
        {
            Id = 1,
            Name = "Old Name",
            Remarks = "Old Remarks",
            Admin = admin, // Assign the required Admin
            Code = "ABC123", // Provide a valid Code
            IsActive = true
        };

        // Add the admin and group to the in-memory database
        _context.Users.Add(admin);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Prepare the DTO with updated values
        var groupDTO = new GroupDTO
        {
            Id = group.Id,
            Name = "New Name",
            Remarks = "New Remarks",
            IsActive = true
        };

        // Act
        var response = await _groupsRepository.UpdateAsync(groupDTO);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.AreEqual("New Name", response.Result!.Name);
        Assert.AreEqual("New Remarks", response.Result.Remarks);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSaveImage_WhenImageIsProvided()
    {
        // Arrange
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var group = new Group
        {
            Id = 1,
            Name = "Old Name",
            Remarks = "Old Remarks",
            Admin = admin,
            Code = "ABC123",
            IsActive = true,
            Image = null // Initially, the group has no image
        };

        // Add the admin and group to the in-memory database
        _context.Users.Add(admin);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Mock the file storage to simulate saving the image
        var imageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }); // Example base64-encoded image
        var savedImagePath = "saved-image-path.jpg"; // The path returned by the mock
        Mock.Get(_fileStorageMock)
            .Setup(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "groups"))
            .ReturnsAsync(savedImagePath);

        var groupDTO = new GroupDTO
        {
            Id = group.Id,
            Name = "New Name",
            Remarks = "New Remarks",
            IsActive = true,
            Image = imageBase64 // Provide a base64-encoded image
        };

        // Act
        var response = await _groupsRepository.UpdateAsync(groupDTO);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.AreEqual("New Name", response.Result!.Name);
        Assert.AreEqual("New Remarks", response.Result.Remarks);
        Assert.AreEqual(savedImagePath, response.Result.Image); // Ensure the image path was updated

        // Verify that the SaveFileAsync method was called with the correct arguments
        Mock.Get(_fileStorageMock).Verify(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "groups"), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenGroupNotFound()
    {
        // Arrange
        var groupDTO = new GroupDTO { Id = 999, Name = "New Name", Remarks = "New Remarks" };

        // Act
        var response = await _groupsRepository.UpdateAsync(groupDTO);

        // Assert
        Assert.IsFalse(response.WasSuccess);
        Assert.AreEqual("ERR014", response.Message);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user@example.com",
            FirstName = "Jane",
            LastName = "Doe"
        };

        _context.Users.Add(admin);
        _context.Users.Add(user);

        var group1 = new Group
        {
            Name = "Group 1",
            Admin = admin,
            Code = "ABC123", // Provide a valid code
            IsActive = true,
            Members = new List<UserGroup> { new() { User = user } }
        };

        var group2 = new Group
        {
            Name = "Group 2",
            Admin = admin,
            Code = "DEF456", // Provide a valid code
            IsActive = true,
            Members = new List<UserGroup> { new() { User = user } }
        };

        _context.Groups.Add(group1);
        _context.Groups.Add(group2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO { Email = user.Email, Filter = "G" };

        // Act
        var response = await _groupsRepository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.AreEqual(2, response.Result);
    }

    [TestMethod]
    public async Task CheckPredictionsForAllMatchesAsync_ShouldAddNewPredictions_WhenNoPredictionsExist()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user1@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "Admin"
        };

        var localTeam = new Team { Id = 1, Name = "Local Team" };
        var visitorTeam = new Team { Id = 2, Name = "Visitor Team" };

        var group = new Group
        {
            Id = 1,
            Code = "ABC123",
            Name = "Test Group",
            TournamentId = 1,
            Admin = admin,
            Members = [new UserGroup { User = user }]
        };

        var tournament = new Tournament
        {
            Id = group.TournamentId,
            Name = "Test Tournament"
        };

        var match = new Match
        {
            Id = 1,
            Tournament = tournament,
            TournamentId = tournament.Id,
            Date = DateTime.UtcNow,
            IsActive = true,
            Local = localTeam,
            LocalId = localTeam.Id,
            Visitor = visitorTeam,
            VisitorId = visitorTeam.Id
        };

        _context.Users.Add(user);
        _context.Users.Add(admin);
        _context.Teams.Add(localTeam);
        _context.Teams.Add(visitorTeam);
        _context.Groups.Add(group);
        _context.Tournaments.Add(tournament);
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        // Act
        await _groupsRepository.CheckPredictionsForAllMatchesAsync(group.Id);

        // Assert
        var predictions = await _context.Predictions.Where(p => p.GroupId == group.Id).ToListAsync();
        Assert.AreEqual(1, predictions.Count); // Ensure a prediction was added
        Assert.AreEqual(match.Id, predictions.First().Match.Id); // Ensure the match ID is correct
    }

    [TestMethod]
    public async Task CheckPredictionsForAllMatchesAsync_ShouldDoNothing_WhenGroupDoesNotExist()
    {
        // Arrange
        var nonExistentGroupId = 999;

        // Act
        await _groupsRepository.CheckPredictionsForAllMatchesAsync(nonExistentGroupId);

        // Assert
        // No exception should be thrown and no predictions should be added
        var predictions = await _context.Predictions.ToListAsync();
        Assert.AreEqual(0, predictions.Count);
    }

    [TestMethod]
    public async Task CheckPredictionsForAllMatchesAsync_ShouldDoNothing_WhenTournamentHasNoMatches()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user1@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "Admin"
        };

        var tournament = new Tournament
        {
            Id = 1,
            Name = "Test Tournament",
            Matches = [] // Tournament has no matches
        };

        var group = new Group
        {
            Id = 1,
            Code = "ABC123",
            Name = "Test Group",
            TournamentId = tournament.Id,
            Admin = admin,
            Members = [new() { User = user }]
        };

        _context.Users.Add(user);
        _context.Users.Add(admin);
        _context.Groups.Add(group);
        _context.Tournaments.Add(tournament); // Add tournament without matches
        await _context.SaveChangesAsync();

        // Act
        await _groupsRepository.CheckPredictionsForAllMatchesAsync(group.Id);

        // Assert
        // No predictions should be added since the tournament has no matches
        var predictions = await _context.Predictions.ToListAsync();
        Assert.AreEqual(0, predictions.Count);
    }

    [TestMethod]
    public async Task CheckPredictionsForAllMatchesAsync_ShouldReturn_WhenTournamentDoesNotExist()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user1@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "Admin"
        };

        var group = new Group
        {
            Id = 1,
            Code = "ABC123",
            Name = "Test Group",
            TournamentId = 1, // Non-existent tournament
            Admin = admin,
            Members = new List<UserGroup> { new UserGroup { User = user } }
        };

        _context.Users.Add(user);
        _context.Users.Add(admin);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Act
        await _groupsRepository.CheckPredictionsForAllMatchesAsync(group.Id);

        // Assert
        // Since the tournament does not exist, no predictions should be added
        var predictions = await _context.Predictions.ToListAsync();
        Assert.AreEqual(0, predictions.Count);
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturnAllActiveGroups()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user1@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user2@example.com",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var admin1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin1@example.com",
            FirstName = "Admin",
            LastName = "One"
        };

        var admin2 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin2@example.com",
            FirstName = "Admin",
            LastName = "Two"
        };

        var tournament = new Tournament
        {
            Id = 1,
            Name = "Test Tournament"
        };

        var group1 = new Group
        {
            Id = 1,
            Code = "000001",
            Name = "Group 1",
            IsActive = true,
            Tournament = tournament,
            Admin = admin1, // Set the required Admin
            Members = [new UserGroup { User = user1 }]
        };

        var group2 = new Group
        {
            Id = 2,
            Code = "000002",
            Name = "Group 2",
            IsActive = true,
            Tournament = tournament,
            Admin = admin2, // Set the required Admin
            Members = [new UserGroup { User = user2 }]
        };

        var group3 = new Group
        {
            Id = 3,
            Code = "000003",
            Name = "Group 3",
            IsActive = false, // Inactive group
            Tournament = tournament,
            Admin = admin1, // Set the required Admin
            Members = [new UserGroup { User = user1 }]
        };

        _context.Users.AddRange(user1, user2, admin1, admin2);
        _context.Tournaments.Add(tournament);
        _context.Groups.AddRange(group1, group2, group3);
        await _context.SaveChangesAsync();

        // Act
        var response = await _groupsRepository.GetAllAsync();

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.AreEqual(2, response.Result!.Count()); // Only 2 active groups should be returned
        Assert.IsTrue(response.Result!.Any(g => g.Name == "Group 1"));
        Assert.IsTrue(response.Result!.Any(g => g.Name == "Group 2"));
        Assert.IsFalse(response.Result!.Any(g => g.Name == "Group 3")); // Inactive group should not be included
    }

    [TestMethod]
    public async Task GetAsyncByCode_ShouldReturnGroup_WhenGroupExists()
    {
        // Arrange
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var group = new Group
        {
            Id = 1,
            Code = "ABC123",
            Name = "Test Group",
            IsActive = true,
            Admin = admin // Set the required Admin
        };

        _context.Users.Add(admin);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Act
        var response = await _groupsRepository.GetAsync(group.Code);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual(group.Id, response.Result.Id);
        Assert.AreEqual(group.Code, response.Result.Code);
        Assert.AreEqual(group.Name, response.Result.Name);
    }

    [TestMethod]
    public async Task GetAsyncByCode_ShouldReturnError_WhenGroupDoesNotExist()
    {
        // Arrange
        var nonExistentCode = "XYZ789"; // Code for a group that doesn't exist

        // Act
        var response = await _groupsRepository.GetAsync(nonExistentCode);

        // Assert
        Assert.IsFalse(response.WasSuccess);
        Assert.AreEqual("ERR001", response.Message);
        Assert.IsNull(response.Result);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnGroups_WhenUserHasGroupsWithoutFilter()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var tournament = new Tournament
        {
            Id = 1,
            Name = "Test Tournament"
        };

        var group1 = new Group
        {
            Id = 1,
            Code = "000001",
            Name = "Group 1",
            IsActive = true,
            Tournament = tournament,
            Admin = admin,
            Members = [new UserGroup { User = user }]
        };

        var group2 = new Group
        {
            Id = 2,
            Code = "000002",
            Name = "Group 2",
            IsActive = true,
            Tournament = tournament,
            Admin = admin,
            Members = [new UserGroup { User = user }]
        };

        _context.Users.AddRange(user, admin);
        _context.Tournaments.Add(tournament);
        _context.Groups.AddRange(group1, group2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Email = user.Email,
            Page = 1,
            RecordsNumber = 10
        };

        // Act
        var response = await _groupsRepository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.AreEqual(2, response.Result!.Count());
        Assert.IsTrue(response.Result!.Any(g => g.Name == "Group 1"));
        Assert.IsTrue(response.Result!.Any(g => g.Name == "Group 2"));
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnFilteredGroups_WhenFilterIsApplied()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var tournament = new Tournament
        {
            Id = 1,
            Name = "Test Tournament"
        };

        var group1 = new Group
        {
            Id = 1,
            Code = "000001",
            Name = "Group 1",
            IsActive = true,
            Tournament = tournament,
            Admin = admin,
            Members = [new UserGroup { User = user }]
        };

        var group2 = new Group
        {
            Id = 2,
            Code = "000002",
            Name = "Another Group",
            IsActive = true,
            Tournament = tournament,
            Admin = admin,
            Members = [new UserGroup { User = user }]
        };

        _context.Users.AddRange(user, admin);
        _context.Tournaments.Add(tournament);
        _context.Groups.AddRange(group1, group2);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Email = user.Email,
            Filter = "Group 1", // Apply filter for "Group 1"
            Page = 1,
            RecordsNumber = 10
        };

        // Act
        var response = await _groupsRepository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.AreEqual(1, response.Result!.Count());
        Assert.IsTrue(response.Result!.Any(g => g.Name == "Group 1"));
        Assert.IsFalse(response.Result!.Any(g => g.Name == "Another Group"));
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnEmptyList_WhenUserHasNoGroups()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var tournament = new Tournament
        {
            Id = 1,
            Name = "Test Tournament"
        };

        var group1 = new Group
        {
            Id = 1,
            Code = "000001",
            Name = "Group 1",
            IsActive = true,
            Tournament = tournament,
            Admin = admin,
            Members = new List<UserGroup> { new() { User = admin } } // Different user
        };

        _context.Users.AddRange(user, admin);
        _context.Tournaments.Add(tournament);
        _context.Groups.Add(group1);
        await _context.SaveChangesAsync();

        var pagination = new PaginationDTO
        {
            Email = user.Email,
            Page = 1,
            RecordsNumber = 10
        };

        // Act
        var response = await _groupsRepository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.AreEqual(0, response.Result!.Count()); // User has no groups
    }

    [TestMethod]
    public async Task AddAsync_ShouldSaveImage_WhenImageIsProvided()
    {
        // Arrange
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var tournament = new Tournament
        {
            Id = 1,
            Name = "Test Tournament"
        };

        var groupDTO = new GroupDTO
        {
            AdminId = admin.Id,
            TournamentId = tournament.Id,
            Name = "Test Group",
            Remarks = "Test Remarks",
            Image = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }) // Example base64-encoded image
        };

        // Mock the GetUserAsync to return the admin user
        Mock.Get(_usersRepositoryMock)
            .Setup(repo => repo.GetUserAsync(admin.Id))
            .ReturnsAsync(admin);

        // Add tournament to the in-memory context
        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync();

        // Mock the file storage to simulate saving the image
        var savedImagePath = "saved-image-path.jpg"; // The path returned by the mock
        Mock.Get(_fileStorageMock)
            .Setup(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "groups"))
            .ReturnsAsync(savedImagePath);

        // Act
        var response = await _groupsRepository.AddAsync(groupDTO);

        // Assert
        Assert.IsTrue(response.WasSuccess);
        Assert.IsNotNull(response.Result);
        Assert.AreEqual("Test Group", response.Result.Name);
        Assert.AreEqual(savedImagePath, response.Result.Image); // Ensure the image path was saved

        // Verify that SaveFileAsync was called with the correct parameters
        Mock.Get(_fileStorageMock).Verify(f => f.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "groups"), Times.Once);
    }
}