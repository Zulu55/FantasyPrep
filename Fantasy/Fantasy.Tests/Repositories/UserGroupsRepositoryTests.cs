using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Tests.General;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Fantasy.Tests.Repositories;

[TestClass]
public class UserGroupsRepositoryTests
{
    private DataContext _context = null!;
    private UserGroupsRepository _userGroupsRepository = null!;
    private Mock<IUsersRepository> _usersRepositoryMock = null!;

    [TestInitialize]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new DataContext(options);
        _usersRepositoryMock = new Mock<IUsersRepository>();
        _userGroupsRepository = new UserGroupsRepository(_context, _usersRepositoryMock.Object);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context.Database.EnsureDeleted();
    }

    [TestMethod]
    public async Task JoinAsync_ShouldReturnError_WhenGroupNotFound()
    {
        // Arrange
        var joinGroupDTO = new JoinGroupDTO { Code = "ABC123", UserName = "testUser" };

        // Act
        var result = await _userGroupsRepository.JoinAsync(joinGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR017", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnFilteredUserGroups_WhenFilterIsApplied()
    {
        // Arrange
        var pagination = new PaginationDTO
        {
            Id = 1,
            Page = 1,
            RecordsNumber = 10,
            Filter = "john"
        };

        var userGroups = new List<UserGroup>
        {
            new() {
                Id = 1,
                GroupId = 1,
                User = new User { FirstName = "John", LastName = "Doe" }
            },
            new() {
                Id = 2,
                GroupId = 1,
                User = new User { FirstName = "Jane", LastName = "Smith" }
            },
            new() {
                Id = 3,
                GroupId = 1,
                User = new User { FirstName = "Johnny", LastName = "Appleseed" }
            }
        };

        // Add the user groups to the in-memory database
        _context.UserGroups.AddRange(userGroups);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userGroupsRepository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        var resultList = result.Result!.ToList();

        // Only "John Doe" and "Johnny Appleseed" should match the filter "john"
        Assert.AreEqual(2, resultList.Count);
        Assert.IsTrue(resultList.Any(ug => ug.User.FirstName == "John" && ug.User.LastName == "Doe"));
        Assert.IsTrue(resultList.Any(ug => ug.User.FirstName == "Johnny" && ug.User.LastName == "Appleseed"));
        Assert.IsFalse(resultList.Any(ug => ug.User.FirstName == "Jane" && ug.User.LastName == "Smith"));
    }

    [TestMethod]
    public async Task JoinAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        var joinGroupDTO = new JoinGroupDTO { Code = "ABC123", UserName = "testUser" };

        // Creating a valid Group with required properties
        var group = new Group
        {
            Id = 1,
            Code = "ABC123",
            Name = "Test Group",
            AdminId = Guid.NewGuid().ToString(),
            IsActive = true
        };

        // Add the group to the in-memory context
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Mock the user repository to return null (user not found)
        _usersRepositoryMock.Setup(u => u.GetUserAsync(joinGroupDTO.UserName)).ReturnsAsync((User)null!);

        // Act
        var result = await _userGroupsRepository.JoinAsync(joinGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR013", result.Message);
    }

    [TestMethod]
    public async Task JoinAsync_ShouldAddUserGroup_WhenSuccessful()
    {
        // Arrange
        var joinGroupDTO = new JoinGroupDTO { Code = "ABC123", UserName = "testUser" };

        // Creating a valid Group with required properties
        var group = new Group
        {
            Id = 1,
            Code = "ABC123",
            Name = "Test Group",
            AdminId = Guid.NewGuid().ToString(),
            IsActive = true
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testUser",
            FirstName = "John",
            LastName = "Doe"
        };

        // Add the group to the in-memory context
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Mock the user repository to return the user
        _usersRepositoryMock.Setup(u => u.GetUserAsync(joinGroupDTO.UserName)).ReturnsAsync(user);

        // Act
        var result = await _userGroupsRepository.JoinAsync(joinGroupDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(user, result.Result.User);
        Assert.AreEqual(group, result.Result.Group);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddUserGroup_WhenSuccessful()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };

        // Creating a valid Group with required properties
        var group = new Group
        {
            Id = 1,
            Name = "Group 1",
            AdminId = Guid.NewGuid().ToString(),
            Code = "ABC123",
            IsActive = true
        };

        var user = new User { Id = userGroupDTO.UserId, FirstName = "John", LastName = "Doe" };

        // Add the group to the in-memory context
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Mock the user repository to return the user
        _usersRepositoryMock.Setup(u => u.GetUserAsync(Guid.Parse(userGroupDTO.UserId))).ReturnsAsync(user);

        // Act
        var result = await _userGroupsRepository.AddAsync(userGroupDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(group, result.Result!.Group);
        Assert.AreEqual(user, result.Result.User);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };

        _usersRepositoryMock.Setup(u => u.GetUserAsync(Guid.Parse(userGroupDTO.UserId))).ReturnsAsync((User)null!);

        // Act
        var result = await _userGroupsRepository.AddAsync(userGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR013", result.Message);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenGroupNotFound()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };
        var user = new User { Id = userGroupDTO.UserId, FirstName = "John", LastName = "Doe" };

        _usersRepositoryMock.Setup(u => u.GetUserAsync(Guid.Parse(userGroupDTO.UserId))).ReturnsAsync(user);

        // Act
        var result = await _userGroupsRepository.AddAsync(userGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR014", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnPaginatedUserGroups_WhenSuccessful()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var userGroups = new List<UserGroup>
        {
            new UserGroup { Id = 1, User = new User { FirstName = "John", LastName = "Doe" }, GroupId = 1 },
            new UserGroup { Id = 2, User = new User { FirstName = "Jane", LastName = "Smith" }, GroupId = 1 }
        };

        _context.UserGroups.AddRange(userGroups);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userGroupsRepository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetAsyncById_ShouldReturnUserGroup_WhenSuccessful()
    {
        // Arrange
        var userGroup = new UserGroup
        {
            Id = 1,
            User = new User
            {
                FirstName = "John",
                LastName = "Doe"
            }
        };

        // Add the userGroup to the in-memory database
        _context.UserGroups.Add(userGroup);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userGroupsRepository.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(userGroup, result.Result);
    }

    [TestMethod]
    public async Task GetAsyncById_ShouldReturnError_WhenUserGroupNotFound()
    {
        // Act
        var result = await _userGroupsRepository.GetAsync(1);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR001", result.Message);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnTotalRecordCount()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var userGroups = new List<UserGroup>
        {
            new() { Id = 1, User = new User { FirstName = "John", LastName = "Doe" }, GroupId = 1 },
            new() { Id = 2, User = new User { FirstName = "Jane", LastName = "Smith" }, GroupId = 1 }
        };

        _context.UserGroups.AddRange(userGroups);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userGroupsRepository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnFilteredRecordCount_WhenFilterIsApplied()
    {
        // Arrange
        var pagination = new PaginationDTO
        {
            Id = 1,
            Filter = "john"
        };

        var userGroups = new List<UserGroup>
        {
            new() {
                Id = 1,
                GroupId = 1,
                User = new User { FirstName = "John", LastName = "Doe" }
            },
            new() {
                Id = 2,
                GroupId = 1,
                User = new User { FirstName = "Jane", LastName = "Smith" }
            },
            new() {
                Id = 3,
                GroupId = 1,
                User = new User { FirstName = "Johnny", LastName = "Appleseed" }
            }
        };

        // Add the user groups to the in-memory database
        _context.UserGroups.AddRange(userGroups);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userGroupsRepository.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateUserGroup_WhenSuccessful()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };
        var userGroupDTO = new UserGroupDTO { Id = 1, IsActive = false };
        var userGroup = new UserGroup
        {
            Id = 1,
            IsActive = true,
            UserId = user.Id,
            User = user
        };

        // Add the user and userGroup to the in-memory database
        _context.Users.Add(user);
        _context.UserGroups.Add(userGroup);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userGroupsRepository.UpdateAsync(userGroupDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsFalse(userGroup.IsActive);  // The userGroup should now be inactive
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenUserGroupNotFound()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { Id = 1 };

        // Act
        var result = await _userGroupsRepository.UpdateAsync(userGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR015", result.Message);
    }

    [TestMethod]
    public async Task GetAsyncByGroupIdAndEmail_ShouldReturnUserGroup_WhenSuccessful()
    {
        // Arrange
        var userGroup = new UserGroup
        {
            Id = 1,
            GroupId = 1,
            User = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            }
        };

        // Add the userGroup to the in-memory database
        _context.UserGroups.Add(userGroup);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userGroupsRepository.GetAsync(1, "test@example.com");

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(userGroup, result.Result);
    }

    [TestMethod]
    public async Task GetAsyncByGroupIdAndEmail_ShouldReturnError_WhenUserGroupNotFound()
    {
        // Act
        var result = await _userGroupsRepository.GetAsync(1, "test@example.com");

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR001", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenDbUpdateExceptionOccurs_ForUserGroup()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Create a UserGroup entity with required fields (UserId is required)
        var userGroup = new UserGroup
        {
            Id = 1,
            UserId = Guid.NewGuid().ToString(), // Ensure UserId is set
            IsActive = true
        };

        context.UserGroups.Add(userGroup);
        await context.SaveChangesAsync();

        // Use FakeDbContext to simulate DbUpdateException
        var fakeContext = new FakeDbContext(options);
        var repository = new UserGroupsRepository(fakeContext, _usersRepositoryMock.Object);
        var userGroupDTO = new UserGroupDTO
        {
            Id = 1,
            IsActive = false  // Update some value
        };

        // Act
        var result = await repository.UpdateAsync(userGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message);  // Ensure the correct error message for DbUpdateException
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenGeneralExceptionOccurs_ForUserGroup()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Create and add entities directly to the context
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };
        var group = new Group { Id = 1, Name = "Group A", AdminId = Guid.NewGuid().ToString(), Code = "GRP123" };
        var userGroup = new UserGroup { Id = 1, User = user, Group = group, IsActive = true };

        context.Users.Add(user);
        context.Groups.Add(group);
        context.UserGroups.Add(userGroup);
        await context.SaveChangesAsync();

        // Use the FakeDbContextWithGeneralException to simulate an exception
        var fakeContext = new FakeDbContextWithGeneralException(options);
        var repository = new UserGroupsRepository(fakeContext, _usersRepositoryMock.Object);
        var userGroupDTO = new UserGroupDTO
        {
            Id = 1,
            IsActive = false
        };

        // Act
        var result = await repository.UpdateAsync(userGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenDbUpdateExceptionOccurs_ForUserGroup()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Mocking the IUsersRepository
        var mockUsersRepository = new Mock<IUsersRepository>();

        // Create related entities
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };
        var group = new Group { Id = 1, Name = "Group A", AdminId = Guid.NewGuid().ToString(), Code = "GRP123" };

        // Mock GetUserAsync to return a valid user
        mockUsersRepository.Setup(repo => repo.GetUserAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        // Add group to the context
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Use FakeDbContext to simulate DbUpdateException
        var fakeContext = new FakeDbContext(options);
        var repository = new UserGroupsRepository(fakeContext, mockUsersRepository.Object);

        var userGroupDTO = new UserGroupDTO
        {
            UserId = user.Id,
            GroupId = group.Id
        };

        // Act
        var result = await repository.AddAsync(userGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message);  // Verify that DbUpdateException is caught and handled
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenGeneralExceptionOccurs_ForUserGroup()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Mocking the IUsersRepository
        var mockUsersRepository = new Mock<IUsersRepository>();

        // Create related entities
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };
        var group = new Group { Id = 1, Name = "Group A", AdminId = Guid.NewGuid().ToString(), Code = "GRP123" };

        // Mock GetUserAsync to return a valid user
        mockUsersRepository.Setup(repo => repo.GetUserAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        // Add group to the context
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Use FakeDbContextWithGeneralException to simulate a general exception
        var fakeContext = new FakeDbContextWithGeneralException(options);
        var repository = new UserGroupsRepository(fakeContext, mockUsersRepository.Object);

        var userGroupDTO = new UserGroupDTO
        {
            UserId = user.Id,
            GroupId = group.Id
        };

        // Act
        var result = await repository.AddAsync(userGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message);  // Verify that a general exception is caught and handled
    }

    [TestMethod]
    public async Task JoinAsync_ReturnsError_WhenDbUpdateExceptionOccurs_ForUserGroup()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Mocking the IUsersRepository
        var mockUsersRepository = new Mock<IUsersRepository>();

        // Create related entities
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };
        var group = new Group { Id = 1, Name = "Group A", AdminId = Guid.NewGuid().ToString(), Code = "GRP123" };

        // Mock GetUserAsync to return a valid user
        mockUsersRepository.Setup(repo => repo.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Add group to the context
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Use FakeDbContext to simulate DbUpdateException
        var fakeContext = new FakeDbContext(options);
        var repository = new UserGroupsRepository(fakeContext, mockUsersRepository.Object);

        var joinGroupDTO = new JoinGroupDTO
        {
            UserName = user.Id,
            Code = group.Code
        };

        // Act
        var result = await repository.JoinAsync(joinGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("ERR003", result.Message);  // Verify that DbUpdateException is caught and handled
    }

    [TestMethod]
    public async Task JoinAsync_ReturnsError_WhenGeneralExceptionOccurs_ForUserGroup()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new DataContext(options);

        // Mocking the IUsersRepository
        var mockUsersRepository = new Mock<IUsersRepository>();

        // Create related entities
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };
        var group = new Group { Id = 1, Name = "Group A", AdminId = Guid.NewGuid().ToString(), Code = "GRP123" };

        // Mock GetUserAsync to return a valid user
        mockUsersRepository.Setup(repo => repo.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Add group to the context
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Use FakeDbContextWithGeneralException to simulate a general exception
        var fakeContext = new FakeDbContextWithGeneralException(options);
        var repository = new UserGroupsRepository(fakeContext, mockUsersRepository.Object);

        var joinGroupDTO = new JoinGroupDTO
        {
            UserName = user.Id,
            Code = group.Code
        };

        // Act
        var result = await repository.JoinAsync(joinGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("General exception occurred", result.Message);  // Verify that a general exception is caught and handled
    }
}