using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Moq;

namespace Fantasy.Tests.UnitsOfWork
{
    [TestClass]
    public class PredictionsUnitOfWorkTests
    {
        private Mock<IPredictionsRepository> _predictionsRepositoryMock = null!;
        private PredictionsUnitOfWork _predictionsUnitOfWork = null!;

        [TestInitialize]
        public void SetUp()
        {
            // Initialize the mock for IPredictionsRepository
            _predictionsRepositoryMock = new Mock<IPredictionsRepository>();

            // Initialize the unit of work with the mocked repository
            _predictionsUnitOfWork = new PredictionsUnitOfWork(
                new Mock<IGenericRepository<Prediction>>().Object,
                _predictionsRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetAsync_ByPagination_ShouldReturnCorrectResponse()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
            var mockPredictions = new List<Prediction> { new() { Id = 1 }, new() { Id = 2 } };
            var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = true, Result = mockPredictions };

            _predictionsRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.GetAsync(pagination);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(mockPredictions, result.Result);
        }

        [TestMethod]
        public async Task GetAsync_ById_ShouldReturnCorrectResponse()
        {
            // Arrange
            var mockPrediction = new Prediction { Id = 1 };
            var response = new ActionResponse<Prediction> { WasSuccess = true, Result = mockPrediction };

            _predictionsRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.GetAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(mockPrediction, result.Result);
        }

        [TestMethod]
        public async Task AddAsync_ShouldReturnCorrectResponse()
        {
            // Arrange
            var predictionDTO = new PredictionDTO { Id = 1 };
            var mockPrediction = new Prediction { Id = 1 };
            var response = new ActionResponse<Prediction> { WasSuccess = true, Result = mockPrediction };

            _predictionsRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<PredictionDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.AddAsync(predictionDTO);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(mockPrediction, result.Result);
        }

        [TestMethod]
        public async Task GetTotalRecordsAsync_ShouldReturnCorrectResponse()
        {
            // Arrange
            var paginationDTO = new PaginationDTO { Id = 1 };
            var response = new ActionResponse<int> { WasSuccess = true, Result = 100 };

            _predictionsRepositoryMock.Setup(repo => repo.GetTotalRecordsAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.GetTotalRecordsAsync(paginationDTO);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(100, result.Result);
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldReturnCorrectResponse()
        {
            // Arrange
            var predictionDTO = new PredictionDTO { Id = 1 };
            var mockPrediction = new Prediction { Id = 1 };
            var response = new ActionResponse<Prediction> { WasSuccess = true, Result = mockPrediction };

            _predictionsRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<PredictionDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.UpdateAsync(predictionDTO);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(mockPrediction, result.Result);
        }

        [TestMethod]
        public async Task GetPositionsAsync_ShouldReturnCorrectResponse()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
            var mockPositions = new List<PositionDTO> { new() { User = new User(), Points = 20 }, new() { User = new User(), Points = 10 } };
            var response = new ActionResponse<IEnumerable<PositionDTO>> { WasSuccess = true, Result = mockPositions };

            _predictionsRepositoryMock.Setup(repo => repo.GetPositionsAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.GetPositionsAsync(pagination);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(mockPositions, result.Result);
        }

        [TestMethod]
        public async Task GetTotalRecordsForPositionsAsync_ShouldReturnCorrectResponse()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1 };
            var response = new ActionResponse<int> { WasSuccess = true, Result = 50 };

            _predictionsRepositoryMock.Setup(repo => repo.GetTotalRecordsForPositionsAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.GetTotalRecordsForPositionsAsync(pagination);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(50, result.Result);
        }

        [TestMethod]
        public async Task GetAllPredictionsAsync_ShouldReturnCorrectResponse()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
            var mockPredictions = new List<Prediction> { new Prediction { Id = 1 }, new Prediction { Id = 2 } };
            var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = true, Result = mockPredictions };

            _predictionsRepositoryMock.Setup(repo => repo.GetAllPredictionsAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.GetAllPredictionsAsync(pagination);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(mockPredictions, result.Result);
        }

        [TestMethod]
        public async Task GetTotalRecordsAllPredictionsAsync_ShouldReturnCorrectResponse()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1 };
            var response = new ActionResponse<int> { WasSuccess = true, Result = 80 };

            _predictionsRepositoryMock.Setup(repo => repo.GetTotalRecordsAllPredictionsAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.GetTotalRecordsAllPredictionsAsync(pagination);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(80, result.Result);
        }

        [TestMethod]
        public async Task GetBalanceAsync_ShouldReturnCorrectResponse()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1, Page = 1, RecordsNumber = 10 };
            var mockPredictions = new List<Prediction> { new Prediction { Id = 1 }, new Prediction { Id = 2 } };
            var response = new ActionResponse<IEnumerable<Prediction>> { WasSuccess = true, Result = mockPredictions };

            _predictionsRepositoryMock.Setup(repo => repo.GetBalanceAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.GetBalanceAsync(pagination);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(mockPredictions, result.Result);
        }

        [TestMethod]
        public async Task GetTotalRecordsBalanceAsync_ShouldReturnCorrectResponse()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1 };
            var response = new ActionResponse<int> { WasSuccess = true, Result = 60 };

            _predictionsRepositoryMock.Setup(repo => repo.GetTotalRecordsBalanceAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _predictionsUnitOfWork.GetTotalRecordsBalanceAsync(pagination);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(60, result.Result);
        }
    }
}