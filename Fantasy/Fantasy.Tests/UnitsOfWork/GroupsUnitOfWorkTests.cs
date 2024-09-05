using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Moq;

namespace Fantasy.Tests.UnitsOfWork;

[TestClass]
public class GroupsUnitOfWorkTests
{
    private Mock<IGroupsRepository> _mockGroupsRepository = null!;
    private GroupsUnitOfWork _unitOfWork = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockGroupsRepository = new Mock<IGroupsRepository>();
        _unitOfWork = new GroupsUnitOfWork(Mock.Of<IGenericRepository<Group>>(), _mockGroupsRepository.Object);
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsGroups()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var groups = new List<Group> { new Group { Id = 1, Name = "Test Group" } };

        _mockGroupsRepository.Setup(r => r.GetAsync(paginationDTO))
            .ReturnsAsync(new ActionResponse<IEnumerable<Group>> { WasSuccess = true, Result = groups });

        // Act
        var result = await _unitOfWork.GetAsync(paginationDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(groups, result.Result);
        _mockGroupsRepository.Verify(r => r.GetAsync(paginationDTO), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ById_ReturnsGroup()
    {
        // Arrange
        var group = new Group { Id = 1, Name = "Test Group" };

        _mockGroupsRepository.Setup(r => r.GetAsync(1))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = true, Result = group });

        // Act
        var result = await _unitOfWork.GetAsync(1);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(group, result.Result);
        _mockGroupsRepository.Verify(r => r.GetAsync(1), Times.Once);
    }

    [TestMethod]
    public async Task AddAsync_ReturnsAddedGroup()
    {
        // Arrange
        var groupDTO = new GroupDTO { Name = "New Group" };
        var group = new Group { Id = 1, Name = "New Group" };

        _mockGroupsRepository.Setup(r => r.AddAsync(groupDTO))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = true, Result = group });

        // Act
        var result = await _unitOfWork.AddAsync(groupDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(group, result.Result);
        _mockGroupsRepository.Verify(r => r.AddAsync(groupDTO), Times.Once);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsTotalRecords()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        _mockGroupsRepository.Setup(r => r.GetTotalRecordsAsync(paginationDTO))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = true, Result = 100 });

        // Act
        var result = await _unitOfWork.GetTotalRecordsAsync(paginationDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(100, result.Result);
        _mockGroupsRepository.Verify(r => r.GetTotalRecordsAsync(paginationDTO), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsUpdatedGroup()
    {
        // Arrange
        var groupDTO = new GroupDTO { Id = 1, Name = "Updated Group" };
        var group = new Group { Id = 1, Name = "Updated Group" };

        _mockGroupsRepository.Setup(r => r.UpdateAsync(groupDTO))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = true, Result = group });

        // Act
        var result = await _unitOfWork.UpdateAsync(groupDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(group, result.Result);
        _mockGroupsRepository.Verify(r => r.UpdateAsync(groupDTO), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ByCode_ReturnsGroup()
    {
        // Arrange
        var groupCode = "test-code";
        var group = new Group { Id = 1, Name = "Test Group", Code = groupCode };

        _mockGroupsRepository.Setup(r => r.GetAsync(groupCode))
            .ReturnsAsync(new ActionResponse<Group> { WasSuccess = true, Result = group });

        // Act
        var result = await _unitOfWork.GetAsync(groupCode);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(group, result.Result);
        _mockGroupsRepository.Verify(r => r.GetAsync(groupCode), Times.Once);
    }

    [TestMethod]
    public async Task CheckPredictionsForAllMatchesAsync_CallsRepositoryMethod()
    {
        // Arrange
        int groupId = 1;

        _mockGroupsRepository.Setup(r => r.CheckPredictionsForAllMatchesAsync(groupId))
            .Returns(Task.CompletedTask);

        // Act
        await _unitOfWork.CheckPredictionsForAllMatchesAsync(groupId);

        // Assert
        _mockGroupsRepository.Verify(r => r.CheckPredictionsForAllMatchesAsync(groupId), Times.Once);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsAllGroups()
    {
        // Arrange
        var groups = new List<Group> { new Group { Id = 1, Name = "Test Group" } };

        _mockGroupsRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new ActionResponse<IEnumerable<Group>> { WasSuccess = true, Result = groups });

        // Act
        var result = await _unitOfWork.GetAllAsync();

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(groups, result.Result);
        _mockGroupsRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }
}