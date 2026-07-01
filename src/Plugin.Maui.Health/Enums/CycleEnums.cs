namespace Plugin.Maui.Health.Enums;

/// <summary>The kind of menstrual-cycle event carried by a <c>CycleEntry</c>.</summary>
public enum CycleEventType
{
	MenstruationFlow,
	OvulationTest,
	SexualActivity,
	IntermenstrualBleeding,
}

/// <summary>Menstrual flow level, normalised across HealthKit and Health Connect.</summary>
public enum MenstruationFlow
{
	Unspecified,
	None,
	Light,
	Medium,
	Heavy,
}

/// <summary>Ovulation test result, normalised across HealthKit and Health Connect.</summary>
public enum OvulationTestResult
{
	Unspecified,
	Negative,
	Positive,
	High,
}

/// <summary>Whether protection was used during sexual activity.</summary>
public enum SexualActivityProtection
{
	Unspecified,
	Protected,
	Unprotected,
}
