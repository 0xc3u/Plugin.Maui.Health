<!-- 
Everything in here is of course optional. If you want to add/remove something, absolutely do so as you see fit.
This example README has some dummy APIs you'll need to replace and only acts as a placeholder for some inspiration that you can fill in with your own functionalities.
-->
![](nuget.png)
# Plugin.Maui.Health

`Plugin.Maui.Health` provides the ability to access personal health data in your .NET MAUI application.

## Install Plugin

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.Health.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.Health/)

Available on [NuGet](http://www.nuget.org/packages/Plugin.Maui.Health).

Install with the dotnet CLI: `dotnet add package Plugin.Maui.Health`, or through the NuGet Package Manager in Visual Studio.

## API Usage

`Plugin.Maui.Health` provides the `Health` class that has a single property `Property` that you can get or set.

You can either use it as a static class, e.g.: `Health.Default.Property = 1` or with dependency injection: `builder.Services.AddSingleton<IHealth>(Health.Default);`

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



#### Android

No permissions are needed for Android.

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

    public void StartHealth()
    {
        Health.ReadingChanged += (sender, reading) =>
        {
            Console.WriteLine(reading.Thing);
        };

        Health.Start();
    }
}
```

### Straight usage

Alternatively if you want to skip using the dependency injection approach you can use the `Health.Default` property.

```csharp
public class HealthViewModel
{
    public void StartHealth()
    {
        Health.ReadingChanged += (sender, reading) =>
        {
            Console.WriteLine(Health.Thing);
        };

        Health.Default.Start();
    }
}
```

### Health

Once you have created a `Health` you can interact with it in the following ways:

#### Events

##### `ReadingChanged`

Occurs when Health reading changes.

#### Properties

##### `IsSupported`

Gets a value indicating whether reading the Health is supported on this device.

##### `IsMonitoring`

Gets a value indicating whether the Health is actively being monitored.

#### Methods

##### `Start()`

Start monitoring for changes to the Health.

##### `Stop()`

Stop monitoring for changes to the Health.

# Acknowledgements

This project could not have came to be without these projects and people, thank you! <3
