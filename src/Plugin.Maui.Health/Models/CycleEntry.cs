using Plugin.Maui.Health.Enums;

namespace Plugin.Maui.Health.Models;

/// <summary>
/// A single menstrual-cycle / reproductive-health entry. The relevant value field depends on
/// <see cref="Type"/>: <see cref="Flow"/> for <c>MenstruationFlow</c>, <see cref="OvulationResult"/> for
/// <c>OvulationTest</c>, <see cref="Protection"/> for <c>SexualActivity</c>;
/// <c>IntermenstrualBleeding</c> is a bare occurrence at <see cref="Time"/>.
/// </summary>
public sealed record CycleEntry
{
	public DateTimeOffset Time { get; init; }
	public CycleEventType Type { get; init; }

	public MenstruationFlow Flow { get; init; }
	public OvulationTestResult OvulationResult { get; init; }
	public SexualActivityProtection Protection { get; init; }

	public string? Source { get; init; }
}
