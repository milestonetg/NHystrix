# NHystrix

## What is NHystrix

NHystrix is inspired by the [Netflix/Hystrix project](https://github.com/Netflix/Hystrix) to provide similar
functionality and resilience to C#/.Net based services and clients. The [Hystrix Wiki](https://github.com/Netflix/Hystrix/wiki) 
is worth a read to understand what challenges N/Hystrix aims to solve.

## What it is NOT

Due to language differences and feature differences between Java and C#/.Net, this is NOT a direct port. If you are
familiar with Hystrix, you'll recognize similarities in API and structure, but also see that NHystrix arrives at the
end goal a bit differently.

## Quick Start

### Install the Nuget package to your project:

# [Powershell](#tab/tabid-1)

```
Install-Package NHystrix
```

# [CLI](#tab/tabid-2)

```
dotnet install NHystrix
```

***

### Create a command class:

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

### Execute your command:

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
