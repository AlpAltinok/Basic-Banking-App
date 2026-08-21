using BankaApp.Application.DTOs.Auth;
using BankaApp.Application.Interfaces;
using BankaApp.Application.Services;
using BankaApp.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BankaApp.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IWalletRepository> _wallets = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();

    private AuthService CreateSut() => new(
        _users.Object,
        _wallets.Object,
        _uow.Object,
        _hasher.Object,
        _jwt.Object);

    [Fact]
    public async Task RegisterAsync_Should_Create_User_Wallet_And_Return_Token()
    {
        // Arrange — ne bekliyoruz?
        _users.Setup(x => x.ExistsByEmailAsync("eren@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hasher.Setup(x => x.Hash("Sifre123!")).Returns("hashed");
        _jwt.Setup(x => x.CreateToken(It.IsAny<User>()))
            .Returns(("fake-jwt", DateTime.UtcNow.AddHours(1)));

        var sut = CreateSut();

        // Act
        var result = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "eren@example.com",
            FullName = "Eren",
            Password = "Sifre123!"
        });

        // Assert — neden önemli?
        result.Email.Should().Be("eren@example.com");
        result.AccessToken.Should().Be("fake-jwt");
        _users.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _wallets.Verify(x => x.AddAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
