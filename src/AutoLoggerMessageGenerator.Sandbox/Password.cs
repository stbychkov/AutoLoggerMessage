namespace AutoLoggerMessageGenerator.Sandbox;

public class Password
{
    public string Value { get; private set; }

    public Password(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long.");

        Value = value;
    }
}
