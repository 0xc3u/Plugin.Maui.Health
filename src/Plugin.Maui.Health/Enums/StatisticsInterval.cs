namespace Plugin.Maui.Health.Enums;

/// <summary>The bucket size for <c>ReadStatisticsAsync</c>. Buckets align to local-time boundaries.</summary>
public enum StatisticsInterval
{
	Hourly,
	Daily,
	Weekly,
	Monthly,
}
