using Fantasy.Backend.Data;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
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
}