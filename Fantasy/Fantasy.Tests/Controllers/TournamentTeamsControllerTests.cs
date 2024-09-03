using Fantasy.Backend.Controllers;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Fantasy.Tests.Controllers;

[TestClass]
public class TournamentTeamsControllerTests
{
    private Mock<ITournamentTeamsUnitOfWork> _mockUnitOfWork = null!;
    private TournamentTeamsController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<ITournamentTeamsUnitOfWork>();
        _controller = new TournamentTeamsController(null!, _mockUnitOfWork.Object);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsOkResult_WithTournamentTeams()
    {
        // Arrange
        var tournamentId = 1;
        var mockTeams = new List<TournamentTeam>
        {
            new() { Id = 1, TournamentId = tournamentId },
            new() { Id = 2, TournamentId = tournamentId }
        };
        _mockUnitOfWork.Setup(u => u.GetComboAsync(tournamentId))
                       .ReturnsAsync(mockTeams);

        // Act
        var result = await _controller.GetComboAsync(tournamentId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var teams = okResult.Value as IEnumerable<TournamentTeam>;
        Assert.IsNotNull(teams);
        Assert.AreEqual(2, teams.Count());
    }

    [TestMethod]
    public async Task PostAsync_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var tournamentTeamDTO = new TournamentTeamDTO { Id = 1, TournamentId = 1, TeamId = 1 };
        var actionResponse = new ActionResponse<TournamentTeam>
        {
            WasSuccess = true,
            Result = new TournamentTeam { Id = 1, TournamentId = 1, TeamId = 1 }
        };
        _mockUnitOfWork.Setup(u => u.AddAsync(tournamentTeamDTO))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _controller.PostAsync(tournamentTeamDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var team = okResult.Value as TournamentTeam;
        Assert.IsNotNull(team);
        Assert.AreEqual(1, team.Id);
    }

    [TestMethod]
    public async Task PostAsync_ReturnsBadRequest_WhenFailure()
    {
        // Arrange
        var tournamentTeamDTO = new TournamentTeamDTO { Id = 1, TournamentId = 1, TeamId = 1 };
        var actionResponse = new ActionResponse<TournamentTeam>
        {
            WasSuccess = false,
            Message = "Error occurred"
        };
        _mockUnitOfWork.Setup(u => u.AddAsync(tournamentTeamDTO))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _controller.PostAsync(tournamentTeamDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("Error occurred", badRequestResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsOkResult_WithPaginatedTournamentTeams()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var mockTeams = new List<TournamentTeam>
        {
            new TournamentTeam { Id = 1, TournamentId = 1 },
            new TournamentTeam { Id = 2, TournamentId = 1 }
        };
        var actionResponse = new ActionResponse<IEnumerable<TournamentTeam>>
        {
            WasSuccess = true,
            Result = mockTeams
        };
        _mockUnitOfWork.Setup(u => u.GetAsync(pagination))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var teams = okResult.Value as IEnumerable<TournamentTeam>;
        Assert.IsNotNull(teams);
        Assert.AreEqual(2, teams.Count());
    }

    [TestMethod]
    public async Task GetAsync_ReturnsBadRequest_WhenFailure()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<IEnumerable<TournamentTeam>>
        {
            WasSuccess = false
        };
        _mockUnitOfWork.Setup(u => u.GetAsync(pagination))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsOkResult_WithTotalRecords()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<int>
        {
            WasSuccess = true,
            Result = 5
        };
        _mockUnitOfWork.Setup(u => u.GetTotalRecordsAsync(pagination))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _controller.GetTotalRecordsAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(5, okResult.Value);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsBadRequest_WhenFailure()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<int>
        {
            WasSuccess = false
        };
        _mockUnitOfWork.Setup(u => u.GetTotalRecordsAsync(pagination))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _controller.GetTotalRecordsAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
    }
}