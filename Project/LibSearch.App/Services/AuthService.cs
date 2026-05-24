using LibSearch.App.Data;
using LibSearch.App.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibSearch.App.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthService(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<(bool Ok, string? Error, User? User)> RegisterAsync(string username, string password, CancellationToken ct = default)
    {
        username = (username ?? string.Empty).Trim();
        if (username.Length < 3)
            return (false, "Username must be at least 3 characters.", null);

        var exists = await _db.Users.AnyAsync(u => u.Username == username, ct).ConfigureAwait(false);
        if (exists)
            return (false, "Username already taken.", null);

        var user = new User
        {
            Username = username,
            PasswordHash = _hasher.Hash(password),
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return (true, null, user);
    }

    public async Task<(bool Ok, string? Error, User? User)> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        username = (username ?? string.Empty).Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct).ConfigureAwait(false);
        if (user is null || !_hasher.Verify(password ?? string.Empty, user.PasswordHash))
            return (false, "Invalid username or password.", null);
        return (true, null, user);
    }
}
