using BankaApp.Application.Common.Exceptions;
using BankaApp.Application.DTOs.Auth;
using BankaApp.Application.Interfaces;
using BankaApp.Domain.Entities;

namespace BankaApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new BusinessException("EMAIL_ALREADY_EXISTS", "Bu e-posta adresi zaten kayıtlı.");
        }

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        // Kayıtta otomatik boş cüzdan açılır — bankada hesap açılışına benzer.
        var wallet = new Wallet
        {
            UserId = user.Id,
            Balance = 0m,
            Currency = "TRY"
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _walletRepository.AddAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        // Güvenlik: "kullanıcı yok" ile "şifre yanlış"ı ayırma (enumeration riski).
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new BusinessException("INVALID_CREDENTIALS", "E-posta veya şifre hatalı.");
        }

        if (!user.IsActive)
        {
            throw new BusinessException("USER_INACTIVE", "Hesap aktif değil.");
        }

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var (token, expiresAt) = _jwtTokenService.CreateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AccessToken = token,
            ExpiresAtUtc = expiresAt
        };
    }
}
