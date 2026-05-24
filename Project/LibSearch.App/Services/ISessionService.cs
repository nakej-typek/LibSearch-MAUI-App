using LibSearch.App.Models.Entities;

namespace LibSearch.App.Services;

public interface ISessionService
{
    User? CurrentUser { get; }
    event EventHandler<User?>? UserChanged;
    void SetUser(User user);
    void Clear();
}
