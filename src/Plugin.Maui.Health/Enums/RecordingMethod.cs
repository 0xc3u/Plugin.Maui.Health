namespace Plugin.Maui.Health.Enums;

/// <summary>How a sample was recorded, normalised across HealthKit and Health Connect.</summary>
public enum RecordingMethod
{
	/// <summary>Not reported by the platform.</summary>
	Unknown,

	/// <summary>The user entered the value by hand.</summary>
	Manual,

	/// <summary>Recorded automatically by a device/app in the background.</summary>
	Automatic,

	/// <summary>Actively recorded during a tracked session (e.g. a workout).</summary>
	ActivelyRecorded,
}
