namespace AutoLoggerMessageGenerator.Sandbox;

public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, User> _users = new();

    public Task<User> GetByUsernameAsync(string username)
    {
        _users.TryGetValue(username, out var user);
        return Task.FromResult(user);
    }

    public Task AddAsync(User user)
    {
        _users[user.Username] = user;
        return Task.CompletedTask;
    }
}
