﻿using Plugin.Maui.Health.Enums;

namespace Plugin.Maui.Health;

public interface IHealth
{
	bool IsHealthDataSupported { get; }
	Task<bool> CheckPermissionAsync(HealthParameter healthParameter, PermissionType permissionType);
	Task<double> ReadCountAsync(HealthParameter healthParameter, DateTime from, DateTime until);
	Task<double?> ReadLatestAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit);
	Task<double> ReadAverageAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit);
	Task<double> ReadMinAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit);
	Task<double> ReadMaxAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit);
	Task<List<double>> ReadAllAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit);
	Task<bool> WriteAsync(HealthParameter healthParameter, DateTime? date, double valueToStore);
}