using Fantasy.Backend.Controllers;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Match = Fantasy.Shared.Entities.Match;

namespace Fantasy.Tests.Controllers;

[TestClass]
public class MatchesControllerTests
{
    private Mock<IMatchesUnitOfWork> _matchesUnitOfWorkMock = null!;
    private MatchesController _matchesController = null!;

    [TestInitialize]
    public void SetUp()
    {
        _matchesUnitOfWorkMock = new Mock<IMatchesUnitOfWork>();
        _matchesController = new MatchesController(null!, _matchesUnitOfWorkMock.Object);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnFilteredMatches_WhenFilterIsApplied()
    {
        // Arrange
        var pagination = new PaginationDTO
        {
            Id = 1,
            Page = 1,
            RecordsNumber = 10,
            Filter = "match"  // Applying a filter that should match on match names
        };

        var matches = new List<Match>
        {
            new() { Id = 1, TournamentId = 1, Local = new Team { Name = "Team A" }, Visitor = new Team { Name = "Team B" } },
            new() { Id = 2, TournamentId = 1, Local = new Team { Name = "Match C" }, Visitor = new Team { Name = "Team D" } }
        };

        _matchesUnitOfWorkMock.Setup(uow => uow.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<IEnumerable<Match>> { WasSuccess = true, Result = matches });

        // Act
        var result = await _matchesController.GetAsync(pagination) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);

        var returnedMatches = result.Value as IEnumerable<Match>;
        Assert.IsNotNull(returnedMatches);
        Assert.AreEqual(2, returnedMatches.Count());
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnBadRequest_WhenWasSuccessIsFalse()
    {
        // Arrange
        var pagination = new PaginationDTO
        {
            Id = 1,
            Page = 1,
            RecordsNumber = 10
        };

        // Mock the response to return WasSuccess = false to trigger the BadRequest scenario
        _matchesUnitOfWorkMock.Setup(uow => uow.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<IEnumerable<Match>> { WasSuccess = false });

        // Act
        var result = await _matchesController.GetAsync(pagination) as BadRequestResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode); // Check if BadRequest is returned
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnFilteredRecordCount_WhenFilterIsApplied()
    {
        // Arrange
        var pagination = new PaginationDTO
        {
            Id = 1,
            Page = 1,
            RecordsNumber = 10,
            Filter = "match"  // Applying a filter that should match on match names
        };

        _matchesUnitOfWorkMock.Setup(uow => uow.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = true, Result = 2 });

        // Act
        var result = await _matchesController.GetTotalRecordsAsync(pagination) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);

        var totalRecords = result.Value as int?;
        Assert.AreEqual(2, totalRecords);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnBadRequest_WhenWasSuccessIsFalse()
    {
        // Arrange
        var pagination = new PaginationDTO
        {
            Id = 1,
            Page = 1,
            RecordsNumber = 10
        };

        // Mock the response to return WasSuccess = false to trigger the BadRequest scenario
        _matchesUnitOfWorkMock.Setup(uow => uow.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = false });

        // Act
        var result = await _matchesController.GetTotalRecordsAsync(pagination) as BadRequestResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode); // Check if BadRequest is returned
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnMatch_WhenMatchExists()
    {
        // Arrange
        var match = new Match { Id = 1, Local = new Team { Name = "Team A" }, Visitor = new Team { Name = "Team B" } };

        _matchesUnitOfWorkMock.Setup(uow => uow.GetAsync(It.IsAny<int>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = true, Result = match });

        // Act
        var result = await _matchesController.GetAsync(1) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        var returnedMatch = result.Value as Match;
        Assert.IsNotNull(returnedMatch);
        Assert.AreEqual(match.Id, returnedMatch.Id);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnNotFound_WhenMatchDoesNotExist()
    {
        // Arrange
        _matchesUnitOfWorkMock.Setup(uow => uow.GetAsync(It.IsAny<int>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = false, Message = "Match not found" });

        // Act
        var result = await _matchesController.GetAsync(1) as NotFoundObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Match not found", result.Value);
    }

    [TestMethod]
    public async Task PostAsync_ShouldReturnOk_WhenMatchIsAddedSuccessfully()
    {
        // Arrange
        var matchDTO = new MatchDTO { Id = 1, LocalId = 1, VisitorId = 2 };

        _matchesUnitOfWorkMock.Setup(uow => uow.AddAsync(It.IsAny<MatchDTO>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = true, Result = new Match { Id = 1 } });

        // Act
        var result = await _matchesController.PostAsync(matchDTO) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        var returnedMatch = result.Value as Match;
        Assert.IsNotNull(returnedMatch);
        Assert.AreEqual(1, returnedMatch.Id);
    }

    [TestMethod]
    public async Task PostAsync_ShouldReturnBadRequest_WhenAddingMatchFails()
    {
        // Arrange
        var matchDTO = new MatchDTO { Id = 1, LocalId = 1, VisitorId = 2 };

        _matchesUnitOfWorkMock.Setup(uow => uow.AddAsync(It.IsAny<MatchDTO>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = false, Message = "Error adding match" });

        // Act
        var result = await _matchesController.PostAsync(matchDTO) as BadRequestObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Error adding match", result.Value);
    }

    [TestMethod]
    public async Task PutAsync_ShouldReturnOk_WhenMatchIsUpdatedSuccessfully()
    {
        // Arrange
        var matchDTO = new MatchDTO { Id = 1, LocalId = 1, VisitorId = 2 };

        _matchesUnitOfWorkMock.Setup(uow => uow.UpdateAsync(It.IsAny<MatchDTO>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = true, Result = new Match { Id = 1 } });

        // Act
        var result = await _matchesController.PutAsync(matchDTO) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        var updatedMatch = result.Value as Match;
        Assert.IsNotNull(updatedMatch);
        Assert.AreEqual(1, updatedMatch.Id);
    }

    [TestMethod]
    public async Task PutAsync_ShouldReturnBadRequest_WhenUpdatingMatchFails()
    {
        // Arrange
        var matchDTO = new MatchDTO { Id = 1, LocalId = 1, VisitorId = 2 };

        _matchesUnitOfWorkMock.Setup(uow => uow.UpdateAsync(It.IsAny<MatchDTO>()))
            .ReturnsAsync(new ActionResponse<Match> { WasSuccess = false, Message = "Error updating match" });

        // Act
        var result = await _matchesController.PutAsync(matchDTO) as BadRequestObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Error updating match", result.Value);
    }
}