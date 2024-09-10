using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Fantasy.Tests.UnitsOfWork;

[TestClass]
public class UsersUnitOfWorkTests
{
    private Mock<IUsersRepository> _mockUsersRepository = null!;
    private UsersUnitOfWork _usersUnitOfWork = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockUsersRepository = new Mock<IUsersRepository>();
        _usersUnitOfWork = new UsersUnitOfWork(_mockUsersRepository.Object);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsUserList_WhenPaginationIsProvided()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var users = new List<User> { new User { Email = "test@example.com" } };

        _mockUsersRepository.Setup(x => x.GetAsync(paginationDTO))
            .ReturnsAsync(new ActionResponse<IEnumerable<User>> { WasSuccess = true, Result = users });

        // Act
        var result = await _usersUnitOfWork.GetAsync(paginationDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(users, result.Result);
        _mockUsersRepository.Verify(x => x.GetAsync(paginationDTO), Times.Once);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsTotalRecords()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        _mockUsersRepository.Setup(x => x.GetTotalRecordsAsync(paginationDTO))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = true, Result = 100 });

        // Act
        var result = await _usersUnitOfWork.GetTotalRecordsAsync(paginationDTO);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(100, result.Result);
        _mockUsersRepository.Verify(x => x.GetTotalRecordsAsync(paginationDTO), Times.Once);
    }

    [TestMethod]
    public async Task GeneratePasswordResetTokenAsync_ReturnsToken()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        _mockUsersRepository.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset_token");

        // Act
        var token = await _usersUnitOfWork.GeneratePasswordResetTokenAsync(user);

        // Assert
        Assert.AreEqual("reset_token", token);
        _mockUsersRepository.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
    }

    [TestMethod]
    public async Task ResetPasswordAsync_ReturnsIdentityResult()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        _mockUsersRepository.Setup(x => x.ResetPasswordAsync(user, "token", "new_password"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersUnitOfWork.ResetPasswordAsync(user, "token", "new_password");

        // Assert
        Assert.AreEqual(IdentityResult.Success, result);
        _mockUsersRepository.Verify(x => x.ResetPasswordAsync(user, "token", "new_password"), Times.Once);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsIdentityResult()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        _mockUsersRepository.Setup(x => x.ChangePasswordAsync(user, "old_password", "new_password"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersUnitOfWork.ChangePasswordAsync(user, "old_password", "new_password");

        // Assert
        Assert.AreEqual(IdentityResult.Success, result);
        _mockUsersRepository.Verify(x => x.ChangePasswordAsync(user, "old_password", "new_password"), Times.Once);
    }

    [TestMethod]
    public async Task UpdateUserAsync_ReturnsIdentityResult()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        _mockUsersRepository.Setup(x => x.UpdateUserAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersUnitOfWork.UpdateUserAsync(user);

        // Assert
        Assert.AreEqual(IdentityResult.Success, result);
        _mockUsersRepository.Verify(x => x.UpdateUserAsync(user), Times.Once);
    }

    [TestMethod]
    public async Task GetUserAsync_ById_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId.ToString(), Email = "test@example.com" };
        _mockUsersRepository.Setup(x => x.GetUserAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _usersUnitOfWork.GetUserAsync(userId);

        // Assert
        Assert.AreEqual(user, result);
        _mockUsersRepository.Verify(x => x.GetUserAsync(userId), Times.Once);
    }

    [TestMethod]
    public async Task GenerateEmailConfirmationTokenAsync_ReturnsToken()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        _mockUsersRepository.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation_token");

        // Act
        var token = await _usersUnitOfWork.GenerateEmailConfirmationTokenAsync(user);

        // Assert
        Assert.AreEqual("confirmation_token", token);
        _mockUsersRepository.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once);
    }

    [TestMethod]
    public async Task ConfirmEmailAsync_ReturnsIdentityResult()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        _mockUsersRepository.Setup(x => x.ConfirmEmailAsync(user, "token"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersUnitOfWork.ConfirmEmailAsync(user, "token");

        // Assert
        Assert.AreEqual(IdentityResult.Success, result);
        _mockUsersRepository.Verify(x => x.ConfirmEmailAsync(user, "token"), Times.Once);
    }

    [TestMethod]
    public async Task AddUserAsync_ReturnsIdentityResult()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        _mockUsersRepository.Setup(x => x.AddUserAsync(user, "password"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersUnitOfWork.AddUserAsync(user, "password");

        // Assert
        Assert.AreEqual(IdentityResult.Success, result);
        _mockUsersRepository.Verify(x => x.AddUserAsync(user, "password"), Times.Once);
    }

    [TestMethod]
    public async Task AddUserToRoleAsync_SuccessfullyAddsUserToRole()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };

        // Act
        await _usersUnitOfWork.AddUserToRoleAsync(user, "Admin");

        // Assert
        _mockUsersRepository.Verify(x => x.AddUserToRoleAsync(user, "Admin"), Times.Once);
    }

    [TestMethod]
    public async Task CheckRoleAsync_VerifiesRoleCheck()
    {
        // Arrange

        // Act
        await _usersUnitOfWork.CheckRoleAsync("Admin");

        // Assert
        _mockUsersRepository.Verify(x => x.CheckRoleAsync("Admin"), Times.Once);
    }

    [TestMethod]
    public async Task GetUserAsync_ByEmail_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User { Email = email };
        _mockUsersRepository.Setup(x => x.GetUserAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _usersUnitOfWork.GetUserAsync(email);

        // Assert
        Assert.AreEqual(user, result);
        _mockUsersRepository.Verify(x => x.GetUserAsync(email), Times.Once);
    }

    [TestMethod]
    public async Task IsUserInRoleAsync_ReturnsTrue_WhenUserIsInRole()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        _mockUsersRepository.Setup(x => x.IsUserInRoleAsync(user, "Admin"))
            .ReturnsAsync(true);

        // Act
        var result = await _usersUnitOfWork.IsUserInRoleAsync(user, "Admin");

        // Assert
        Assert.IsTrue(result);
        _mockUsersRepository.Verify(x => x.IsUserInRoleAsync(user, "Admin"), Times.Once);
    }

    [TestMethod]
    public async Task LoginAsync_ReturnsSignInResult()
    {
        // Arrange
        var loginDTO = new LoginDTO { Email = "test@example.com", Password = "password" };
        _mockUsersRepository.Setup(x => x.LoginAsync(loginDTO))
            .ReturnsAsync(SignInResult.Success);

        // Act
        var result = await _usersUnitOfWork.LoginAsync(loginDTO);

        // Assert
        Assert.AreEqual(SignInResult.Success, result);
        _mockUsersRepository.Verify(x => x.LoginAsync(loginDTO), Times.Once);
    }

    [TestMethod]
    public async Task LogoutAsync_CallsRepositoryLogout()
    {
        // Act
        await _usersUnitOfWork.LogoutAsync();

        // Assert
        _mockUsersRepository.Verify(x => x.LogoutAsync(), Times.Once);
    }
}