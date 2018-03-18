# NHystrix

## What is NHystrix

NHystrix is inspired by the [Netflix/Hystrix project](https://github.com/Netflix/Hystrix) to provide similar
functionality and resilience to C#/.Net based services and clients. The [Hystrix Wiki](https://github.com/Netflix/Hystrix/wiki) 
is worth a read to understand what challenges N/Hystrix aims to solve.

The current release provides...
- Circuit Breaker Pattern (https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
- Bulkhead Pattern (https://docs.microsoft.com/en-us/azure/architecture/patterns/bulkhead)
- Timeouts

...functionality.

## What it is NOT

Due to language and feature differences between Java and C#/.Net, this is NOT a direct port. If you are
familiar with Hystrix, you'll recognize similarities in API and structure, but also see that NHystrix arrives at the
end goal a bit differently. For example, rather than the interfaces working with Reactive Extensions futures and promises, 
they support C# async/await.

## Quick Start

### Install the NuGet package to your project

Packages can be found at:

https://www.nuget.org/packages/NHystrix/

# [Powershell](#tab/powershell)

```
Install-Package NHystrix
```

# [CLI](#tab/dotnet-cli)

```
dotnet add package NHystrix
```

***

### Create a command class

``` cs
public class GreeterCommand : HystrixCommand<string>
{
    string greeter;

    public GreeterCommand(string greeter, HystrixCommandProperties properties)
        : base(new HystrixCommandKey("Test", new HystrixCommandGroup("TestGroup")), properties)
    {
        this.greeter = greeter;
    }

    protected override Task<string> RunAsync()
    {
        return Task.FromResult($"Hello, {greeter}!");
    }

    protected override string OnHandleFallback()
    {
        return "Hello World!";
    }
}
```

### Execute your command

``` cs
var properties = new HystrixCommandProperties()
{
    FallbackEnabled = true
};

var cmd = new GreeterCommand("Bob", properties);

string s = await cmd.ExecuteAsync()

```

Output:
```
Hello, Bob!
```
