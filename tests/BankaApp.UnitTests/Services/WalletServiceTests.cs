using BankaApp.Application.Common.Exceptions;
using BankaApp.Application.DTOs.Wallet;
using BankaApp.Application.Interfaces;
using BankaApp.Application.Services;
using BankaApp.Domain.Entities;
using BankaApp.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BankaApp.UnitTests.Services;

public class WalletServiceTests
{
    private readonly Mock<IWalletRepository> _wallets = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Guid _userId = Guid.NewGuid();

    private WalletService CreateSut() => new(_wallets.Object, _transactions.Object, _uow.Object);

    private Wallet CreateWallet(decimal balance = 100m) => new()
    {
        Id = Guid.NewGuid(),
        UserId = _userId,
        Balance = balance,
        Currency = "TRY"
    };

    [Fact]
    public async Task DepositAsync_Should_Increase_Balance_And_Write_Ledger()
    {
        var wallet = CreateWallet(100m);
        _wallets.Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var sut = CreateSut();

        var result = await sut.DepositAsync(_userId, new MoneyOperationRequest
        {
            Amount = 50m,
            Description = "Maaş"
        });

        result.Balance.Should().Be(150m);
        _transactions.Verify(x => x.AddAsync(
            It.Is<Transaction>(t =>
                t.Type == TransactionType.Deposit &&
                t.Amount == 50m &&
                t.ToWalletId == wallet.Id &&
                t.FromWalletId == null),
            It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_Should_Decrease_Balance_When_Funds_Enough()
    {
        var wallet = CreateWallet(100m);
        _wallets.Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var sut = CreateSut();

        var result = await sut.WithdrawAsync(_userId, new MoneyOperationRequest { Amount = 40m });

        result.Balance.Should().Be(60m);
        _transactions.Verify(x => x.AddAsync(
            It.Is<Transaction>(t =>
                t.Type == TransactionType.Withdrawal &&
                t.FromWalletId == wallet.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_Should_Fail_When_Insufficient_Balance()
    {
        var wallet = CreateWallet(20m);
        _wallets.Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var sut = CreateSut();

        var act = async () => await sut.WithdrawAsync(
            _userId,
            new MoneyOperationRequest { Amount = 50m });

        var ex = await act.Should().ThrowAsync<BusinessException>();
        ex.Which.ErrorCode.Should().Be("INSUFFICIENT_BALANCE");
        wallet.Balance.Should().Be(20m); // bakiyeye dokunulmamalı
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
