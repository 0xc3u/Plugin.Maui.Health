using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Models;

namespace Plugin.Maui.Health;

partial class HealthDataProviderImplementation : IHealth
{
	public bool IsSupported => throw new NotImplementedException();

	public Task<bool> CheckPermissionAsync(HealthParameter healthParameter, PermissionType permissionType)
	{
		throw new NotImplementedException();
	}

	public Task<List<Sample>> ReadAllAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<double?> ReadAverageAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<double> ReadCountAsync(HealthParameter healthParameter, DateTime from, DateTime until)
	{
		throw new NotImplementedException();
	}

	public Task<double?> ReadLatestAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<double?> ReadMaxAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<double?> ReadMinAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<bool> WriteAsync(HealthParameter healthParameter, DateTime? date, double valueToStore)
	{
		throw new NotImplementedException();
	}
}