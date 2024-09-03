using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Moq;

namespace Fantasy.Tests.UnitsOfWork;

[TestClass]
public class TournamentsUnitOfWorkTests
{
    private Mock<IGenericRepository<Tournament>> _mockGenericRepository = null!;
    private Mock<ITournamentsRepository> _mockTournamentsRepository = null!;
    private TournamentsUnitOfWork _unitOfWork = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockGenericRepository = new Mock<IGenericRepository<Tournament>>();
        _mockTournamentsRepository = new Mock<ITournamentsRepository>();
        _unitOfWork = new TournamentsUnitOfWork(_mockGenericRepository.Object, _mockTournamentsRepository.Object);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsActionResponse_WhenSuccess()
    {
        // Arrange
        var tournamentDTO = new TournamentDTO { /* Tournament properties */ };
        var response = new ActionResponse<Tournament> { WasSuccess = true };
        _mockTournamentsRepository.Setup(r => r.AddAsync(tournamentDTO))
                                  .ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.AddAsync(tournamentDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
    }

    [TestMethod]
    public async Task GetComboAsync_ReturnsTournamentList_WhenSuccess()
    {
        // Arrange
        var tournaments = new List<Tournament> { new() { Id = 1, Name = "Tournament 1" } };
        _mockTournamentsRepository.Setup(r => r.GetComboAsync())
                                  .ReturnsAsync(tournaments);

        // Act
        var result = await _unitOfWork.GetComboAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsTotalRecords_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = true, Result = 5 };
        _mockTournamentsRepository.Setup(r => r.GetTotalRecordsAsync(pagination))
                                  .ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetTotalRecordsAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(5, result.Result);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsActionResponse_WhenSuccess()
    {
        // Arrange
        var tournamentDTO = new TournamentDTO { /* Tournament properties */ };
        var response = new ActionResponse<Tournament> { WasSuccess = true };
        _mockTournamentsRepository.Setup(r => r.UpdateAsync(tournamentDTO))
                                  .ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.UpdateAsync(tournamentDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsActionResponse_WhenSuccess()
    {
        // Arrange
        var tournament = new Tournament { Id = 1, Name = "Tournament 1" };
        var response = new ActionResponse<Tournament> { WasSuccess = true, Result = tournament };
        _mockTournamentsRepository.Setup(r => r.GetAsync(1))
                                  .ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Id);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsActionResponse_WithAllTournaments()
    {
        // Arrange
        var tournaments = new List<Tournament> { new() { Id = 1, Name = "Tournament 1" } };
        var response = new ActionResponse<IEnumerable<Tournament>> { WasSuccess = true, Result = tournaments };
        _mockTournamentsRepository.Setup(r => r.GetAsync())
                                  .ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetAsync();

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsActionResponse_WithPaginatedTournaments()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var tournaments = new List<Tournament> { new() { Id = 1, Name = "Tournament 1" } };
        var response = new ActionResponse<IEnumerable<Tournament>> { WasSuccess = true, Result = tournaments };
        _mockTournamentsRepository.Setup(r => r.GetAsync(pagination))
                                  .ReturnsAsync(response);

        // Act
        var result = await _unitOfWork.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
    }
}