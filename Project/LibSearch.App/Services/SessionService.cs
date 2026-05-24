using LibSearch.App.Models.Entities;

namespace LibSearch.App.Services;

public class SessionService : ISessionService
{
    public User? CurrentUser { get; private set; }

    public event EventHandler<User?>? UserChanged;

    public void SetUser(User user)
    {
        CurrentUser = user;
        UserChanged?.Invoke(this, user);
    }

    public void Clear()
    {
        CurrentUser = null;
        UserChanged?.Invoke(this, null);
    }
}
