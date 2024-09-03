using Fantasy.Backend.Controllers;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Fantasy.Tests.Controllers;

[TestClass]
public class GenericControllerTests
{
    private Mock<IGenericUnitOfWork<SampleEntity>> _mockUnitOfWork = null!;
    private GenericController<SampleEntity> _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IGenericUnitOfWork<SampleEntity>>();
        _controller = new GenericController<SampleEntity>(_mockUnitOfWork.Object);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<IEnumerable<SampleEntity>>
        {
            WasSuccess = true,
            Result = new List<SampleEntity> { new SampleEntity { Id = 1, Name = "Entity 1" } }
        };

        _mockUnitOfWork.Setup(uow => uow.GetAsync()).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<SampleEntity>));
    }

    [TestMethod]
    public async Task GetAsync_ReturnsBadRequest_WhenNotSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<IEnumerable<SampleEntity>> { WasSuccess = false };
        _mockUnitOfWork.Setup(uow => uow.GetAsync()).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO();
        var mockResponse = new ActionResponse<IEnumerable<SampleEntity>>
        {
            WasSuccess = true,
            Result = new List<SampleEntity> { new SampleEntity { Id = 1, Name = "Entity 1" } }
        };

        _mockUnitOfWork.Setup(uow => uow.GetAsync(pagination)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<SampleEntity>));
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsBadRequest_WhenNotSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO();
        var mockResponse = new ActionResponse<IEnumerable<SampleEntity>> { WasSuccess = false };
        _mockUnitOfWork.Setup(uow => uow.GetAsync(pagination)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<int>
        {
            WasSuccess = true,
            Result = 10
        };

        _mockUnitOfWork.Setup(uow => uow.GetTotalRecordsAsync()).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetTotalRecordsAsync();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(10, okResult.Value);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsBadRequest_WhenNotSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<int> { WasSuccess = false };
        _mockUnitOfWork.Setup(uow => uow.GetTotalRecordsAsync()).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetTotalRecordsAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = true,
            Result = new SampleEntity { Id = 1, Name = "Entity 1" }
        };

        _mockUnitOfWork.Setup(uow => uow.GetAsync(1)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync(1);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(SampleEntity));
    }

    [TestMethod]
    public async Task GetAsync_WithId_ReturnsNotFound_WhenNotSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<SampleEntity> { WasSuccess = false };
        _mockUnitOfWork.Setup(uow => uow.GetAsync(1)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetAsync(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task PostAsync_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var model = new SampleEntity { Id = 1, Name = "Entity 1" };
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = true,
            Result = model
        };

        _mockUnitOfWork.Setup(uow => uow.AddAsync(model)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.PostAsync(model);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(SampleEntity));
    }

    [TestMethod]
    public async Task PostAsync_ReturnsBadRequest_WhenNotSuccess()
    {
        // Arrange
        var model = new SampleEntity { Id = 1, Name = "Entity 1" };
        var mockResponse = new ActionResponse<SampleEntity> { WasSuccess = false, Message = "Error" };

        _mockUnitOfWork.Setup(uow => uow.AddAsync(model)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.PostAsync(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("Error", badRequestResult.Value);
    }

    [TestMethod]
    public async Task PutAsync_ReturnsOkResult_WhenSuccess()
    {
        // Arrange
        var model = new SampleEntity { Id = 1, Name = "Entity 1" };
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = true,
            Result = model
        };

        _mockUnitOfWork.Setup(uow => uow.UpdateAsync(model)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.PutAsync(model);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(SampleEntity));
    }

    [TestMethod]
    public async Task PutAsync_ReturnsBadRequest_WhenNotSuccess()
    {
        // Arrange
        var model = new SampleEntity { Id = 1, Name = "Entity 1" };
        var mockResponse = new ActionResponse<SampleEntity> { WasSuccess = false, Message = "Error" };

        _mockUnitOfWork.Setup(uow => uow.UpdateAsync(model)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.PutAsync(model);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("Error", badRequestResult.Value);
    }

    [TestMethod]
    public async Task DeleteAsync_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<SampleEntity> { WasSuccess = true };

        // Configura el mock para que el tipo genérico sea `SampleEntity`
        _mockUnitOfWork.Setup(uow => uow.DeleteAsync(It.IsAny<int>())).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task DeleteAsync_ReturnsBadRequest_WhenNotSuccess()
    {
        // Arrange
        var mockResponse = new ActionResponse<SampleEntity>
        {
            WasSuccess = false,
            Message = "Error occurred while deleting the entity."
        };

        _mockUnitOfWork.Setup(uow => uow.DeleteAsync(It.IsAny<int>())).ReturnsAsync(mockResponse as ActionResponse<SampleEntity>);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("Error occurred while deleting the entity.", badRequestResult.Value);
    }
}

public class SampleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}