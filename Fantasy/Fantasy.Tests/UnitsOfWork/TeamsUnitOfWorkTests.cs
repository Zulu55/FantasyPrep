using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Moq;

namespace Fantasy.Tests.UnitsOfWork;

[TestClass]
public class TeamsUnitOfWorkTests
{
    private Mock<ITeamsRepository> _mockTeamsRepository = null!;
    private Mock<IGenericRepository<Team>> _mockGenericRepository = null!;
    private TeamsUnitOfWork _teamsUnitOfWork = null!;

    [TestInitialize]
    public void Setup()
    {
        // Initialize mocks and the unit of work
        _mockTeamsRepository = new Mock<ITeamsRepository>();
        _mockGenericRepository = new Mock<IGenericRepository<Team>>();
        _teamsUnitOfWork = new TeamsUnitOfWork(_mockGenericRepository.Object, _mockTeamsRepository.Object);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsActionResponse_WhenSuccess()
    {
        // Arrange: Mock AddAsync
        var teamDTO = new TeamDTO { Name = "Team A", CountryId = 1 };
        var team = new Team { Id = 1, Name = "Team A" };
        var actionResponse = new ActionResponse<Team> { WasSuccess = true, Result = team };
        _mockTeamsRepository.Setup(r => r.AddAsync(teamDTO)).ReturnsAsync(actionResponse);

        // Act: Call the AddAsync method
        var result = await _teamsUnitOfWork.AddAsync(teamDTO);

        // Assert: Verify the action response is returned
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(team, result.Result);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsError_WhenFailure()
    {
        // Arrange: Mock AddAsync to return an error response
        var teamDTO = new TeamDTO { Name = "Team A", CountryId = 1 };
        var actionResponse = new ActionResponse<Team> { WasSuccess = false, Message = "Error adding team" };
        _mockTeamsRepository.Setup(r => r.AddAsync(It.IsAny<TeamDTO>())).ReturnsAsync(actionResponse);

        // Act: Call the AddAsync method
        var result = await _teamsUnitOfWork.AddAsync(teamDTO);

        // Assert: Verify the error response
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Error adding team", result.Message);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsTeams_WhenSuccess()
    {
        // Arrange: Mock GetComboAsync
        var comboData = new List<Team> { new Team { Id = 1, Name = "Team A" }, new Team { Id = 2, Name = "Team B" } };
        _mockTeamsRepository.Setup(r => r.GetComboAsync(It.IsAny<int>())).ReturnsAsync(comboData);

        // Act: Call the GetComboAsync method
        var result = await _teamsUnitOfWork.GetComboAsync(1);

        // Assert: Verify the result is a list of teams
        Assert.AreEqual(comboData, result);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsActionResponse_WhenSuccess()
    {
        // Arrange: Mock UpdateAsync
        var teamDTO = new TeamDTO { Id = 1, Name = "Updated Team A", CountryId = 1 };
        var team = new Team { Id = 1, Name = "Updated Team A" };
        var actionResponse = new ActionResponse<Team> { WasSuccess = true, Result = team };
        _mockTeamsRepository.Setup(r => r.UpdateAsync(teamDTO)).ReturnsAsync(actionResponse);

        // Act: Call the UpdateAsync method
        var result = await _teamsUnitOfWork.UpdateAsync(teamDTO);

        // Assert: Verify the action response is returned
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(team, result.Result);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsError_WhenFailure()
    {
        // Arrange: Mock UpdateAsync to return an error response
        var teamDTO = new TeamDTO { Id = 1, Name = "Updated Team A", CountryId = 1 };
        var actionResponse = new ActionResponse<Team> { WasSuccess = false, Message = "Error updating team" };
        _mockTeamsRepository.Setup(r => r.UpdateAsync(It.IsAny<TeamDTO>())).ReturnsAsync(actionResponse);

        // Act: Call the UpdateAsync method
        var result = await _teamsUnitOfWork.UpdateAsync(teamDTO);

        // Assert: Verify the error response
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Error updating team", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsActionResponse_WhenSuccess()
    {
        // Arrange: Mock GetAsync by ID
        var team = new Team { Id = 1, Name = "Team A" };
        var actionResponse = new ActionResponse<Team> { WasSuccess = true, Result = team };
        _mockTeamsRepository.Setup(r => r.GetAsync(1)).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method
        var result = await _teamsUnitOfWork.GetAsync(1);

        // Assert: Verify the action response
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(team, result.Result);
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsError_WhenFailure()
    {
        // Arrange: Mock GetAsync by ID to return an error response
        var actionResponse = new ActionResponse<Team> { WasSuccess = false, Message = "Team not found" };
        _mockTeamsRepository.Setup(r => r.GetAsync(1)).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method
        var result = await _teamsUnitOfWork.GetAsync(1);

        // Assert: Verify the error response
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Team not found", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsActionResponse_WhenSuccess()
    {
        // Arrange: Mock GetAsync to return a list of teams
        var teams = new List<Team> { new Team { Id = 1, Name = "Team A" }, new Team { Id = 2, Name = "Team B" } };
        var actionResponse = new ActionResponse<IEnumerable<Team>> { WasSuccess = true, Result = teams };
        _mockTeamsRepository.Setup(r => r.GetAsync()).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method
        var result = await _teamsUnitOfWork.GetAsync();

        // Assert: Verify the action response
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(teams, result.Result);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsPaginatedTeams_WhenSuccess()
    {
        // Arrange: Mock GetAsync with pagination
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var teams = new List<Team> { new Team { Id = 1, Name = "Team A" }, new Team { Id = 2, Name = "Team B" } };
        var actionResponse = new ActionResponse<IEnumerable<Team>> { WasSuccess = true, Result = teams };
        _mockTeamsRepository.Setup(r => r.GetAsync(pagination)).ReturnsAsync(actionResponse);

        // Act: Call the GetAsync method with pagination
        var result = await _teamsUnitOfWork.GetAsync(pagination);

        // Assert: Verify the action response
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(teams, result.Result);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsActionResponse_WhenSuccess()
    {
        // Arrange: Mock GetTotalRecordsAsync
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<int> { WasSuccess = true, Result = 100 };
        _mockTeamsRepository.Setup(r => r.GetTotalRecordsAsync(pagination)).ReturnsAsync(actionResponse);

        // Act: Call the GetTotalRecordsAsync method
        var result = await _teamsUnitOfWork.GetTotalRecordsAsync(pagination);

        // Assert: Verify the action response
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(100, result.Result);
    }
}