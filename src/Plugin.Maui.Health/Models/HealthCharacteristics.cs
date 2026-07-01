using Plugin.Maui.Health.Enums;

namespace Plugin.Maui.Health.Models;

/// <summary>
/// The user's static profile characteristics (read-only). iOS-only — populated from HealthKit's
/// characteristic types; Android Health Connect has no equivalent.
/// </summary>
public sealed record HealthCharacteristics
{
	public DateTimeOffset? DateOfBirth { get; init; }
	public BiologicalSex BiologicalSex { get; init; }
	public BloodType BloodType { get; init; }
	public FitzpatrickSkinType SkinType { get; init; }
	public WheelchairUse WheelchairUse { get; init; }
}
