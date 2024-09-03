using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Responses;
using Moq;

namespace Fantasy.Tests.UnitsOfWork;

[TestClass]
public class GenericUnitOfWorkTests
{
    private Mock<IGenericRepository<SampleEntity>> _mockRepository = null!;
    private GenericUnitOfWork<SampleEntity> _unitOfWork = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IGenericRepository<SampleEntity>>();
        _unitOfWork = new GenericUnitOfWork<SampleEntity>(_mockRepository.Object);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsAddedEntity_WhenSuccess()
    {
        // Arrange
        var model = new SampleEntity { Id = 1, Name = "Test Entity" };
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = true,
            Result = model
        };

        _mockRepository.Setup(repo => repo.AddAsync(model)).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.AddAsync(model);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(model, result.Result);
    }

    [TestMethod]
    public async Task DeleteAsync_ReturnsDeletedEntity_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = true,
            Result = new SampleEntity { Id = 1, Name = "Test Entity" }
        };

        _mockRepository.Setup(repo => repo.DeleteAsync(It.IsAny<int>())).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.DeleteAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(1, result.Result.Id);
    }

    [TestMethod]
    public async Task DeleteAsync_ReturnsError_WhenNotSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = false,
            Message = "Error occurred while deleting the entity."
        };

        _mockRepository.Setup(repo => repo.DeleteAsync(It.IsAny<int>())).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.DeleteAsync(1);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Error occurred while deleting the entity.", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsEntities_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<IEnumerable<SampleEntity>>
        {
            WasSuccess = true,
            Result = [new() { Id = 1, Name = "Test Entity" }]
        };

        _mockRepository.Setup(repo => repo.GetAsync()).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.GetAsync();

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsEntity_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = true,
            Result = new SampleEntity { Id = 1, Name = "Test Entity" }
        };

        _mockRepository.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(1, result.Result.Id);
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsError_WhenNotSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = false,
            Message = "Entity not found."
        };

        _mockRepository.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.GetAsync(1);

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Entity not found.", result.Message);
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsPaginatedEntities_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO();
        var mockResponse = new ActionResponse<IEnumerable<SampleEntity>>
        {
            WasSuccess = true,
            Result = [new SampleEntity { Id = 1, Name = "Test Entity" }]
        };

        _mockRepository.Setup(repo => repo.GetAsync(pagination)).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsTotalCount_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<int>
        {
            WasSuccess = true,
            Result = 10
        };

        _mockRepository.Setup(repo => repo.GetTotalRecordsAsync()).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.GetTotalRecordsAsync();

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(10, result.Result);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsUpdatedEntity_WhenSuccess()
    {
        // Arrange
        var model = new SampleEntity { Id = 1, Name = "Updated Entity" };
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = true,
            Result = model
        };

        _mockRepository.Setup(repo => repo.UpdateAsync(model)).ReturnsAsync(mockResponse);

        // Act
        var result = await _unitOfWork.UpdateAsync(model);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(model, result.Result);
    }
}

public class SampleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}