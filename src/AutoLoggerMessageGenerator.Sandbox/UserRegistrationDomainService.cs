using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.Sandbox;

public class UserRegistrationDomainService(IUserRepository userRepository, ILogger logger)
{
    public async Task RegisterAsync(string username, string password)
    {
        logger.LogInformation("Starting registration for username: {Username}", username);

        var existingUser = await userRepository.GetByUsernameAsync(username);
        if (existingUser != null)
        {
            logger.LogWarning("Username {Username} is already taken.", username);
            return;
        }

        User newUser;
        try
        {
            newUser = new User(username, password);
            logger.LogInformation("User object created for username: {Username}", username);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create user object for username: {Username}", username);
            return;
        }

        await userRepository.AddAsync(newUser);
        logger.LogInformation("User successfully registered with username: {Username}", username);
    }
}
