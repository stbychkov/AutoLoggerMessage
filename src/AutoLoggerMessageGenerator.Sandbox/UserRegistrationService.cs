using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.Sandbox;

public partial class UserRegistrationService(
    UserRegistrationDomainService domainService,
    ILogger<UserRegistrationService> logger)
{
    public async Task RegisterUserAsync(string username, string password)
    {
        try
        {
            logger.LogInformation("Registering user with username: {Username}", username);
            await domainService.RegisterAsync(username, password);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An unexpected error occurred during user registration for username: {Username}", username);
            throw; 
        }
    }
}
