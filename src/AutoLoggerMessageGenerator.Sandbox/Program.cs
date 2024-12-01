using AutoLoggerMessageGenerator.Sandbox;
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<UserRegistrationService>();

var userRepository = new InMemoryUserRepository();

var userRegistrationDomainService = new UserRegistrationDomainService(userRepository, logger);
var userRegistrationService = new UserRegistrationService(userRegistrationDomainService, logger);

await userRegistrationService.RegisterUserAsync("newUser", "password123");
await userRegistrationService.RegisterUserAsync("takenUser", "password123");
await userRegistrationService.RegisterUserAsync("newUser2", "123");
