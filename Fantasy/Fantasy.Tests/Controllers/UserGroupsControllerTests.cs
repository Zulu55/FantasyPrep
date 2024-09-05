using Fantasy.Backend.Controllers;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Fantasy.Tests.Controllers;

[TestClass]
public class UserGroupsControllerTests
{
    private Mock<IUserGroupsUnitOfWork> _userGroupsUnitOfWorkMock = null!;
    private Mock<IGenericUnitOfWork<UserGroup>> _genericUnitOfWorkMock = null!;
    private UserGroupsController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _userGroupsUnitOfWorkMock = new Mock<IUserGroupsUnitOfWork>();
        _genericUnitOfWorkMock = new Mock<IGenericUnitOfWork<UserGroup>>();
        _controller = new UserGroupsController(_genericUnitOfWorkMock.Object, _userGroupsUnitOfWorkMock.Object);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnOk_WhenPaginationSucceeds()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var groups = new List<UserGroup> { new() { Id = 1 }, new() { Id = 2 } };
        var response = new ActionResponse<IEnumerable<UserGroup>> { WasSuccess = true, Result = groups };

        _userGroupsUnitOfWorkMock.Setup(u => u.GetAsync(pagination)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(groups, okResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnBadRequest_WhenPaginationFails()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<IEnumerable<UserGroup>> { WasSuccess = false };

        _userGroupsUnitOfWorkMock.Setup(u => u.GetAsync(pagination)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnOk_WhenTotalRecordsSucceeds()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var totalRecords = 100;
        var response = new ActionResponse<int> { WasSuccess = true, Result = totalRecords };

        _userGroupsUnitOfWorkMock.Setup(u => u.GetTotalRecordsAsync(pagination)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetTotalRecordsAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(totalRecords, okResult.Value);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnBadRequest_WhenTotalRecordsFails()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = false };

        _userGroupsUnitOfWorkMock.Setup(u => u.GetTotalRecordsAsync(pagination)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetAsyncById_ShouldReturnOk_WhenGroupExists()
    {
        // Arrange
        var groupId = 1;
        var group = new UserGroup { Id = groupId };
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = group };

        _userGroupsUnitOfWorkMock.Setup(u => u.GetAsync(groupId)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetAsync(groupId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(group, okResult.Value);
    }

    [TestMethod]
    public async Task GetAsyncById_ShouldReturnNotFound_WhenGroupDoesNotExist()
    {
        // Arrange
        var groupId = 1;
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Group not found" };

        _userGroupsUnitOfWorkMock.Setup(u => u.GetAsync(groupId)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetAsync(groupId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual("Group not found", notFoundResult.Value);
    }

    [TestMethod]
    public async Task PostAsync_ShouldReturnOk_WhenGroupAddedSuccessfully()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = new UserGroup { Id = 1 } };

        _userGroupsUnitOfWorkMock.Setup(u => u.AddAsync(userGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _controller.PostAsync(userGroupDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(response.Result, okResult.Value);
    }

    [TestMethod]
    public async Task PostAsync_ShouldReturnBadRequest_WhenAddFails()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Add failed" };

        _userGroupsUnitOfWorkMock.Setup(u => u.AddAsync(userGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _controller.PostAsync(userGroupDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("Add failed", badRequestResult.Value);
    }

    [TestMethod]
    public async Task PutAsync_ShouldReturnOk_WhenGroupUpdatedSuccessfully()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = new UserGroup { Id = 1 } };

        _userGroupsUnitOfWorkMock.Setup(u => u.UpdateAsync(userGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _controller.PutAsync(userGroupDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(response.Result, okResult.Value);
    }

    [TestMethod]
    public async Task PutAsync_ShouldReturnNotFound_WhenUpdateFails()
    {
        // Arrange
        var userGroupDTO = new UserGroupDTO { UserId = Guid.NewGuid().ToString(), GroupId = 1 };
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Update failed" };

        _userGroupsUnitOfWorkMock.Setup(u => u.UpdateAsync(userGroupDTO)).ReturnsAsync(response);

        // Act
        var result = await _controller.PutAsync(userGroupDTO);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual("Update failed", notFoundResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnOk_WhenGroupIsFoundByEmail()
    {
        // Arrange
        int groupId = 1;
        string email = "user@example.com";
        var group = new UserGroup { Id = groupId };
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = group };

        _userGroupsUnitOfWorkMock.Setup(u => u.GetAsync(groupId, email)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetAsync(groupId, email);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(group, okResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnNotFound_WhenGroupIsNotFoundByEmail()
    {
        // Arrange
        int groupId = 1;
        string email = "user@example.com";
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Group not found" };

        _userGroupsUnitOfWorkMock.Setup(u => u.GetAsync(groupId, email)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetAsync(groupId, email);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual("Group not found", notFoundResult.Value);
    }

    [TestMethod]
    public async Task PostAsync_ShouldReturnOk_WhenJoinGroupIsSuccessful()
    {
        // Arrange
        var joinGroupDTO = new JoinGroupDTO { Code = "ABC123" };
        var response = new ActionResponse<UserGroup> { WasSuccess = true, Result = new UserGroup { Id = 1 } };

        // Mock HttpContext for User.Identity.Name
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "testUser") // Simulate logged-in user
        }));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        _userGroupsUnitOfWorkMock.Setup(u => u.JoinAsync(It.Is<JoinGroupDTO>(j => j.UserName == "testUser")))
                                 .ReturnsAsync(response);

        // Act
        var result = await _controller.PostAsync(joinGroupDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(response.Result, okResult.Value);
    }

    [TestMethod]
    public async Task PostAsync_ShouldReturnBadRequest_WhenJoinGroupFails()
    {
        // Arrange
        var joinGroupDTO = new JoinGroupDTO { Code = "ABC123" };
        var response = new ActionResponse<UserGroup> { WasSuccess = false, Message = "Join group failed" };

        // Mock HttpContext for User.Identity.Name
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
            new(ClaimTypes.Name, "testUser") // Simulate a logged-in user with a Name claim
            ]))
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Mock the JoinAsync method to simulate a failed join operation
        _userGroupsUnitOfWorkMock.Setup(u => u.JoinAsync(It.Is<JoinGroupDTO>(j => j.UserName == "testUser")))
                                 .ReturnsAsync(response);

        // Act
        var result = await _controller.PostAsync(joinGroupDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("Join group failed", badRequestResult.Value);
    }
}