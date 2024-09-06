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
public class PredictionsControllerTests
{
    private Mock<IPredictionsUnitOfWork> _predictionsUnitOfWorkMock = null!;
    private PredictionsController _predictionsController = null!;
    private Mock<ClaimsPrincipal> _mockUser = null!;

    [TestInitialize]
    public void SetUp()
    {
        // Initialize the mock for IPredictionsUnitOfWork
        _predictionsUnitOfWorkMock = new Mock<IPredictionsUnitOfWork>();

        // Mock the User Identity
        _mockUser = new Mock<ClaimsPrincipal>();
        _mockUser.Setup(u => u.Identity!.Name).Returns("testuser@example.com");
        _mockUser.Setup(u => u.Identity!.IsAuthenticated).Returns(true); // Make sure user is authenticated

        // Create the controller with the mocked unit of work
        _predictionsController = new PredictionsController(
            new Mock<IGenericUnitOfWork<Prediction>>().Object,
            _predictionsUnitOfWorkMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _mockUser.Object }
            }
        };
    }

    [TestMethod]
    public async Task GetBalanceAsync_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = true, Result = new List<Prediction>() };

        _predictionsUnitOfWorkMock.Setup(u => u.GetBalanceAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetBalanceAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<Prediction>));
    }

    [TestMethod]
    public async Task GetBalanceAsync_ShouldReturnBadRequest_WhenFailed()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = false };

        _predictionsUnitOfWorkMock.Setup(u => u.GetBalanceAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetBalanceAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task GetTotalRecordsBalanceAsync_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = true, Result = 10 };

        _predictionsUnitOfWorkMock.Setup(u => u.GetTotalRecordsBalanceAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetTotalRecordsBalanceAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(10, okResult.Value);
    }

    [TestMethod]
    public async Task GetTotalRecordsBalanceAsync_ShouldReturnBadRequest_WhenFailed()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = false };

        _predictionsUnitOfWorkMock.Setup(u => u.GetTotalRecordsBalanceAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetTotalRecordsBalanceAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task GetAllPredictionsAsync_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = true, Result = new List<Prediction>() };

        _predictionsUnitOfWorkMock.Setup(u => u.GetAllPredictionsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetAllPredictionsAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<Prediction>));
    }

    [TestMethod]
    public async Task GetAllPredictionsAsync_ShouldReturnBadRequest_WhenFailed()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = false };

        _predictionsUnitOfWorkMock.Setup(u => u.GetAllPredictionsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetAllPredictionsAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task GetTotalRecordsAllPredictionsAsync_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = true, Result = 50 };

        _predictionsUnitOfWorkMock.Setup(u => u.GetTotalRecordsAllPredictionsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetTotalRecordsAllPredictionsAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(50, okResult.Value);
    }

    [TestMethod]
    public async Task GetTotalRecordsAllPredictionsAsync_ShouldReturnBadRequest_WhenFailed()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = false };

        _predictionsUnitOfWorkMock.Setup(u => u.GetTotalRecordsAllPredictionsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetTotalRecordsAllPredictionsAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task GetTotalRecordsForPositionsAsync_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = true, Result = 30 };

        _predictionsUnitOfWorkMock.Setup(u => u.GetTotalRecordsForPositionsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetTotalRecordsForPositionsAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(30, okResult.Value);
    }

    [TestMethod]
    public async Task GetTotalRecordsForPositionsAsync_ShouldReturnBadRequest_WhenFailed()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = false };

        _predictionsUnitOfWorkMock.Setup(u => u.GetTotalRecordsForPositionsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetTotalRecordsForPositionsAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task GetAsync_ById_ShouldReturnOk_WhenPredictionIsFound()
    {
        // Arrange
        var prediction = new Prediction { Id = 1 };
        var response = new ActionResponse<Prediction> { WasSuccess = true, Result = prediction };

        _predictionsUnitOfWorkMock.Setup(u => u.GetAsync(It.IsAny<int>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetAsync(1);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(prediction, okResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_ById_ShouldReturnNotFound_WhenPredictionIsNotFound()
    {
        // Arrange
        var response = new ActionResponse<Prediction> { WasSuccess = false, Message = "Not Found" };

        _predictionsUnitOfWorkMock.Setup(u => u.GetAsync(It.IsAny<int>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetAsync(1);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("Not Found", notFoundResult.Value);
    }

    [TestMethod]
    public async Task PutAsync_ShouldReturnOk_WhenUpdateIsSuccessful()
    {
        // Arrange
        var predictionDTO = new PredictionDTO { Id = 1 };
        var response = new ActionResponse<Prediction> { WasSuccess = true, Result = new Prediction() };

        _predictionsUnitOfWorkMock.Setup(u => u.UpdateAsync(It.IsAny<PredictionDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.PutAsync(predictionDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.IsInstanceOfType(okResult.Value, typeof(Prediction));
    }

    [TestMethod]
    public async Task PutAsync_ShouldReturnBadRequest_WhenUpdateFails()
    {
        // Arrange
        var predictionDTO = new PredictionDTO { Id = 1 };
        var response = new ActionResponse<Prediction> { WasSuccess = false, Message = "Error" };

        _predictionsUnitOfWorkMock.Setup(u => u.UpdateAsync(It.IsAny<PredictionDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.PutAsync(predictionDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Error", badRequestResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnOk_WhenResponseIsSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var mockPredictions = new List<Prediction> { new Prediction { Id = 1 }, new Prediction { Id = 2 } };
        var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = true, Result = mockPredictions };

        _predictionsUnitOfWorkMock.Setup(u => u.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);  // Ensure that the result is OkObjectResult
        Assert.AreEqual(200, okResult.StatusCode);  // Ensure that the status code is 200 (OK)
        Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<Prediction>));  // Ensure the value is of the correct type
        Assert.AreEqual(mockPredictions, okResult.Value);  // Ensure that the returned value matches the mocked result
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnBadRequest_WhenResponseIsFailure()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = false };

        _predictionsUnitOfWorkMock.Setup(u => u.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);  // Ensure that the result is BadRequestResult
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure that the status code is 400 (Bad Request)
    }

    [TestMethod]
    public async Task GetAsync_ShouldSetPaginationEmailToUserIdentityName()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = true, Result = new List<Prediction>() };

        _predictionsUnitOfWorkMock.Setup(u => u.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        await _predictionsController.GetAsync(pagination);

        // Assert
        _predictionsUnitOfWorkMock.Verify(u => u.GetAsync(It.Is<PaginationDTO>(p => p.Email == "testuser@example.com")), Times.Once);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnOk_WhenResponseIsSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = true, Result = 100 };

        _predictionsUnitOfWorkMock.Setup(u => u.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetTotalRecordsAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);  // Ensure that the result is OkObjectResult
        Assert.AreEqual(200, okResult.StatusCode);  // Ensure that the status code is 200 (OK)
        Assert.AreEqual(100, okResult.Value);  // Ensure the returned value matches the mocked result
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldReturnBadRequest_WhenResponseIsFailure()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = false };

        _predictionsUnitOfWorkMock.Setup(u => u.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetTotalRecordsAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);  // Ensure that the result is BadRequestResult
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure that the status code is 400 (Bad Request)
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ShouldSetPaginationEmailToUserIdentityName()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<int> { WasSuccess = true, Result = 100 };

        _predictionsUnitOfWorkMock.Setup(u => u.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        await _predictionsController.GetTotalRecordsAsync(pagination);

        // Assert
        _predictionsUnitOfWorkMock.Verify(u => u.GetTotalRecordsAsync(It.Is<PaginationDTO>(p => p.Email == "testuser@example.com")), Times.Once);
    }

    [TestMethod]
    public async Task PostAsync_ShouldReturnOk_WhenPredictionIsAddedSuccessfully()
    {
        // Arrange
        var predictionDTO = new PredictionDTO { Id = 1 };
        var mockPrediction = new Prediction { Id = 1 };
        var response = new ActionResponse<Prediction> { WasSuccess = true, Result = mockPrediction };

        _predictionsUnitOfWorkMock.Setup(u => u.AddAsync(It.IsAny<PredictionDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.PostAsync(predictionDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);  // Ensure that the result is OkObjectResult
        Assert.AreEqual(200, okResult.StatusCode);  // Ensure that the status code is 200 (OK)
        Assert.IsInstanceOfType(okResult.Value, typeof(Prediction));  // Ensure the value is of the correct type
        Assert.AreEqual(mockPrediction, okResult.Value);  // Ensure that the returned value matches the mocked result
    }

    [TestMethod]
    public async Task PostAsync_ShouldReturnBadRequest_WhenPredictionAdditionFails()
    {
        // Arrange
        var predictionDTO = new PredictionDTO { Id = 1 };
        var response = new ActionResponse<Prediction> { WasSuccess = false, Message = "Error adding prediction" };

        _predictionsUnitOfWorkMock.Setup(u => u.AddAsync(It.IsAny<PredictionDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.PostAsync(predictionDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);  // Ensure that the result is BadRequestObjectResult
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure that the status code is 400 (Bad Request)
        Assert.AreEqual("Error adding prediction", badRequestResult.Value);  // Ensure the message matches the expected error message
    }

    [TestMethod]
    public async Task PostAsync_ShouldCallAddAsyncWithCorrectPredictionDTO()
    {
        // Arrange
        var predictionDTO = new PredictionDTO { Id = 1 };
        var response = new ActionResponse<Prediction> { WasSuccess = true, Result = new Prediction { Id = 1 } };

        _predictionsUnitOfWorkMock.Setup(u => u.AddAsync(It.IsAny<PredictionDTO>()))
            .ReturnsAsync(response);

        // Act
        await _predictionsController.PostAsync(predictionDTO);

        // Assert
        _predictionsUnitOfWorkMock.Verify(u => u.AddAsync(It.Is<PredictionDTO>(p => p.Id == 1)), Times.Once);
    }

    [TestMethod]
    public async Task GetPositionsAsync_ShouldReturnOk_WhenResponseIsSuccess()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var mockPositions = new List<PositionDTO> { new() { User = new User(), Points = 20 }, new PositionDTO { User = new User(), Points = 10 } }; // Mocked list of positions
        var response = new ActionResponse<IEnumerable<PositionDTO>> { WasSuccess = true, Result = mockPositions };

        _predictionsUnitOfWorkMock.Setup(u => u.GetPositionsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetPositionsAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);  // Ensure that the result is OkObjectResult
        Assert.AreEqual(200, okResult.StatusCode);  // Ensure that the status code is 200 (OK)
        Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<PositionDTO>));  // Ensure the value is of the correct type
        Assert.AreEqual(mockPositions, okResult.Value);  // Ensure that the returned value matches the mocked result
    }

    [TestMethod]
    public async Task GetPositionsAsync_ShouldReturnBadRequest_WhenResponseIsFailure()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<IEnumerable<PositionDTO>> { WasSuccess = false };

        _predictionsUnitOfWorkMock.Setup(u => u.GetPositionsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        var result = await _predictionsController.GetPositionsAsync(pagination);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);  // Ensure that the result is BadRequestResult
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure that the status code is 400 (Bad Request)
    }

    [TestMethod]
    public async Task GetPositionsAsync_ShouldCallGetPositionsAsyncWithCorrectPaginationDTO()
    {
        // Arrange
        var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
        var response = new ActionResponse<IEnumerable<PositionDTO>> { WasSuccess = true, Result = new List<PositionDTO>() };

        _predictionsUnitOfWorkMock.Setup(u => u.GetPositionsAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(response);

        // Act
        await _predictionsController.GetPositionsAsync(pagination);

        // Assert
        _predictionsUnitOfWorkMock.Verify(u => u.GetPositionsAsync(It.Is<PaginationDTO>(p => p.Id == 1)), Times.Once);
    }
}