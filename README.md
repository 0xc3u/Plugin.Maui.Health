![](nuget.png)
# Plugin.Maui.Health

`Plugin.Maui.Health` provides the ability to access personal health data in your .NET MAUI application.


## Build Status
![ci](https://github.com/0xc3u/Plugin.Maui.Health/actions/workflows/ci.yml/badge.svg)


## Install Plugin

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.Health.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.Health/)

Available on [NuGet](http://www.nuget.org/packages/Plugin.Maui.Health).

Install with the dotnet CLI: `dotnet add package Plugin.Maui.Health`, or through the NuGet Package Manager in Visual Studio.

## API Usage

`Plugin.Maui.Health` provides the `Health` class that has a single property `Property` that you can get or set.

You can either use it as a static class, e.g.: `Health.Default.Property = 1` or with dependency injection: `builder.Services.AddSingleton<IHealth>(Health.Default);`


### Platform supported

| Platform | Minimum Version Supported |
|----------|--------------------------|
| iOS      | 14.0+                     |
| Android  | currently not supported        |



### Permissions

Before you can start using Health, you will need to request the proper permissions on each platform.

#### iOS

You need to add permissions in your Info.plist file to read/write to the HealthKit store.

```xml
<key>NSHealthShareUsageDescription</key>
<string>We need access to your health data to read your steps and other metrics.</string>
<key>NSHealthUpdateUsageDescription</key>
<string>We need access to write your steps and other metrics.</string>
````

### Dependency Injection

You will first need to register the `Health` with the `MauiAppBuilder` following the same pattern that the .NET MAUI Essentials libraries follow.

```csharp
builder.Services.AddSingleton(Health.Default);
```

You can then enable your classes to depend on `IHealth` as per the following example.

```csharp
public class HealthViewModel
{
    readonly IHealth Health;

    public HealthViewModel(IHealth Health)
    {
        this.Health = Health;
    }
}
```

### Straight usage

Alternatively if you want to skip using the dependency injection approach you can use the `Health.Default` property.

```csharp
public class HealthViewModel
{
    public async Task<double> ReadStepsCountAsync()
    {
        	var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.StepCount, Health.Enums.PermissionType.Read | Health.Enums.PermissionType.Write);
			if (hasPermission)
			{
				return await health.ReadCountAsync(Enums.HealthParameter.StepCount, DateTime.Now.AddDays(-1), DateTime.Now);
			}
    }
}
```

### Health

Once you have created a `Health` you can interact with it in the following ways:

#### Properties

##### `IsSupported`

Gets a value indicating whether reading the Health is supported on this device.


#### Methods

##### `CheckPermissionAsync`
Asynchronously checks and requests the specified permissions for a given health parameter.
```csharp
	var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.StepCount, Health.Enums.PermissionType.Read | Health.Enums.PermissionType.Write);
```

##### `ReadCountAsync`
Asynchronously reads the cumulative count of a specified "HealthParameter" within a given date range.
```csharp
	var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.StepCount, Health.Enums.PermissionType.Write);
	if (hasPermission)
	{
		var stepsCount = await health.ReadCountAsync(Enums.HealthParameter.StepCount, DateTime.Now.AddDays(-1), DateTime.Now);
	}
```

##### `WriteAsync`
Asynchronously writes a count-based health value to the HealthKit store.
```csharp
	var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.StepCount, Health.Enums.PermissionType.Write);
	if (hasPermission)
	{
		await health.WriteAsync(Health.Enums.HealthParameter.StepCount, DateTime.Now, 250);
	}
```


# Acknowledgements

This project could not have came to be without these projects and people, thank you! <3
