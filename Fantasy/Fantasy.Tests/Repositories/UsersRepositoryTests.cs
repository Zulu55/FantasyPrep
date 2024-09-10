using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Fantasy.Tests.Repositories;

[TestClass]
public class UsersRepositoryTests
{
    private UsersRepository _usersRepository = null!;
    private DataContext _context = null!;
    private Mock<UserManager<User>> _mockUserManager = null!;
    private Mock<SignInManager<User>> _mockSignInManager = null!;
    private Mock<RoleManager<IdentityRole>> _mockRoleManager = null!;
    private Mock<IFileStorage> _mockFileStorage = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _context = new DataContext(options);

        // Setup mocks
        _mockUserManager = MockUserManager();
        _mockSignInManager = MockSignInManager();
        _mockRoleManager = MockRoleManager();
        _mockFileStorage = new Mock<IFileStorage>();

        _usersRepository = new UsersRepository(
            _context,
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockSignInManager.Object,
            _mockFileStorage.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted(); // Clean up the database after each test
        _context.Dispose();
    }

    private Mock<SignInManager<User>> MockSignInManager()
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var logger = new Mock<ILogger<SignInManager<User>>>();
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        var userConfirmation = new Mock<IUserConfirmation<User>>();

        return new Mock<SignInManager<User>>(
            _mockUserManager.Object,
            httpContextAccessor.Object,
            claimsFactory.Object,
            options.Object,
            logger.Object,
            schemes.Object,
            userConfirmation.Object
        );
    }

    // Helper methods to mock UserManager, SignInManager, RoleManager
    private Mock<UserManager<User>> MockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private Mock<RoleManager<IdentityRole>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();
        return new Mock<RoleManager<IdentityRole>>(store.Object, null!, null!, null!, null!);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsUsersWithPagination()
    {
        // Arrange: Set up the in-memory database options with a unique name for this test
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // Unique database for each test
            .Options;

        using var context = new DataContext(options);

        // Create and add the country once
        var country = new Country { Id = 1, Name = "TestCountry" };
        await context.Countries.AddAsync(country);
        await context.SaveChangesAsync(); // Save the country to avoid conflicts

        // Create users and associate them with the country
        var user1 = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe", Country = country };
        var user2 = new User { Id = Guid.NewGuid().ToString(), FirstName = "Jane", LastName = "Doe", Country = country };

        // Add users to the in-memory database
        await context.Users.AddRangeAsync(user1, user2);
        await context.SaveChangesAsync(); // Save the users

        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Create the UsersRepository with the in-memory context
        var repository = new UsersRepository(context, null!, null!, null!, null!);

        // Act: Retrieve users with pagination
        var result = await repository.GetAsync(pagination);

        // Assert: Verify that the result was successful and contains 2 users
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result!.Count());
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_ReturnsCorrectCount()
    {
        // Arrange: Set up the in-memory database options with a unique name for this test
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // Unique database for each test
            .Options;

        using var context = new DataContext(options);

        // Create a user and add it to the in-memory database
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "John",
            LastName = "Doe"
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Set up pagination parameters
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Create the UsersRepository with the in-memory context
        var repository = new UsersRepository(context, null!, null!, null!, null!);

        // Act: Get the total records count
        var result = await repository.GetTotalRecordsAsync(pagination);

        // Assert: Verify that the total count is correct
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result);
    }

    [TestMethod]
    public async Task GeneratePasswordResetTokenAsync_ReturnsToken()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset_token");

        // Act
        var token = await _usersRepository.GeneratePasswordResetTokenAsync(user);

        // Assert
        Assert.AreEqual("reset_token", token);
    }

    [TestMethod]
    public async Task ResetPasswordAsync_ReturnsSuccessResult()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        _mockUserManager.Setup(x => x.ResetPasswordAsync(user, "token", "new_password"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersRepository.ResetPasswordAsync(user, "token", "new_password");

        // Assert
        Assert.AreEqual(IdentityResult.Success, result);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsSuccessResult()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, "old_password", "new_password"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersRepository.ChangePasswordAsync(user, "old_password", "new_password");

        // Assert
        Assert.AreEqual(IdentityResult.Success, result);
    }

    [TestMethod]
    public async Task AddUserAsync_ReturnsSuccessResult_WhenPhotoIsNotProvided()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        _mockUserManager.Setup(x => x.CreateAsync(user, "password"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _usersRepository.AddUserAsync(user, "password");

        // Assert
        Assert.AreEqual(IdentityResult.Success, result);
    }

    [TestMethod]
    public async Task AddUserAsync_StoresPhoto_WhenProvided()
    {
        // Arrange: Set up the in-memory database options
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        // Create a new DataContext with the in-memory database
        using var context = new DataContext(options);

        // Create a valid Base64 string for the photo
        var validBase64Photo = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });

        // Create a user and associate the photo with it
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",   // Required field
            LastName = "Doe",      // Required field
            Photo = validBase64Photo
        };

        // Mock FileStorage to simulate saving the photo
        var mockFileStorage = new Mock<IFileStorage>();
        mockFileStorage.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "users"))
            .ReturnsAsync("http://someurl.com/photo.jpg");

        // Mock UserManager to simulate user creation
        var mockUserManager = MockUserManager();
        mockUserManager.Setup(x => x.CreateAsync(user, "password"))
            .ReturnsAsync(IdentityResult.Success);

        // Create UsersRepository using the in-memory database and mocked dependencies
        var usersRepository = new UsersRepository(context, mockUserManager.Object, null!, null!, mockFileStorage.Object);

        // Act: Add the user and store the photo
        var result = await usersRepository.AddUserAsync(user, "password");

        // Assert: Verify that the user creation was successful and the photo was saved
        Assert.AreEqual(IdentityResult.Success, result);

        // Verify that the photo was saved and the URL was updated
        mockFileStorage.Verify(x => x.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", "users"), Times.Once);
        Assert.AreEqual("http://someurl.com/photo.jpg", user.Photo);
    }

    [TestMethod]
    public async Task GetUserAsync_ById_ReturnsUser()
    {
        // Arrange: Set up the in-memory database options
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        // Create a new DataContext with the in-memory database
        using var context = new DataContext(options);

        // Create a country and add it to the in-memory database
        var country = new Country { Id = 1, Name = "TestCountry" };
        await context.Countries.AddAsync(country);
        await context.SaveChangesAsync();

        // Create a user and add it to the in-memory database
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",       // Required property
            LastName = "Doe",         // Required property
            Country = country         // Set the Country
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync(); // Ensure the user is saved

        // Create the UsersRepository with the in-memory context
        var repository = new UsersRepository(context, null!, null!, null!, null!);

        // Act: Retrieve the user by ID
        var result = await repository.GetUserAsync(Guid.Parse(user.Id));

        // Assert: Verify that the user retrieved is the one added
        Assert.IsNotNull(result, "User should not be null");
        Assert.AreEqual(user.Email, result.Email);
        Assert.AreEqual(user.FirstName, result.FirstName);
        Assert.AreEqual(user.LastName, result.LastName);
        Assert.IsNotNull(result.Country);
        Assert.AreEqual(country.Name, result.Country.Name);
    }

    [TestMethod]
    public async Task GetUserAsync_ByEmail_ReturnsUser()
    {
        // Arrange: Set up the in-memory database options with a unique name for each test
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // Unique database for each test
            .Options;

        // Create a new DataContext with the in-memory database
        using var context = new DataContext(options);

        // Create a country and add it to the in-memory database with a unique Id
        var country = new Country
        {
            Id = 1,   // Ensure this Id is unique across your tests, or use Guid.NewGuid().ToString() if possible
            Name = "TestCountry"
        };

        await context.Countries.AddAsync(country);
        await context.SaveChangesAsync();  // Save the country to avoid conflicts

        // Create a user and associate it with the country
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",       // Required property
            LastName = "Doe",         // Required property
            Country = country         // Attach the country to the user
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();  // Ensure the user is saved

        // Verify the user was saved in the database
        var savedUser = await context.Users.Include(u => u.Country).FirstOrDefaultAsync(u => u.Email == user.Email);
        Assert.IsNotNull(savedUser, "The user was not saved in the in-memory database.");

        // Create the UsersRepository with the in-memory context
        var repository = new UsersRepository(context, null!, null!, null!, null!);

        // Act: Retrieve the user by email
        var result = await repository.GetUserAsync(user.Email);

        // Assert: Verify that the user retrieved is the one added
        Assert.IsNotNull(result, "User should not be null");
        Assert.AreEqual(user.Email, result.Email);
        Assert.AreEqual(user.FirstName, result.FirstName);
        Assert.AreEqual(user.LastName, result.LastName);
        Assert.IsNotNull(result.Country);
        Assert.AreEqual(country.Name, result.Country.Name);
    }

    [TestMethod]
    public async Task LoginAsync_ReturnsSignInResult()
    {
        // Arrange
        var loginDTO = new LoginDTO { Email = "test@example.com", Password = "password" };
        _mockSignInManager.Setup(x => x.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, false, true))
            .ReturnsAsync(SignInResult.Success);

        // Act
        var result = await _usersRepository.LoginAsync(loginDTO);

        // Assert
        Assert.AreEqual(SignInResult.Success, result);
    }

    [TestMethod]
    public async Task LogoutAsync_CallsSignOut()
    {
        // Act
        await _usersRepository.LogoutAsync();

        // Assert
        _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
    }

    [TestMethod]
    public async Task IsUserInRoleAsync_ReturnsTrue_WhenUserIsInRole()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        var roleName = "Admin";

        // Mock UserManager to return true for IsInRoleAsync
        var mockUserManager = MockUserManager();
        mockUserManager.Setup(x => x.IsInRoleAsync(user, roleName))
            .ReturnsAsync(true);

        // Create UsersRepository with the mocked UserManager
        var usersRepository = new UsersRepository(null!, mockUserManager.Object, null!, null!, null!);

        // Act: Call the IsUserInRoleAsync method
        var result = await usersRepository.IsUserInRoleAsync(user, roleName);

        // Assert: Verify that the result is true
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsUserInRoleAsync_ReturnsFalse_WhenUserIsNotInRole()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        var roleName = "Admin";

        // Mock UserManager to return false for IsInRoleAsync
        var mockUserManager = MockUserManager();
        mockUserManager.Setup(x => x.IsInRoleAsync(user, roleName))
            .ReturnsAsync(false);

        // Create UsersRepository with the mocked UserManager
        var usersRepository = new UsersRepository(null!, mockUserManager.Object, null!, null!, null!);

        // Act: Call the IsUserInRoleAsync method
        var result = await usersRepository.IsUserInRoleAsync(user, roleName);

        // Assert: Verify that the result is false
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CheckRoleAsync_DoesNotCreateRole_WhenRoleExists()
    {
        // Arrange
        var roleName = "Admin";

        // Mock RoleManager to return true when checking if the role exists
        _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
            .ReturnsAsync(true);

        // Create the UsersRepository with the mocked RoleManager
        var usersRepository = new UsersRepository(null!, null!, _mockRoleManager.Object, null!, null!);

        // Act
        await usersRepository.CheckRoleAsync(roleName);

        // Assert: Verify that CreateAsync was never called since the role already exists
        _mockRoleManager.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
    }

    [TestMethod]
    public async Task CheckRoleAsync_CreatesRole_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleName = "Admin";

        // Mock RoleManager to return false when checking if the role exists
        _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
            .ReturnsAsync(false);

        // Mock RoleManager to simulate successful role creation
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Create the UsersRepository with the mocked RoleManager
        var usersRepository = new UsersRepository(null!, null!, _mockRoleManager.Object, null!, null!);

        // Act
        await usersRepository.CheckRoleAsync(roleName);

        // Assert: Verify that CreateAsync was called once with the correct role
        _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == roleName)), Times.Once);
    }

    [TestMethod]
    public async Task AddUserToRoleAsync_AddsUserToRoleSuccessfully()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        var roleName = "Admin";

        // Mock UserManager to simulate successful role addition
        _mockUserManager.Setup(x => x.AddToRoleAsync(user, roleName))
            .ReturnsAsync(IdentityResult.Success);

        // Create UsersRepository with the mocked UserManager
        var usersRepository = new UsersRepository(null!, _mockUserManager.Object, null!, null!, null!);

        // Act: Call the AddUserToRoleAsync method
        await usersRepository.AddUserToRoleAsync(user, roleName);

        // Assert: Verify that AddToRoleAsync was called once with the correct parameters
        _mockUserManager.Verify(x => x.AddToRoleAsync(user, roleName), Times.Once);
    }

    [TestMethod]
    public async Task GenerateEmailConfirmationTokenAsync_ReturnsToken()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        var expectedToken = "testToken";

        // Mock UserManager to return a token when GenerateEmailConfirmationTokenAsync is called
        _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(expectedToken);

        // Create UsersRepository with the mocked UserManager
        var usersRepository = new UsersRepository(null!, _mockUserManager.Object, null!, null!, null!);

        // Act: Call GenerateEmailConfirmationTokenAsync
        var result = await usersRepository.GenerateEmailConfirmationTokenAsync(user);

        // Assert: Verify that the returned token matches the expected value
        Assert.AreEqual(expectedToken, result);
    }

    [TestMethod]
    public async Task ConfirmEmailAsync_ReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        var token = "validToken";

        // Mock UserManager to simulate a successful confirmation
        _mockUserManager.Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Success);

        // Create UsersRepository with the mocked UserManager
        var usersRepository = new UsersRepository(null!, _mockUserManager.Object, null!, null!, null!);

        // Act: Call ConfirmEmailAsync
        var result = await usersRepository.ConfirmEmailAsync(user, token);

        // Assert: Verify that the confirmation was successful
        Assert.AreEqual(IdentityResult.Success, result);
    }

    [TestMethod]
    public async Task ConfirmEmailAsync_ReturnsFailure_WhenInvalidToken()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };
        var token = "invalidToken";

        // Mock UserManager to simulate a failed confirmation
        var identityResult = IdentityResult.Failed(new IdentityError { Description = "Invalid token" });
        _mockUserManager.Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(identityResult);

        // Create UsersRepository with the mocked UserManager
        var usersRepository = new UsersRepository(null!, _mockUserManager.Object, null!, null!, null!);

        // Act: Call ConfirmEmailAsync with an invalid token
        var result = await usersRepository.ConfirmEmailAsync(user, token);

        // Assert: Verify that the confirmation failed
        Assert.AreEqual(identityResult, result);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("Invalid token", result.Errors.First().Description);
    }

    [TestMethod]
    public async Task UpdateUserAsync_ReturnsSuccess_WhenUserIsUpdatedSuccessfully()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", FirstName = "John", LastName = "Doe" };

        // Mock UserManager to simulate successful user update
        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Create UsersRepository with the mocked UserManager
        var usersRepository = new UsersRepository(null!, _mockUserManager.Object, null!, null!, null!);

        // Act: Call UpdateUserAsync
        var result = await usersRepository.UpdateUserAsync(user);

        // Assert: Verify that the update was successful
        Assert.AreEqual(IdentityResult.Success, result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_ReturnsFailure_WhenUserUpdateFails()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", FirstName = "John", LastName = "Doe" };

        // Mock UserManager to simulate failed user update
        var identityResult = IdentityResult.Failed(new IdentityError { Description = "Update failed" });
        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(identityResult);

        // Create UsersRepository with the mocked UserManager
        var usersRepository = new UsersRepository(null!, _mockUserManager.Object, null!, null!, null!);

        // Act: Call UpdateUserAsync
        var result = await usersRepository.UpdateUserAsync(user);

        // Assert: Verify that the update failed
        Assert.AreEqual(identityResult, result);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("Update failed", result.Errors.First().Description);
    }

    [TestMethod]
    public async Task GetTotalRecordsAsync_WithFilter_ReturnsFilteredCount()
    {
        // Arrange: Add users to the in-memory database
        var user1 = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };
        var user2 = new User { Id = Guid.NewGuid().ToString(), FirstName = "Jane", LastName = "Smith" };
        var user3 = new User { Id = Guid.NewGuid().ToString(), FirstName = "Michael", LastName = "Johnson" };

        await _context.Users.AddRangeAsync(user1, user2, user3);
        await _context.SaveChangesAsync();

        // Create a PaginationDTO with a filter that matches "John"
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10, Filter = "John" };

        // Act: Call GetTotalRecordsAsync with the filter
        var result = await _usersRepository.GetTotalRecordsAsync(pagination);

        // Assert: Verify that the result is correct (should match 2 users: "John Doe" and "Michael Johnson")
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result); // Expecting 2 users with "John" in either FirstName or LastName
    }

    [TestMethod]
    public async Task GetAsync_WithFilter_ReturnsFilteredUsers()
    {
        // Arrange: Add users and a country to the in-memory database
        var country = new Country { Id = 1, Name = "TestCountry" };
        var user1 = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe", Country = country };
        var user2 = new User { Id = Guid.NewGuid().ToString(), FirstName = "Jane", LastName = "Smith", Country = country };
        var user3 = new User { Id = Guid.NewGuid().ToString(), FirstName = "Michael", LastName = "Johnson", Country = country };

        await _context.Countries.AddAsync(country);
        await _context.Users.AddRangeAsync(user1, user2, user3);
        await _context.SaveChangesAsync();

        // Create a PaginationDTO with a filter that matches "John"
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10, Filter = "John" };

        // Act: Call GetAsync with the filter
        var result = await _usersRepository.GetAsync(pagination);

        // Assert: Verify that the result contains the correct users (should match "John Doe" and "Michael Johnson")
        Assert.IsTrue(result.WasSuccess);
        var filteredUsers = result.Result!.ToList();
        Assert.AreEqual(2, filteredUsers.Count); // Expecting 2 users
        Assert.IsTrue(filteredUsers.Any(u => u.FirstName == "John" && u.LastName == "Doe"));
        Assert.IsTrue(filteredUsers.Any(u => u.FirstName == "Michael" && u.LastName == "Johnson"));
    }
}