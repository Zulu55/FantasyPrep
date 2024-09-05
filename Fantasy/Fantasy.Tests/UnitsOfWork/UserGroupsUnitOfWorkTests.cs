using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Moq;

namespace Fantasy.Tests.UnitsOfWork;

[TestClass]
public class UserGroupsUnitOfWorkTests
{
    private Mock<IUserGroupsRepository> _userGroupsRepositoryMock = null!;
    private Mock<IGenericRepository<UserGroup>> _genericRepositoryMock = null!;
    private UserGroupsUnitOfWork _unitOfWork = null!;

    [TestInitialize]
    public void SetUp()
    {
        _userGroupsRepositoryMock = new Mock<IUserGroupsRepository>();
        _genericRepositoryMock = new Mock<IGenericRepository<UserGroup>>();
        _unitOfWork = new UserGroupsUnitOfWork(_genericRepositoryMock.Object, _userGroupsRepositoryMock.Object);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnGroups_WhenPaginationIsSuccessful()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var userGroups = new List<UserGroup> { new() { Id = 1 }, new UserGroup { Id = 2 } };
        var response = new ActionResponse<IEnumerable<UserGroup>> { WasSuccess = true, Result = userGroups };

        _userGroupsRepositoryMock.Setup(repo => repo.GetAsync(pagination)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(userGroups, result.Result);
    }

    [TestMethod]
    public async Task GetAsyncById_ShouldReturnGroup_WhenIdExists()
    {
        // Arrange
        var userGroup = new UserGroup { Id = 1 };
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = userGroup };

        _userGroupsRepositoryMock.Setup(repo => repo.GetAsync(1)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(userGroup, result.Result);
    }

    [TestMethod]
    public async Task GetAsyncById_ShouldReturnError_WhenIdDoesNotExist()
    {
        // Arrange
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Group not found" };

        _userGroupsRepositoryMock.Setup(repo => repo.GetAsync(1)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetAsync(1);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Group not found", result.Message);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddGroup_WhenSuccessful()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = new UserGroup { Id = 1 } };

        _userGroupsRepositoryMock.Setup(repo => repo.AddAsync(userGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.AddAsync(userGroupDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(response.Result, result.Result);
    }

    [TestMethod]
    public async Task AddAsync_ShouldReturnError_WhenAddFails()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Add failed" };

        _userGroupsRepositoryMock.Setup(repo => repo.AddAsync(userGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.AddAsync(userGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Add failed", result.Message);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnTotalRecords_WhenSuccessful()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = true, Result = 100 };

        _userGroupsRepositoryMock.Setup(repo => repo.GetTotalRecordsAsync(pagination)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(100, result.Result);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateGroup_WhenSuccessful()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = new UserGroup { Id = 1 } };

        _userGroupsRepositoryMock.Setup(repo => repo.UpdateAsync(userGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.UpdateAsync(userGroupDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(response.Result, result.Result);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnError_WhenUpdateFails()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Update failed" };

        _userGroupsRepositoryMock.Setup(repo => repo.UpdateAsync(userGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.UpdateAsync(userGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Update failed", result.Message);
    }

    [TestMethod]
    public async Task JoinAsync_ShouldJoinGroup_WhenSuccessful()
    {
        // Arrange
        var joinGroupDTO = new JoinGroupDTO { Code = "ABC123", UserName = "testUser" };
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = new UserGroup { Id = 1 } };

        _userGroupsRepositoryMock.Setup(repo => repo.JoinAsync(joinGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.JoinAsync(joinGroupDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(response.Result, result.Result);
    }

    [TestMethod]
    public async Task JoinAsync_ShouldReturnError_WhenJoinFails()
    {
        // Arrange
        var joinGroupDTO = new JoinGroupDTO { Code = "ABC123", UserName = "testUser" };
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Join failed" };

        _userGroupsRepositoryMock.Setup(repo => repo.JoinAsync(joinGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.JoinAsync(joinGroupDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Join failed", result.Message);
    }

    [TestMethod]
    public async Task GetAsyncByGroupIdAndEmail_ShouldReturnGroup_WhenSuccessful()
    {
        // Arrange
        int groupId = 1;
        string email = "test@example.com";
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = new UserGroup { Id = 1 } };

        _userGroupsRepositoryMock.Setup(repo => repo.GetAsync(groupId, email)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetAsync(groupId, email);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(response.Result, result.Result);
    }

    [TestMethod]
    public async Task GetAsyncByGroupIdAndEmail_ShouldReturnError_WhenGroupNotFound()
    {
        // Arrange
        int groupId = 1;
        string email = "test@example.com";
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Group not found" };

        _userGroupsRepositoryMock.Setup(repo => repo.GetAsync(groupId, email)).ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetAsync(groupId, email);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Group not found", result.Message);
    }
}