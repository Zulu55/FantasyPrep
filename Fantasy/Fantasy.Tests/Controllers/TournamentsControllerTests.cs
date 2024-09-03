using Fantasy.Backend.Controllers;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Fantasy.Tests.Controllers;

[TestClass]
public class TournamentsControllerTests
{
    private Mock<ITournamentsUnitOfWork> _mockUnitOfWork = null!;
    private TournamentsController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<ITournamentsUnitOfWork>();
        _controller = new TournamentsController(null!, _mockUnitOfWork.Object);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var tournaments = new List<Tournament> { new() { Id = 1, Name = "Tournament 1" } };
        _mockUnitOfWork.Setup(u => u.GetAsync())
                       .ReturnsAsync(new ActionResponse<IEnumerable<Tournament>> { WasSuccess = true, Result = tournaments });

        // Act
        var result = await _controller.GetAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetAsync_ReturnsBadRequest_WhenFailed()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.GetAsync())
                       .ReturnsAsync(new ActionResponse<IEnumerable<Tournament>> { WasSuccess = false });

        // Act
        var result = await _controller.GetAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var tournaments = new List<Tournament> { new() { Id = 1, Name = "Tournament 1" } };
        _mockUnitOfWork.Setup(u => u.GetAsync(pagination))
                       .ReturnsAsync(new ActionResponse<IEnumerable<Tournament>> { WasSuccess = true, Result = tournaments });

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsBadRequest_WhenFailed()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        _mockUnitOfWork.Setup(u => u.GetAsync(pagination))
                       .ReturnsAsync(new ActionResponse<IEnumerable<Tournament>> { WasSuccess = false });

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        _mockUnitOfWork.Setup(u => u.GetTotalRecordsAsync(pagination))
                       .ReturnsAsync(new ActionResponse<int> { WasSuccess = true, Result = 5 });

        // Act
        var result = await _controller.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsBadRequest_WhenFailed()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        _mockUnitOfWork.Setup(u => u.GetTotalRecordsAsync(pagination))
                       .ReturnsAsync(new ActionResponse<int> { WasSuccess = false });

        // Act
        var result = await _controller.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament 1" };
        _mockUnitOfWork.Setup(u => u.GetAsync(1))
                       .ReturnsAsync(new ActionResponse<Tournament> { WasSuccess = true, Result = tournament });

        // Act
        var result = await _controller.GetAsync(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsNotFound_WhenFailed()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.GetAsync(1))
                       .ReturnsAsync(new ActionResponse<Tournament> { WasSuccess = false, Message = "Not found" });

        // Act
        var result = await _controller.GetAsync(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsOk()
    {
        // Arrange
        var comboList = new List<Tournament> { new Tournament { Id = 1, Name = "Combo 1" } };
        _mockUnitOfWork.Setup(u => u.GetComboAsync())
                       .ReturnsAsync(comboList);

        // Act
        var result = await _controller.GetComboAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task PostAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var tournamentDTO = new TournamentDTO { };
        _mockUnitOfWork.Setup(u => u.AddAsync(tournamentDTO))
                       .ReturnsAsync(new ActionResponse<Tournament> { WasSuccess = true, Result = new Tournament() });

        // Act
        var result = await _controller.PostAsync(tournamentDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task PostAsync_ReturnsBadRequest_WhenFailed()
    {
        // Arrange
        var tournamentDTO = new TournamentDTO { };
        _mockUnitOfWork.Setup(u => u.AddAsync(tournamentDTO))
                       .ReturnsAsync(new ActionResponse<Tournament> { WasSuccess = false, Message = "Error" });

        // Act
        var result = await _controller.PostAsync(tournamentDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task PutAsync_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var tournamentDTO = new TournamentDTO { };
        _mockUnitOfWork.Setup(u => u.UpdateAsync(tournamentDTO))
                       .ReturnsAsync(new ActionResponse<Tournament> { WasSuccess = true, Result = new Tournament() });

        // Act
        var result = await _controller.PutAsync(tournamentDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task PutAsync_ReturnsNotFound_WhenFailed()
    {
        // Arrange
        var tournamentDTO = new TournamentDTO { };
        _mockUnitOfWork.Setup(u => u.UpdateAsync(tournamentDTO))
                       .ReturnsAsync(new ActionResponse<Tournament> { WasSuccess = false, Message = "Not found" });

        // Act
        var result = await _controller.PutAsync(tournamentDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }
}