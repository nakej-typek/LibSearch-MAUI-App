using LibSearch.App.Models.Entities;

namespace LibSearch.App.Services;

public interface IAuthService
{
    Task<(bool Ok, string? Error, User? User)> RegisterAsync(string username, string password, CancellationToken ct = default);
    Task<(bool Ok, string? Error, User? User)> LoginAsync(string username, string password, CancellationToken ct = default);
}
