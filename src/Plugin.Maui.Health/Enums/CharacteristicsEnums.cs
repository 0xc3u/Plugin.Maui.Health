namespace Plugin.Maui.Health.Enums;

/// <summary>Biological sex, mirroring HealthKit's <c>HKBiologicalSex</c>.</summary>
public enum BiologicalSex
{
	NotSet,
	Female,
	Male,
	Other,
}

/// <summary>Blood type, mirroring HealthKit's <c>HKBloodType</c>.</summary>
public enum BloodType
{
	NotSet,
	APositive,
	ANegative,
	BPositive,
	BNegative,
	ABPositive,
	ABNegative,
	OPositive,
	ONegative,
}

/// <summary>Fitzpatrick skin type (I–VI), mirroring HealthKit's <c>HKFitzpatrickSkinType</c>.</summary>
public enum FitzpatrickSkinType
{
	NotSet,
	I,
	II,
	III,
	IV,
	V,
	VI,
}

/// <summary>Wheelchair use, mirroring HealthKit's <c>HKWheelchairUse</c>.</summary>
public enum WheelchairUse
{
	NotSet,
	No,
	Yes,
}
