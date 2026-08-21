using BankaApp.Application.Common.Exceptions;
using BankaApp.Application.DTOs.Transfer;
using BankaApp.Application.Interfaces;
using BankaApp.Application.Services;
using BankaApp.Domain.Entities;
using BankaApp.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BankaApp.UnitTests.Services;

public class TransferServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IWalletRepository> _wallets = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private readonly Guid _fromUserId = Guid.NewGuid();
    private readonly Guid _toUserId = Guid.NewGuid();

    private TransferService CreateSut()
    {
        // Transaction callback'ini gerçekten çalıştır (SaveChanges UoW mock'unda).
        _uow.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<TransferResponse>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<TransferResponse>>, CancellationToken>(
                async (op, ct) => await op(ct));

        return new TransferService(_users.Object, _wallets.Object, _transactions.Object, _uow.Object);
    }

    [Fact]
    public async Task TransferAsync_Should_Move_Money_Between_Wallets()
    {
        var fromUser = new User { Id = _fromUserId, Email = "ali@banka.app" };
        var toUser = new User { Id = _toUserId, Email = "veli@banka.app" };
        var fromWallet = new Wallet { Id = Guid.NewGuid(), UserId = _fromUserId, Balance = 500m, Currency = "TRY" };
        var toWallet = new Wallet { Id = Guid.NewGuid(), UserId = _toUserId, Balance = 100m, Currency = "TRY" };

        _users.Setup(x => x.GetByIdAsync(_fromUserId, It.IsAny<CancellationToken>())).ReturnsAsync(fromUser);
        _users.Setup(x => x.GetByEmailAsync("veli@banka.app", It.IsAny<CancellationToken>())).ReturnsAsync(toUser);
        _wallets.Setup(x => x.GetByUserIdAsync(_fromUserId, It.IsAny<CancellationToken>())).ReturnsAsync(fromWallet);
        _wallets.Setup(x => x.GetByUserIdAsync(_toUserId, It.IsAny<CancellationToken>())).ReturnsAsync(toWallet);

        var sut = CreateSut();

        var result = await sut.TransferAsync(_fromUserId, new TransferRequest
        {
            ToEmail = "veli@banka.app",
            Amount = 150m,
            Description = "Borç"
        });

        fromWallet.Balance.Should().Be(350m);
        toWallet.Balance.Should().Be(250m);
        result.Amount.Should().Be(150m);
        result.SenderBalanceAfter.Should().Be(350m);

        _transactions.Verify(x => x.AddAsync(
            It.Is<Transaction>(t =>
                t.Type == TransactionType.Transfer &&
                t.FromWalletId == fromWallet.Id &&
                t.ToWalletId == toWallet.Id &&
                t.Amount == 150m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransferAsync_Should_Fail_On_Insufficient_Balance()
    {
        var fromUser = new User { Id = _fromUserId, Email = "ali@banka.app" };
        var toUser = new User { Id = _toUserId, Email = "veli@banka.app" };
        var fromWallet = new Wallet { Id = Guid.NewGuid(), UserId = _fromUserId, Balance = 50m };
        var toWallet = new Wallet { Id = Guid.NewGuid(), UserId = _toUserId, Balance = 0m };

        _users.Setup(x => x.GetByIdAsync(_fromUserId, It.IsAny<CancellationToken>())).ReturnsAsync(fromUser);
        _users.Setup(x => x.GetByEmailAsync("veli@banka.app", It.IsAny<CancellationToken>())).ReturnsAsync(toUser);
        _wallets.Setup(x => x.GetByUserIdAsync(_fromUserId, It.IsAny<CancellationToken>())).ReturnsAsync(fromWallet);
        _wallets.Setup(x => x.GetByUserIdAsync(_toUserId, It.IsAny<CancellationToken>())).ReturnsAsync(toWallet);

        var sut = CreateSut();

        var act = async () => await sut.TransferAsync(_fromUserId, new TransferRequest
        {
            ToEmail = "veli@banka.app",
            Amount = 100m
        });

        var ex = await act.Should().ThrowAsync<BusinessException>();
        ex.Which.ErrorCode.Should().Be("INSUFFICIENT_BALANCE");
        fromWallet.Balance.Should().Be(50m);
        toWallet.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task TransferAsync_Should_Reject_Self_Transfer()
    {
        var user = new User { Id = _fromUserId, Email = "ali@banka.app" };
        _users.Setup(x => x.GetByIdAsync(_fromUserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _users.Setup(x => x.GetByEmailAsync("ali@banka.app", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var sut = CreateSut();

        var act = async () => await sut.TransferAsync(_fromUserId, new TransferRequest
        {
            ToEmail = "ali@banka.app",
            Amount = 10m
        });

        var ex = await act.Should().ThrowAsync<BusinessException>();
        ex.Which.ErrorCode.Should().Be("SELF_TRANSFER");
    }
}
