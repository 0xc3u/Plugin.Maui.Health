using System.Runtime.CompilerServices;

namespace Plugin.Maui.Health.Exceptions;

/// <summary>
/// The single exception type surfaced by every <see cref="IHealth"/> operation. Native platform
/// failures (HealthKit / Health Connect) are translated into this type so consuming MAUI code can
/// catch one exception and still get the original platform error via <see cref="Exception.InnerException"/>.
/// </summary>
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

	/// <summary>The plugin operation that failed (e.g. <c>ReadAllAsync</c>).</summary>
	public string? Operation { get; init; }

	/// <summary>The platform the error originated on (<c>Android</c> or <c>iOS</c>).</summary>
	public string? Platform { get; init; }

	/// <summary>The health parameter involved, when applicable.</summary>
	public string? Parameter { get; init; }

	/// <summary>
	/// Wraps a native/platform exception into a <see cref="HealthException"/> carrying the operation,
	/// platform and (optionally) parameter context, with a human-readable message. Already-meaningful
	/// <see cref="HealthException"/> instances are returned unchanged so context isn't lost or doubled.
	/// </summary>
	internal static HealthException Wrap(Exception inner, string platform, string? parameter = null,
		[CallerMemberName] string operation = "")
	{
		if (inner is HealthException existing)
			return existing;

		var detail = string.IsNullOrWhiteSpace(inner.Message) ? inner.GetType().Name : inner.Message;
		var parameterText = string.IsNullOrEmpty(parameter) ? string.Empty : $" [{parameter}]";
		var message = $"Plugin.Maui.Health: {operation}{parameterText} failed on {platform}: {detail}";

		return new HealthException(message, inner)
		{
			Operation = operation,
			Platform = platform,
			Parameter = parameter,
		};
	}
}
