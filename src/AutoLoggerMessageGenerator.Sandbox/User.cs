namespace AutoLoggerMessageGenerator.Sandbox;

public class User
{
    public string Username { get; private set; }
    public string Password { get; private set; }

    public User(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username)) 
            throw new ArgumentException("Username cannot be empty", nameof(username));
        
        if (password.Length < 6) 
            throw new ArgumentException("Password must be at least 6 characters long", nameof(password));

        Username = username;
        Password = password;
    }
}
