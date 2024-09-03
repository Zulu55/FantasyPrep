using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Moq;

namespace Fantasy.Tests.UnitsOfWork;

[TestClass]
public class TournamentTeamsUnitOfWorkTests
{
    private Mock<ITournamentTeamsRepository> _mockRepository = null!;
    private TournamentTeamsUnitOfWork _unitOfWork = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<ITournamentTeamsRepository>();
        _unitOfWork = new TournamentTeamsUnitOfWork(null!, _mockRepository.Object);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsActionResponse_WithTournamentTeam_WhenSuccess()
    {
        // Arrange
        var tournamentTeamDTO = new TournamentTeamDTO { Id = 1, TournamentId = 1, TeamId = 1 };
        var actionResponse = new ActionResponse<TournamentTeam>
        {
            WasSuccess = true,
            Result = new TournamentTeam { Id = 1, TournamentId = 1, TeamId = 1 }
        };
        _mockRepository.Setup(r => r.AddAsync(tournamentTeamDTO))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _unitOfWork.AddAsync(tournamentTeamDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(1, result.Result.Id);
        Assert.AreEqual(1, result.Result.TournamentId);
        Assert.AreEqual(1, result.Result.TeamId);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsTournamentTeams()
    {
        // Arrange
        var tournamentId = 1;
        var mockTeams = new List<TournamentTeam>
        {
            new TournamentTeam { Id = 1, TournamentId = tournamentId },
            new TournamentTeam { Id = 2, TournamentId = tournamentId }
        };
        _mockRepository.Setup(r => r.GetComboAsync(tournamentId))
                       .ReturnsAsync(mockTeams);

        // Act
        var result = await _unitOfWork.GetComboAsync(tournamentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual(tournamentId, result.First().TournamentId);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsTotalRecordCount_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<int>
        {
            WasSuccess = true,
            Result = 5
        };
        _mockRepository.Setup(r => r.GetTotalRecordsAsync(pagination))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _unitOfWork.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(5, result.Result);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsPaginatedTournamentTeams_WhenSuccess()
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
        _mockRepository.Setup(r => r.GetAsync(pagination))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _unitOfWork.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(2, result.Result.Count());
    }

    [TestMethod]
    public async Task AddAsync_ReturnsActionResponse_WithError_WhenFailure()
    {
        // Arrange
        var tournamentTeamDTO = new TournamentTeamDTO { Id = 1, TournamentId = 1, TeamId = 1 };
        var actionResponse = new ActionResponse<TournamentTeam>
        {
            WasSuccess = false,
            Message = "Error occurred"
        };
        _mockRepository.Setup(r => r.AddAsync(tournamentTeamDTO))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _unitOfWork.AddAsync(tournamentTeamDTO);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Error occurred", result.Message);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsError_WhenFailure()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<int>
        {
            WasSuccess = false,
            Message = "Error occurred"
        };
        _mockRepository.Setup(r => r.GetTotalRecordsAsync(pagination))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _unitOfWork.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Error occurred", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsError_WhenFailure()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var actionResponse = new ActionResponse<IEnumerable<TournamentTeam>>
        {
            WasSuccess = false,
            Message = "Error occurred"
        };
        _mockRepository.Setup(r => r.GetAsync(pagination))
                       .ReturnsAsync(actionResponse);

        // Act
        var result = await _unitOfWork.GetAsync(pagination);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Error occurred", result.Message);
    }
}