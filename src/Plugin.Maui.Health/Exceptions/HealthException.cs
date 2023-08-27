namespace Plugin.Maui.Health.Exceptions;
public class HealthException : Exception
{
	public HealthException() : base()
	{
	}

	public HealthException(string? message) : base(message)
	{
	}

	public HealthException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}
