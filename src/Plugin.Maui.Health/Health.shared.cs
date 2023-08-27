namespace Plugin.Maui.Health;

public static class HealthDataProvider
{
	static IHealth? defaultImplementation;

	/// <summary>
	/// Provides the default implementation for static usage of this API.
	/// </summary>
	public static IHealth Default =>
		defaultImplementation ??= new HealthDataProviderImplementation();

	internal static void SetDefault(IHealth? implementation) =>
		defaultImplementation = implementation;
}
