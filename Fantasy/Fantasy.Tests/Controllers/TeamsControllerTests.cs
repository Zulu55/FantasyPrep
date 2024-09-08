using Fantasy.Backend.Controllers;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Fantasy.Tests.Controllers;

[TestClass]
public class TeamsControllerTests
{
    private Mock<ITeamsUnitOfWork> _mockTeamsUnitOfWork = null!;
    private Mock<IGenericUnitOfWork<Team>> _mockGenericUnitOfWork = null!;
    private TeamsController _teamsController = null!;

    [TestInitialize]
    public void Setup()
    {
        // Initialize mock objects and controller
        _mockTeamsUnitOfWork = new Mock<ITeamsUnitOfWork>();
        _mockGenericUnitOfWork = new Mock<IGenericUnitOfWork<Team>>();
        _teamsController = new TeamsController(_mockGenericUnitOfWork.Object, _mockTeamsUnitOfWork.Object);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange: Mock GetAsync to return a successful response
        var teams = new List<Team> { new() { Id = 1, Name = "Team A" }, new() { Id = 2, Name = "Team B" } };
        var actionResponse = new ActionResponse<IEnumerable<Team>> { WasSuccess = true, Result = teams };
        _mockTeamsUnitOfWork.Setup(u => u.GetAsync()).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method
        var result = await _teamsController.GetAsync();

        // Assert: Verify that the result is an OkObjectResult with the expected data
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(teams, okResult!.Value);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsBadRequest_WhenFailure()
    {
        // Arrange: Mock GetAsync to return a failed response
        var actionResponse = new ActionResponse<IEnumerable<Team>> { WasSuccess = false };
        _mockTeamsUnitOfWork.Setup(u => u.GetAsync()).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method
        var result = await _teamsController.GetAsync();

        // Assert: Verify that the result is a BadRequestResult
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetAsync_Paginated_ReturnsOk_WhenSuccess()
    {
        // Arrange: Mock paginated GetAsync
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var teams = new List<Team> { new Team { Id = 1, Name = "Team A" } };
        var actionResponse = new ActionResponse<IEnumerable<Team>> { WasSuccess = true, Result = teams };
        _mockTeamsUnitOfWork.Setup(u => u.GetAsync(pagination)).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method with pagination
        var result = await _teamsController.GetAsync(pagination);

        // Assert: Verify that the result is an OkObjectResult with the expected data
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(teams, okResult!.Value);
    }

    [TestMethod]
    public async Task GetAsync_Paginated_ReturnsBadRequest_WhenFailure()
    {
        // Arrange: Mock paginated GetAsync to return a failed response
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<IEnumerable<Team>> { WasSuccess = false };
        _mockTeamsUnitOfWork.Setup(u => u.GetAsync(pagination)).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method with pagination
        var result = await _teamsController.GetAsync(pagination);

        // Assert: Verify that the result is a BadRequestResult
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange: Mock GetTotalRecordsAsync
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<int> { WasSuccess = true, Result = 100 };
        _mockTeamsUnitOfWork.Setup(u => u.GetTotalRecordsAsync(pagination)).ReturnsAsync(actionResponse);

        // Act: Call the GetTotalRecordsAsync method
        var result = await _teamsController.GetTotalRecordsAsync(pagination);

        // Assert: Verify that the result is an OkObjectResult with the total records
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(100, okResult!.Value);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsBadRequest_WhenFailure()
    {
        // Arrange: Mock GetTotalRecordsAsync to return a failed response
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<int> { WasSuccess = false };
        _mockTeamsUnitOfWork.Setup(u => u.GetTotalRecordsAsync(pagination)).ReturnsAsync(actionResponse);

        // Act: Call the GetTotalRecordsAsync method
        var result = await _teamsController.GetTotalRecordsAsync(pagination);

        // Assert: Verify that the result is a BadRequestResult
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsOk_WhenSuccess()
    {
        // Arrange: Mock GetAsync by ID
        var team = new Team { Id = 1, Name = "Team A" };
        var actionResponse = new ActionResponse<Team> { WasSuccess = true, Result = team };
        _mockTeamsUnitOfWork.Setup(u => u.GetAsync(1)).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method by ID
        var result = await _teamsController.GetAsync(1);

        // Assert: Verify that the result is an OkObjectResult with the expected data
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(team, okResult!.Value);
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsNotFound_WhenFailure()
    {
        // Arrange: Mock GetAsync by ID to return a failed response
        var actionResponse = new ActionResponse<Team> { WasSuccess = false, Message = "Team not found" };
        _mockTeamsUnitOfWork.Setup(u => u.GetAsync(1)).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method by ID
        var result = await _teamsController.GetAsync(1);

        // Assert: Verify that the result is a NotFoundObjectResult
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        var notFoundResult = result as NotFoundObjectResult;
        Assert.AreEqual("Team not found", notFoundResult!.Value);
    }

    [TestMethod]
    public async Task PostAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange: Mock AddAsync
        var teamDTO = new TeamDTO { Name = "Team A", CountryId = 1 };
        var team = new Team { Id = 1, Name = "Team A" };
        var actionResponse = new ActionResponse<Team> { WasSuccess = true, Result = team };
        _mockTeamsUnitOfWork.Setup(u => u.AddAsync(teamDTO)).ReturnsAsync(actionResponse);

        // Act: Call the PostAsync method
        var result = await _teamsController.PostAsync(teamDTO);

        // Assert: Verify that the result is an OkObjectResult with the expected data
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(team, okResult!.Value);
    }

    [TestMethod]
    public async Task PostAsync_ReturnsBadRequest_WhenFailure()
    {
        // Arrange: Mock AddAsync to return a failed response
        var teamDTO = new TeamDTO { Name = "Team A", CountryId = 1 };
        var actionResponse = new ActionResponse<Team> { WasSuccess = false, Message = "Error adding team" };
        _mockTeamsUnitOfWork.Setup(u => u.AddAsync(teamDTO)).ReturnsAsync(actionResponse);

        // Act: Call the PostAsync method
        var result = await _teamsController.PostAsync(teamDTO);

        // Assert: Verify that the result is a BadRequestObjectResult
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        Assert.AreEqual("Error adding team", badRequestResult!.Value);
    }

    [TestMethod]
    public async Task PutAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange: Mock UpdateAsync
        var teamDTO = new TeamDTO { Id = 1, Name = "Team A", CountryId = 1 };
        var team = new Team { Id = 1, Name = "Team A" };
        var actionResponse = new ActionResponse<Team> { WasSuccess = true, Result = team };
        _mockTeamsUnitOfWork.Setup(u => u.UpdateAsync(teamDTO)).ReturnsAsync(actionResponse);

        // Act: Call the PutAsync method
        var result = await _teamsController.PutAsync(teamDTO);

        // Assert: Verify that the result is an OkObjectResult with the expected data
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(team, okResult!.Value);
    }

    [TestMethod]
    public async Task PutAsync_ReturnsBadRequest_WhenFailure()
    {
        // Arrange: Mock UpdateAsync to return a failed response
        var teamDTO = new TeamDTO { Id = 1, Name = "Team A", CountryId = 1 };
        var actionResponse = new ActionResponse<Team> { WasSuccess = false, Message = "Error updating team" };
        _mockTeamsUnitOfWork.Setup(u => u.UpdateAsync(teamDTO)).ReturnsAsync(actionResponse);

        // Act: Call the PutAsync method
        var result = await _teamsController.PutAsync(teamDTO);

        // Assert: Verify that the result is a BadRequestObjectResult
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        Assert.AreEqual("Error updating team", badRequestResult!.Value);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange: Mock GetComboAsync to return a list of teams
        var comboData = new List<Team>
    {
        new Team { Id = 1, Name = "Team A" },
        new Team { Id = 2, Name = "Team B" }
    };

        _mockTeamsUnitOfWork.Setup(u => u.GetComboAsync(It.IsAny<int>()))
            .ReturnsAsync(comboData);

        // Act: Call the GetComboAsync method
        var result = await _teamsController.GetComboAsync(1);

        // Assert: Verify that the result is an OkObjectResult with the expected combo data
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(comboData, okResult!.Value);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsEmptyOk_WhenNoData()
    {
        // Arrange: Mock GetComboAsync to return an empty list of teams
        var comboData = new List<Team>(); // Empty list

        _mockTeamsUnitOfWork.Setup(u => u.GetComboAsync(It.IsAny<int>()))
            .ReturnsAsync(comboData);

        // Act: Call the GetComboAsync method
        var result = await _teamsController.GetComboAsync(1);

        // Assert: Verify that the result is an OkObjectResult with an empty list
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.AreEqual(comboData, okResult!.Value); // Should be empty
    }
}