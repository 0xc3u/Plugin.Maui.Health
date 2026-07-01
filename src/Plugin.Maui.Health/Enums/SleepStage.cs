namespace Plugin.Maui.Health.Enums;

/// <summary>A stage within a sleep session, normalised across HealthKit and Health Connect.</summary>
public enum SleepStage
{
	Unknown,
	Awake,
	InBed,
	Sleeping,
	Light,
	Deep,
	Rem,
}
