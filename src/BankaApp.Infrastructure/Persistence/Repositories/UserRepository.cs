using BankaApp.Application.Interfaces;
using BankaApp.Domain.Entities;
using BankaApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankaApp.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
        => _db.Users.AddAsync(user, cancellationToken).AsTask();

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _db.Users.AnyAsync(x => x.Email == email, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
