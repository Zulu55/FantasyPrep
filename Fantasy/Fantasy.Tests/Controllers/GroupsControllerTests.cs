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
public class GroupsControllerTests
{
    private Mock<IGroupsUnitOfWork> _mockGroupsUnitOfWork = null!;
    private GroupsController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockGroupsUnitOfWork = new Mock<IGroupsUnitOfWork>();
        _controller = new GroupsController(Mock.Of<IGenericUnitOfWork<Group>>(), _mockGroupsUnitOfWork.Object);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsOk_WhenWasSuccessIsTrue()
    {
        // Arrange
        var groups = new List<Group> { new Group { Id = 1, Name = "Test Group" } };
        _mockGroupsUnitOfWork.Setup(u => u.GetAllAsync())
            .ReturnsAsync(new ActionResponse<IEnumerable<Group>> { WasSuccess = true, Result = groups });

        // Act
        var result = await _controller.GetAllAsync();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsBadRequest_WhenWasSuccessIsFalse()
    {
        // Arrange
        _mockGroupsUnitOfWork.Setup(u => u.GetAllAsync())
            .ReturnsAsync(new ActionResponse<IEnumerable<Group>> { WasSuccess = false });

        // Act
        var result = await _controller.GetAllAsync();

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsOk_WhenWasSuccessIsTrue()
    {
        // Arrange
        var group = new Group { Id = 1, Name = "Test Group" };
        _mockGroupsUnitOfWork.Setup(u => u.GetAsync(1))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = true, Result = group });

        // Act
        var result = await _controller.GetAsync(1);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(group, okResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsNotFound_WhenWasSuccessIsFalse()
    {
        // Arrange
        _mockGroupsUnitOfWork.Setup(u => u.GetAsync(1))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = false, Message = "Group not found" });

        // Act
        var result = await _controller.GetAsync(1);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("Group not found", notFoundResult.Value);
    }

    [TestMethod]
    public async Task PostAsync_ReturnsOk_WhenWasSuccessIsTrue()
    {
        // Arrange
        var groupDTO = new GroupDTO { Name = "New Group" };

        // Mocking the User.Identity.Name property
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "testuser")
        ], "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockGroupsUnitOfWork.Setup(u => u.AddAsync(It.IsAny<GroupDTO>()))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = true, Result = new Group { Id = 1, Name = "New Group" } });

        // Act
        var result = await _controller.PostAsync(groupDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public async Task PostAsync_ReturnsBadRequest_WhenWasSuccessIsFalse()
    {
        // Arrange
        var groupDTO = new GroupDTO { Name = "New Group" };

        // Mocking the User.Identity.Name property
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
    new Claim(ClaimTypes.Name, "testuser")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockGroupsUnitOfWork.Setup(u => u.AddAsync(It.IsAny<GroupDTO>()))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = false, Message = "Error occurred" });

        // Act
        var result = await _controller.PostAsync(groupDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Error occurred", badRequestResult.Value);
    }

    [TestMethod]
    public async Task PutAsync_ReturnsOk_WhenWasSuccessIsTrue()
    {
        // Arrange
        var groupDTO = new GroupDTO { Id = 1, Name = "Updated Group" };
        _mockGroupsUnitOfWork.Setup(u => u.UpdateAsync(It.IsAny<GroupDTO>()))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = true, Result = new Group { Id = 1, Name = "Updated Group" } });

        // Act
        var result = await _controller.PutAsync(groupDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public async Task PutAsync_ReturnsBadRequest_WhenWasSuccessIsFalse()
    {
        // Arrange
        var groupDTO = new GroupDTO { Id = 1, Name = "Updated Group" };
        _mockGroupsUnitOfWork.Setup(u => u.UpdateAsync(It.IsAny<GroupDTO>()))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = false, Message = "Error occurred" });

        // Act
        var result = await _controller.PutAsync(groupDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Error occurred", badRequestResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_WithCode_ReturnsOk_WhenWasSuccessIsTrue()
    {
        // Arrange
        var groupCode = "test-code";
        var group = new Group { Id = 1, Name = "Test Group", Code = groupCode };

        _mockGroupsUnitOfWork.Setup(u => u.GetAsync(groupCode))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = true, Result = group });

        // Act
        var result = await _controller.GetAsync(groupCode);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(group, okResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_WithCode_ReturnsNotFound_WhenWasSuccessIsFalse()
    {
        // Arrange
        var groupCode = "test-code";

        _mockGroupsUnitOfWork.Setup(u => u.GetAsync(groupCode))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = false, Message = "Group not found" });

        // Act
        var result = await _controller.GetAsync(groupCode);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("Group not found", notFoundResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsOk_WhenWasSuccessIsTrue()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Mocking the User.Identity.Name property
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "testuser")
        ], "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var groups = new List<Group> { new() { Id = 1, Name = "Test Group" } };

        _mockGroupsUnitOfWork.Setup(u => u.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<IEnumerable<Group>> { WasSuccess = true, Result = groups });

        // Act
        var result = await _controller.GetAsync(paginationDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(groups, okResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsBadRequest_WhenWasSuccessIsFalse()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Mocking the User.Identity.Name property
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "testuser")
        ], "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockGroupsUnitOfWork.Setup(u => u.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<IEnumerable<Group>> { WasSuccess = false });

        // Act
        var result = await _controller.GetAsync(paginationDTO);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsOk_WhenWasSuccessIsTrue()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Mocking the User.Identity.Name property
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "testuser")
        ], "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockGroupsUnitOfWork.Setup(u => u.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = true, Result = 100 });

        // Act
        var result = await _controller.GetTotalRecordsAsync(paginationDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(100, okResult.Value);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsBadRequest_WhenWasSuccessIsFalse()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Mocking the User.Identity.Name property
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "testuser")
        ], "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _mockGroupsUnitOfWork.Setup(u => u.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = false });

        // Act
        var result = await _controller.GetTotalRecordsAsync(paginationDTO);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task CheckPredictionsForAllMatchesAsync_ReturnsOk_WhenCalled()
    {
        // Arrange
        int groupId = 1;

        _mockGroupsUnitOfWork.Setup(u => u.CheckPredictionsForAllMatchesAsync(groupId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CheckPredictionsForAllMatchesAsync(groupId);

        // Assert
        var okResult = result as OkResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        _mockGroupsUnitOfWork.Verify(u => u.CheckPredictionsForAllMatchesAsync(groupId), Times.Once);
    }
}