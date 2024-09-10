using Fantasy.Backend.Controllers;
using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Fantasy.Tests.Controllers;

[TestClass]
public class AccountsControllerTests
{
    private Mock<IUsersUnitOfWork> _mockUsersUnitOfWork = null!;
    private Mock<IConfiguration> _mockConfiguration = null!;
    private Mock<IMailHelper> _mockMailHelper = null!;
    private Mock<IFileStorage> _mockFileStorage = null!;
    private Mock<ClaimsIdentity> _mockClaimsIdentity = null!;
    private Mock<ClaimsPrincipal> _mockClaimsPrincipal = null!;
    private Mock<DataContext> _mockContext = null!;
    private AccountsController _controller = null!;
    private DataContext _context = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);

        _mockUsersUnitOfWork = new Mock<IUsersUnitOfWork>();
        _mockClaimsIdentity = new Mock<ClaimsIdentity>();
        _mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
        _mockContext = new Mock<DataContext>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockMailHelper = new Mock<IMailHelper>();
        _mockFileStorage = new Mock<IFileStorage>();

        _controller = new AccountsController(
            _mockUsersUnitOfWork.Object,
            _mockConfiguration.Object,
            _mockMailHelper.Object,
            _context,
            _mockFileStorage.Object);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsOk_WhenUserIsFound()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };

        // Mock User.Identity.Name to simulate the authenticated user email
        _mockClaimsIdentity.Setup(x => x.Name).Returns(user.Email);
        _mockClaimsPrincipal.Setup(x => x.Identity).Returns(_mockClaimsIdentity.Object);

        // Assign the mocked ClaimsPrincipal to the controller's HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _mockClaimsPrincipal.Object }
        };

        // Simulate GetUserAsync returning a valid user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);  // Check for 200 OK status code
        Assert.AreEqual(user, okResult.Value);  // Verify that the returned value is the mock user
    }

    [TestMethod]
    public async Task GetAsync_ReturnsOk_WhenPaginationIsSuccessful()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var users = new List<User> { new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" } };

        _mockUsersUnitOfWork.Setup(x => x.GetAsync(It.IsAny<PaginationDTO>()))
            .ReturnsAsync(new ActionResponse<IEnumerable<User>>
            {
                WasSuccess = true,
                Result = users
            });

        // Act
        var result = await _controller.GetAsync(pagination);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(users, okResult.Value);
    }

    [TestMethod]
    public async Task RecoverPasswordAsync_ReturnsNoContent_WhenEmailIsSentSuccessfully()
    {
        // Arrange
        var emailDTO = new EmailDTO { Email = "test@example.com", Language = "en" };
        var user = new User { Id = Guid.NewGuid().ToString(), Email = emailDTO.Email, FirstName = "John", LastName = "Doe" };

        // Mock the User retrieval and token generation
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(emailDTO.Email))
            .ReturnsAsync(user);

        _mockUsersUnitOfWork.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset_token");

        // Mock the configuration values for email subjects, bodies, and URL
        _mockConfiguration.Setup(x => x["Mail:SubjectRecoveryEn"]).Returns("Password Recovery");
        _mockConfiguration.Setup(x => x["Mail:BodyRecoveryEn"]).Returns("Please reset your password using this link: {0}");
        _mockConfiguration.Setup(x => x["Url Frontend"]).Returns("http://example.com");

        // Mock the Url.Action to return a valid URL
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://example.com/reset_password_link");
        _controller.Url = mockUrlHelper.Object;

        // Mock HttpContext and Request.Scheme
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Mock the email sending process
        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = true });

        // Act
        var result = await _controller.RecoverPasswordAsync(emailDTO);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);
    }

    [TestMethod]
    public async Task LoginAsync_ReturnsOk_WhenLoginIsSuccessful()
    {
        // Arrange
        var loginDTO = new LoginDTO { Email = "test@example.com", Password = "password" };
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = loginDTO.Email,
            FirstName = "John",
            LastName = "Doe",
            Photo = "some_photo_url",
            Country = new Country { Id = 1, Name = "Test Country" }
        };

        _mockUsersUnitOfWork.Setup(x => x.LoginAsync(loginDTO))
            .ReturnsAsync(SignInResult.Success);

        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(loginDTO.Email))
            .ReturnsAsync(user);

        // Provide a valid 256-bit (32-byte) key for JWT signing
        _mockConfiguration.Setup(x => x["jwtKey"]).Returns("this_is_a_very_secure_and_long_key_32_characters");

        // Act
        var result = await _controller.LoginAsync(loginDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.IsNotNull(okResult.Value); // Token should be generated
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsNoContent_WhenPasswordChangeIsSuccessful()
    {
        // Arrange
        var changePasswordDTO = new ChangePasswordDTO { CurrentPassword = "current", NewPassword = "newPassword" };
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };

        // Mock the User.Identity.Name to simulate an authenticated user
        var userIdentity = new GenericIdentity(user.Email);
        var principal = new GenericPrincipal(userIdentity, roles: null);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Mock the GetUserAsync method to return the user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Mock the ChangePasswordAsync method to return success
        _mockUsersUnitOfWork.Setup(x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.ChangePasswordAsync(changePasswordDTO);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);
    }

    [TestMethod]
    public async Task CreateUser_ReturnsNoContent_WhenUserIsCreatedSuccessfully()
    {
        // Arrange
        var userDTO = new UserDTO { Email = "test@example.com", Password = "password", CountryId = 1, Language = "en" };
        var user = new User { Id = Guid.NewGuid().ToString(), Email = userDTO.Email };
        var country = new Country { Id = 1, Name = "Country A" };

        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        // Mock AddUserAsync to return a successful identity result
        _mockUsersUnitOfWork.Setup(x => x.AddUserAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Mock AddUserToRoleAsync to complete successfully
        _mockUsersUnitOfWork.Setup(x => x.AddUserToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Mock SendMail for confirmation email to return a successful response
        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = true });

        // Mock the configuration values for confirmation email
        _mockConfiguration.Setup(x => x["Mail:SubjectConfirmationEn"]).Returns("Confirm your email");
        _mockConfiguration.Setup(x => x["Mail:BodyConfirmationEn"]).Returns("Please confirm your email using this link: {0}");
        _mockConfiguration.Setup(x => x["Url Frontend"]).Returns("http://example.com");

        // Mock Url.Action to return a valid confirmation URL
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://example.com/confirm_email_link");
        _controller.Url = mockUrlHelper.Object;

        // Mock HttpContext and Request.Scheme
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Act
        var result = await _controller.CreateUser(userDTO);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsBadRequest_WhenGetAsyncFails()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Simulate a failed response from the unit of work
        _mockUsersUnitOfWork.Setup(x => x.GetAsync(paginationDTO))
            .ReturnsAsync(new ActionResponse<IEnumerable<User>> { WasSuccess = false });

        // Act
        var result = await _controller.GetAsync(paginationDTO);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure the status code is 400
    }

    [TestMethod]
    public async Task GetPagesAsync_ReturnsOk_WhenGetTotalRecordsIsSuccessful()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };
        var totalRecords = 100;

        // Simulate a successful response from the unit of work
        _mockUsersUnitOfWork.Setup(x => x.GetTotalRecordsAsync(paginationDTO))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = true, Result = totalRecords });

        // Act
        var result = await _controller.GetPagesAsync(paginationDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);  // Ensure the status code is 200
        Assert.AreEqual(totalRecords, okResult.Value);  // Ensure the correct number of total records is returned
    }

    [TestMethod]
    public async Task GetPagesAsync_ReturnsBadRequest_WhenGetTotalRecordsFails()
    {
        // Arrange
        var paginationDTO = new PaginationDTO { Page = 1, RecordsNumber = 10 };

        // Simulate a failed response from the unit of work
        _mockUsersUnitOfWork.Setup(x => x.GetTotalRecordsAsync(paginationDTO))
            .ReturnsAsync(new ActionResponse<int> { WasSuccess = false });

        // Act
        var result = await _controller.GetPagesAsync(paginationDTO);

        // Assert
        var badRequestResult = result as BadRequestResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure the status code is 400
    }

    [TestMethod]
    public async Task RecoverPasswordAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var emailDTO = new EmailDTO { Email = "nonexistent@example.com", Language = "en" };

        // Simulate GetUserAsync returning null (user not found)
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(emailDTO.Email))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _controller.RecoverPasswordAsync(emailDTO);

        // Assert
        var notFoundResult = result as NotFoundResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode); // Ensure the status code is 404
    }

    [TestMethod]
    public async Task RecoverPasswordAsync_ReturnsBadRequest_WhenSendRecoverEmailFails()
    {
        // Arrange
        var emailDTO = new EmailDTO { Email = "test@example.com", Language = "en" };
        var user = new User { Id = Guid.NewGuid().ToString(), Email = emailDTO.Email };

        // Simulate GetUserAsync returning a valid user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(emailDTO.Email))
            .ReturnsAsync(user);

        // Simulate SendMail returning a failure response
        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = false, Message = "Failed to send email" });

        // Mock Url.Action to return a valid URL for the recovery email link
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://example.com/reset_password_link");
        _controller.Url = mockUrlHelper.Object;

        // Mock configuration values used in the email
        _mockConfiguration.Setup(x => x["Mail:SubjectRecoveryEn"]).Returns("Password Recovery");
        _mockConfiguration.Setup(x => x["Mail:BodyRecoveryEn"]).Returns("Click the link to reset your password: {0}");
        _mockConfiguration.Setup(x => x["Url Frontend"]).Returns("http://example.com");

        // Mock HttpContext and Request.Scheme
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");  // Mock the Request.Scheme to avoid null references
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

        // Set HttpContext for the controller
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Act
        var result = await _controller.RecoverPasswordAsync(emailDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure the status code is 400
        Assert.AreEqual("Failed to send email", badRequestResult.Value);  // Ensure the correct error message is returned
    }

    [TestMethod]
    public async Task ResetPasswordAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var resetPasswordDTO = new ResetPasswordDTO { Email = "nonexistent@example.com", Token = "token", NewPassword = "newPassword123" };

        // Simulate GetUserAsync returning null (user not found)
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(resetPasswordDTO.Email))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _controller.ResetPasswordAsync(resetPasswordDTO);

        // Assert
        var notFoundResult = result as NotFoundResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode); // Ensure the status code is 404
    }

    [TestMethod]
    public async Task ResetPasswordAsync_ReturnsNoContent_WhenPasswordResetIsSuccessful()
    {
        // Arrange
        var resetPasswordDTO = new ResetPasswordDTO { Email = "test@example.com", Token = "token", NewPassword = "newPassword123" };
        var user = new User { Id = Guid.NewGuid().ToString(), Email = resetPasswordDTO.Email };

        // Simulate GetUserAsync returning a valid user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(resetPasswordDTO.Email))
            .ReturnsAsync(user);

        // Simulate ResetPasswordAsync returning a successful result
        _mockUsersUnitOfWork.Setup(x => x.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.ResetPasswordAsync(resetPasswordDTO);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode); // Ensure the status code is 204
    }

    [TestMethod]
    public async Task ResetPasswordAsync_ReturnsBadRequest_WhenPasswordResetFails()
    {
        // Arrange
        var resetPasswordDTO = new ResetPasswordDTO { Email = "test@example.com", Token = "invalid_token", NewPassword = "newPassword123" };
        var user = new User { Id = Guid.NewGuid().ToString(), Email = resetPasswordDTO.Email };

        // Simulate GetUserAsync returning a valid user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(resetPasswordDTO.Email))
            .ReturnsAsync(user);

        // Simulate ResetPasswordAsync returning a failed result with an error message
        var identityResult = IdentityResult.Failed(new IdentityError { Description = "Invalid token" });
        _mockUsersUnitOfWork.Setup(x => x.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _controller.ResetPasswordAsync(resetPasswordDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode); // Ensure the status code is 400
        Assert.AreEqual("Invalid token", badRequestResult.Value); // Ensure the correct error message is returned
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var changePasswordDTO = new ChangePasswordDTO { CurrentPassword = "current", NewPassword = "newPassword" };

        // Mark ModelState as invalid
        _controller.ModelState.AddModelError("CurrentPassword", "Required");

        // Act
        var result = await _controller.ChangePasswordAsync(changePasswordDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode); // Ensure the status code is 400
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var changePasswordDTO = new ChangePasswordDTO { CurrentPassword = "current", NewPassword = "newPassword" };

        // Mock the User.Identity.Name to return a specific email (simulating an authenticated user)
        var userIdentityMock = new Mock<ClaimsIdentity>();
        userIdentityMock.Setup(x => x.Name).Returns("test@example.com");
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(x => x.Identity).Returns(userIdentityMock.Object);

        // Assign the mocked User to the controller's HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipalMock.Object }
        };

        // Simulate GetUserAsync returning null (user not found)
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _controller.ChangePasswordAsync(changePasswordDTO);

        // Assert
        var notFoundResult = result as NotFoundResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);  // Ensure the status code is 404
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsBadRequest_WhenPasswordChangeFails()
    {
        // Arrange
        var changePasswordDTO = new ChangePasswordDTO { CurrentPassword = "current", NewPassword = "newPassword" };
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };

        // Mock the User.Identity.Name to return a specific email (simulating an authenticated user)
        var userIdentityMock = new Mock<ClaimsIdentity>();
        userIdentityMock.Setup(x => x.Name).Returns(user.Email);
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(x => x.Identity).Returns(userIdentityMock.Object);

        // Assign the mocked User to the controller's HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipalMock.Object }
        };

        // Simulate GetUserAsync returning a valid user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Simulate ChangePasswordAsync returning a failed result with an error message
        var identityResult = IdentityResult.Failed(new IdentityError { Description = "Invalid password" });
        _mockUsersUnitOfWork.Setup(x => x.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _controller.ChangePasswordAsync(changePasswordDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure the status code is 400
        Assert.AreEqual("Invalid password", badRequestResult.Value);  // Ensure the correct error message is returned
    }

    [TestMethod]
    public async Task PutAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };

        // Mock the User.Identity.Name to return a specific email (simulating an authenticated user)
        var userIdentityMock = new Mock<ClaimsIdentity>();
        userIdentityMock.Setup(x => x.Name).Returns("test@example.com");
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(x => x.Identity).Returns(userIdentityMock.Object);

        // Assign the mocked User to the controller's HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipalMock.Object }
        };

        // Simulate GetUserAsync returning null (user not found)
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _controller.PutAsync(user);

        // Assert
        var notFoundResult = result as NotFoundResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);  // Ensure the status code is 404
    }

    [TestMethod]
    public async Task PutAsync_ReturnsOk_WhenUserUpdateIsSuccessful()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe", Photo = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }) };
        var currentUser = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", Photo = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }), Country = new Country { Id = 1, Name = "USA" } };

        // Mock the User.Identity.Name to return a specific email (simulating an authenticated user)
        var userIdentityMock = new Mock<ClaimsIdentity>();
        userIdentityMock.Setup(x => x.Name).Returns(currentUser.Email);
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(x => x.Identity).Returns(userIdentityMock.Object);

        // Assign the mocked User to the controller's HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipalMock.Object }
        };

        // Simulate GetUserAsync returning the current user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(currentUser);

        // Simulate a successful photo upload
        _mockFileStorage.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("new_photo_url");

        // Simulate a successful user update
        _mockUsersUnitOfWork.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Provide a long enough JWT key for HS256
        _mockConfiguration.Setup(x => x["jwtKey"]).Returns("32CharSecureKeyThatIsLongEnoughForHS256");

        // Act
        var result = await _controller.PutAsync(user);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);  // Ensure the result is OkObjectResult
        Assert.AreEqual(200, okResult.StatusCode);  // Ensure the status code is 200
        Assert.IsNotNull(okResult.Value);  // Ensure the token was returned
    }

    [TestMethod]
    public async Task PutAsync_ReturnsBadRequest_WhenUserUpdateFails()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };
        var currentUser = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };

        // Mock the User.Identity.Name to return a specific email (simulating an authenticated user)
        var userIdentityMock = new Mock<ClaimsIdentity>();
        userIdentityMock.Setup(x => x.Name).Returns(currentUser.Email);
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(x => x.Identity).Returns(userIdentityMock.Object);

        // Assign the mocked User to the controller's HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipalMock.Object }
        };

        // Simulate GetUserAsync returning the current user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(currentUser);

        // Simulate UpdateUserAsync returning a failed result
        var identityResult = IdentityResult.Failed(new IdentityError { Description = "Update failed" });
        _mockUsersUnitOfWork.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _controller.PutAsync(user);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure the status code is 400

        // Extract the IdentityError from the BadRequestObjectResult and check its description
        var identityError = badRequestResult.Value as IdentityError;
        Assert.IsNotNull(identityError);
        Assert.AreEqual("Update failed", identityError.Description);  // Ensure the correct error message is returned
    }

    [TestMethod]
    public async Task PutAsync_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Doe" };
        var currentUser = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };

        // Mock the User.Identity.Name to return a specific email (simulating an authenticated user)
        var userIdentityMock = new Mock<ClaimsIdentity>();
        userIdentityMock.Setup(x => x.Name).Returns(currentUser.Email);
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(x => x.Identity).Returns(userIdentityMock.Object);

        // Assign the mocked User to the controller's HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipalMock.Object }
        };

        // Simulate GetUserAsync returning the current user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(currentUser);

        // Simulate an exception being thrown when trying to update the user
        _mockUsersUnitOfWork.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("An error occurred"));

        // Act
        var result = await _controller.PutAsync(user);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure the status code is 400
        Assert.AreEqual("An error occurred", badRequestResult.Value);  // Ensure the correct error message is returned
    }

    [TestMethod]
    public async Task ResedTokenAsync_ReturnsNoContent_WhenEmailIsSentSuccessfully()
    {
        // Arrange
        var emailDTO = new EmailDTO { Email = "test@example.com", Language = "en" };
        var user = new User { Id = Guid.NewGuid().ToString(), Email = emailDTO.Email };

        // Simulate GetUserAsync returning a valid user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(emailDTO.Email))
            .ReturnsAsync(user);

        // Simulate SendConfirmationEmailAsync returning a success response
        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = true });

        // Mock Url.Action to return a valid URL
        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.Url = Mock.Of<IUrlHelper>(x => x.Action(It.IsAny<UrlActionContext>()) == "https://example.com/confirm");

        // Mock configuration to return non-null values for email subject and body
        _mockConfiguration.Setup(x => x["Mail:SubjectConfirmationEn"]).Returns("Confirm your email");
        _mockConfiguration.Setup(x => x["Mail:BodyConfirmationEn"]).Returns("Please confirm your email by clicking the link: {0}");

        // Act
        var result = await _controller.ResedTokenAsync(emailDTO);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);  // Ensure the status code is 204 No Content
    }

    [TestMethod]
    public async Task ResedTokenAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var emailDTO = new EmailDTO { Email = "test@example.com", Language = "en" };

        // Simulate GetUserAsync returning null (user not found)
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(emailDTO.Email))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _controller.ResedTokenAsync(emailDTO);

        // Assert
        var notFoundResult = result as NotFoundResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);  // Ensure the status code is 404 Not Found
    }

    [TestMethod]
    public async Task ResedTokenAsync_ReturnsBadRequest_WhenEmailSendFails()
    {
        // Arrange
        var emailDTO = new EmailDTO { Email = "test@example.com", Language = "en" };
        var user = new User { Id = Guid.NewGuid().ToString(), Email = emailDTO.Email };

        // Simulate GetUserAsync returning a valid user
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(emailDTO.Email))
            .ReturnsAsync(user);

        // Simulate SendConfirmationEmailAsync returning a failure response
        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = false, Message = "Failed to send email" });

        // Mock Url.Action to return a valid URL
        _controller.ControllerContext = new ControllerContext();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.Url = Mock.Of<IUrlHelper>(x => x.Action(It.IsAny<UrlActionContext>()) == "https://example.com/confirm");

        // Mock configuration to return non-null values for email subject and body
        _mockConfiguration.Setup(x => x["Mail:SubjectConfirmationEn"]).Returns("Confirm your email");
        _mockConfiguration.Setup(x => x["Mail:BodyConfirmationEn"]).Returns("Please confirm your email by clicking the link: {0}");

        // Act
        var result = await _controller.ResedTokenAsync(emailDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);  // Ensure the status code is 400 Bad Request
        Assert.AreEqual("Failed to send email", badRequestResult.Value);  // Ensure the correct error message is returned
    }

    [TestMethod]
    public async Task ConfirmEmailAsync_ReturnsNoContent_WhenEmailConfirmationIsSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var token = "valid_token";
        var user = new User { Id = userId };

        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        _mockUsersUnitOfWork.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.ConfirmEmailAsync(userId, token);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);
    }

    [TestMethod]
    public async Task ConfirmEmailAsync_ReturnsBadRequest_WhenEmailConfirmationFails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var token = "valid_token";
        var user = new User { Id = userId };

        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        _mockUsersUnitOfWork.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        // Act
        var result = await _controller.ConfirmEmailAsync(userId, token);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        // Verify that the error message is correct
        var identityError = badRequestResult.Value as IdentityError;
        Assert.IsNotNull(identityError);
        Assert.AreEqual("Invalid token", identityError.Description);
    }

    [TestMethod]
    public async Task CreateUser_ReturnsBadRequest_WhenCountryNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new DataContext(options);
        var userDTO = new UserDTO { Email = "test@example.com", Password = "password", CountryId = 999, Language = "en" };

        // Create controller with the in-memory context
        _controller = new AccountsController(
            _mockUsersUnitOfWork.Object,
            _mockConfiguration.Object,
            _mockMailHelper.Object,
            context,
            _mockFileStorage.Object);

        // Act
        var result = await _controller.CreateUser(userDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("ERR004", badRequestResult.Value);
    }

    [TestMethod]
    public async Task LoginAsync_ReturnsBadRequest_WhenLoginFails()
    {
        // Arrange
        var loginDTO = new LoginDTO { Email = "test@example.com", Password = "wrong_password" };

        _mockUsersUnitOfWork.Setup(x => x.LoginAsync(loginDTO))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _controller.LoginAsync(loginDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("ERR006", badRequestResult.Value);
    }

    [TestMethod]
    public async Task SendRecoverEmailAsync_ReturnsSuccess_WhenEmailIsSent()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };

        _mockUsersUnitOfWork.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset_token");

        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = true });

        // Mock Url.Action to return a valid URL for the password reset email link
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://example.com/reset_password_link");
        _controller.Url = mockUrlHelper.Object;

        // Mock HttpContext and Request.Scheme to avoid NullReferenceException
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Mock configuration for email subject and body to avoid null references
        _mockConfiguration.Setup(x => x["Mail:SubjectRecoveryEn"]).Returns("Password Recovery");
        _mockConfiguration.Setup(x => x["Mail:BodyRecoveryEn"]).Returns("Please reset your password using this link: {0}");

        // Act
        var result = await _controller.SendRecoverEmailAsync(user, "en");

        // Assert
        Assert.IsTrue(result.WasSuccess);
    }

    [TestMethod]
    public async Task SendRecoverEmailAsync_ReturnsFailure_WhenEmailFails()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };

        _mockUsersUnitOfWork.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset_token");

        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = false, Message = "Failed to send email" });

        // Mock Url.Action to return a valid URL for the password reset email link
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://example.com/reset_password_link");
        _controller.Url = mockUrlHelper.Object;

        // Mock HttpContext and Request.Scheme to avoid NullReferenceException
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Mock configuration for email subject and body to avoid null references
        _mockConfiguration.Setup(x => x["Mail:SubjectRecoveryEn"]).Returns("Password Recovery");
        _mockConfiguration.Setup(x => x["Mail:BodyRecoveryEn"]).Returns("Please reset your password using this link: {0}");

        // Act
        var result = await _controller.SendRecoverEmailAsync(user, "en");

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Failed to send email", result.Message);
    }

    [TestMethod]
    public async Task SendConfirmationEmailAsync_ReturnsSuccess_WhenEmailIsSent()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };

        _mockUsersUnitOfWork.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation_token");

        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = true });

        // Mock Url.Action to return a valid URL for the confirmation email link
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://example.com/confirm_email_link");
        _controller.Url = mockUrlHelper.Object;

        // Mock HttpContext and Request.Scheme to avoid NullReferenceException
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Mock configuration for email subject and body to avoid null references
        _mockConfiguration.Setup(x => x["Mail:SubjectConfirmationEn"]).Returns("Confirm your email");
        _mockConfiguration.Setup(x => x["Mail:BodyConfirmationEn"]).Returns("Please confirm your email using this link: {0}");

        // Act
        var result = await _controller.SendConfirmationEmailAsync(user, "en");

        // Assert
        Assert.IsTrue(result.WasSuccess);
    }

    [TestMethod]
    public async Task SendConfirmationEmailAsync_ReturnsFailure_WhenEmailFails()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com" };

        _mockUsersUnitOfWork.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation_token");

        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = false, Message = "Failed to send email" });

        // Mock Url.Action to return a valid URL for the confirmation email link
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://example.com/confirm_email_link");
        _controller.Url = mockUrlHelper.Object;

        // Mock HttpContext and Request.Scheme to avoid NullReferenceException
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Mock configuration for email subject and body to avoid null references
        _mockConfiguration.Setup(x => x["Mail:SubjectConfirmationEn"]).Returns("Confirm your email");
        _mockConfiguration.Setup(x => x["Mail:BodyConfirmationEn"]).Returns("Please confirm your email using this link: {0}");

        // Act
        var result = await _controller.SendConfirmationEmailAsync(user, "en");

        // Assert
        Assert.IsFalse(result.WasSuccess);
        Assert.AreEqual("Failed to send email", result.Message);
    }

    [TestMethod]
    public async Task SendRecoverEmailAsync_ReturnsSuccess_WhenEmailIsSent_InSpanish()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", FirstName = "John", LastName = "Doe" };

        _mockUsersUnitOfWork.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset_token");

        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = true });

        // Mock Url.Action to return a valid URL for the password reset email link
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://example.com/reset_password_link");
        _controller.Url = mockUrlHelper.Object;

        // Mock HttpContext and Request.Scheme
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Mock configuration for Spanish email subject and body
        _mockConfiguration.Setup(x => x["Mail:SubjectRecoveryEs"]).Returns("Recuperar Contraseña");
        _mockConfiguration.Setup(x => x["Mail:BodyRecoveryEs"]).Returns("Restablece tu contraseña usando este enlace: {0}");

        // Act
        var result = await _controller.SendRecoverEmailAsync(user, "es");

        // Assert
        Assert.IsTrue(result.WasSuccess);
    }

    [TestMethod]
    public async Task SendConfirmationEmailAsync_ReturnsSuccess_WhenEmailIsSent_InSpanish()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", FirstName = "John", LastName = "Doe" };

        _mockUsersUnitOfWork.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation_token");

        _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ActionResponse<string> { WasSuccess = true });

        // Mock Url.Action to return a valid URL for the confirmation email link
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://example.com/confirm_email_link");
        _controller.Url = mockUrlHelper.Object;

        // Mock HttpContext and Request.Scheme
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Mock configuration for Spanish email subject and body
        _mockConfiguration.Setup(x => x["Mail:SubjectConfirmationEs"]).Returns("Confirma tu correo");
        _mockConfiguration.Setup(x => x["Mail:BodyConfirmationEs"]).Returns("Confirma tu correo usando este enlace: {0}");

        // Act
        var result = await _controller.SendConfirmationEmailAsync(user, "es");

        // Assert
        Assert.IsTrue(result.WasSuccess);
    }

    [TestMethod]
    public async Task LoginAsync_ReturnsBadRequest_WhenUserIsLockedOut()
    {
        // Arrange
        var loginDTO = new LoginDTO { Email = "lockedout@example.com", Password = "password" };

        // Simulate that the login attempt results in a locked-out state
        _mockUsersUnitOfWork.Setup(x => x.LoginAsync(loginDTO))
            .ReturnsAsync(SignInResult.LockedOut);

        // Act
        var result = await _controller.LoginAsync(loginDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("ERR007", badRequestResult.Value);  // Verify the correct error message
    }

    [TestMethod]
    public async Task LoginAsync_ReturnsBadRequest_WhenLoginIsNotAllowed()
    {
        // Arrange
        var loginDTO = new LoginDTO { Email = "notallowed@example.com", Password = "password" };

        // Simulate that the login attempt results in a not allowed state
        _mockUsersUnitOfWork.Setup(x => x.LoginAsync(loginDTO))
            .ReturnsAsync(SignInResult.NotAllowed);

        // Act
        var result = await _controller.LoginAsync(loginDTO);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("ERR008", badRequestResult.Value);  // Verify the correct error message
    }

    [TestMethod]
    public async Task ConfirmEmailAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();  // Simulate a valid user ID
        var token = "valid_token";

        // Simulate GetUserAsync returning null, indicating the user does not exist
        _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _controller.ConfirmEmailAsync(userId, token);

        // Assert
        var notFoundResult = result as NotFoundResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);  // Verify the status code is 404 Not Found
    }
}