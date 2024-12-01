namespace AutoLoggerMessageGenerator.Sandbox;

public interface IUserRepository
{
    Task<User> GetByUsernameAsync(string username);
    Task AddAsync(User user);
}
