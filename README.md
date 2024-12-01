# AutoLoggerMessage

Welcome to AutoLoggerMessage, the lazy (but efficient) source generator that automatically creates `LoggerMessage`
methods for high-performance logging. Why did I make this? Well, because I'm lazy and don't want to write these methods
manually every time. I’m hoping this is a temporary solution—maybe one day, something similar will be added out of the
box in the framework. But for now, here we are.

## Why?

You’ve probably heard why using the source-generated logger is the cool new thing (if not, here’s
a [link](https://youtu.be/Otm8tH0Vrp0)).
But let’s be real, when you’ve got a medium-sized project, migrating to the new logging approach is not exactly a simple
task.
And even if you’re starting a fresh project, it can be a little weird to mark every class as partial or rely on some
external partial class where all your logging messages live, far away from the code where you actually use them.

That’s where this library comes in. It’s like a magic wand that automatically migrates all your old logging calls to the
shiny new source-generated approach with zero changes to your code.
Seriously, no need to go hunting down each log call and refactor it yourself. This library handles most of the heavy
lifting for you, so you can spend your time doing more important things!

## Usage

Just follow these two simple steps:

#### 1. Install the Package from NuGet

```shell
dotnet add package <TODO>
```

#### 2. Configure Supporting Interceptors in Your Project

Now that the package is installed, we need to tell your project to use the interceptors.
You’ve got two options here: apply them globally or just for a specific project. Here’s how:

It’s simple. Just install the package, and the source generator will do its magic! No need for you to manually add
LoggerMessage methods or worry about performance.

```xml
<InterceptorsPreviewNamespaces>$(InterceptorsPreviewNamespaces);Microsoft.Extensions.Logging.AutoLoggerMesssage</InterceptorsPreviewNamespaces>
```

## How It Works

This source generator hunts down all logger.Log* methods in your code and, based on their parameters, automatically
creates partial methods for LoggerMessage. But wait, there's more! It also generates a set of interceptors to forward
the logging calls to the newly generated LoggerMessage methods.

LoggerMessage Interceptors
These interceptors work with both old and new versions of the source generator:

The old version comes from the Microsoft.Extensions.Logging.Abstractions package.
The new version is from the Microsoft.Extensions.Telemetry.Abstractions package.
The source generator detects which version is available in your project and adjusts the generated code to offer extra
features when the telemetry package is present. So, you get the best of both worlds (old and new)!

## Configuration

The source generator has a few configurable properties that control its behavior.
These are passed as build properties and tweak the generated code in various ways.

| Property Name                                                             | Description                                                                                                                                                            | DefaultValue |
|---------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| `build_property.AutoLoggerMessageGenerator.GenerateInterceptorAttribute`  | Defines if the interceptor attribute should be generated or not. Simple as that.                                                                                       | true         |
| `build_property.AutoLoggerMessageGenerator.GenerateSkipEnabledCheck`      | Sets `LogProperties.SkipEnabledCheck` to true. This skips the check for whether logging is enabled before calling the log method.                                      | true         |
| `build_property.AutoLoggerMessageGenerator.GenerateOmitReferenceName`     | Sets `LogProperties.OmitReferenceName` to true. This indicates whether to prefix the name of the parameter or property to the generated name of each tag being logged. | false        |
| `build_property.AutoLoggerMessageGenerator.GenerateSkipNullProperties`    | Sets `LogProperties.SkipNullProperties` to true. This ensures that null properties are skipped in the log entries.                                                     | false        |
| `build_property.AutoLoggerMessageGenerator.GenerateTransitive`            | Sets `LogProperties.Transitive` to true. This indicates that each property of any complex objects are expanded individually.                                           | false        |

## If You’re Wondering Why

I bet you have some questions about why things are set up the way they are. Feel free to check out
the [ADR files](./docs/ADR) for more detailed explanations on the decisions behind this package.

## Is This Permanent?

Nope. This package is free to use, but I hope it’s just a temporary hack. It works now, and that’s all that matters!

If you’ve got more questions or comments, hit me up. I’m probably napping or avoiding manual coding—so don’t expect a
super fast response, but I’ll get back to you eventually.
