# AutoLoggerMessage

[![NuGet](https://img.shields.io/nuget/v/stbychkov.AutoLoggerMessage)](https://www.nuget.org/packages/stbychkov.AutoLoggerMessage/)
![Build](https://github.com/stbychkov/AutoLoggerMessage/actions/workflows/dotnet_build.yml/badge.svg)
[![License](https://img.shields.io/github/license/stbychkov/AutoLoggerMessage)](https://github.com/stbychkov/AutoLoggerMessage/blob/main/LICENSE)
[![NuGet Downloads](https://img.shields.io/nuget/dt/stbychkov.AutoLoggerMessage)](https://www.nuget.org/packages/stbychkov.AutoLoggerMessage/)
[![PRs Welcome](https://img.shields.io/badge/PR-Welcome-blue)](https://github.com/stbychkov/stbychkov.AutoLoggerMessage/pulls)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/stbychkov/AutoLoggerMessage)

---

Welcome to `AutoLoggerMessage`, a source generator that automatically creates [`LoggerMessage`](https://youtu.be/Otm8tH0Vrp0) methods, enabling high-performance logging.

## Demo

Click the image below to watch the demo video:

[![Demo](https://raw.githubusercontent.com/stbychkov/AutoLoggerMessage/c4ffa42eab74b75bac814a2beca374893f845a2f/src/AutoLoggerMessageGenerator/Assets/icon.jpg)](https://youtu.be/2MTbv6WtwjM?si=lGVfp3Sz8h2B1b-U)

## Getting started

#### Install the Package from [NuGet](https://www.nuget.org/packages/stbychkov.AutoLoggerMessage)

```shell
dotnet add package stbychkov.AutoLoggerMessage
```

Check [this](https://github.com/stbychkov/AutoLoggerMessage/wiki/Configuration) page for configuration options that can tweak the source generation process.

## Benchmarks

You can achieve performance boosts of up to 90% just by including this source generator in your project.

For `ILogger.Log*` methods:

| Configuration           | Mean      | Ratio | Allocated |
|-------------------------|-----------|-------|-----------|
| Default implementation  | 41.419 ns | 1.00  | 216 B     |
| Default + LoggerMessage | 4.004 ns  | 0.10  | -         |
| AutoLoggerMessage       | 4.577 ns  | 0.11  | -         |

And for `ILogger.DefineScope`:

| Configuration           | Mean      | Ratio | Allocated |
|-------------------------|-----------|-------|-----------|
| BeginScope              | 39.566 ns | 1.00  | 216 B     |
| DefineScope             | 5.197 ns  | 0.13  | -         |
| AutoLoggerMessage       | 5.296 ns  | 0.13  | -         |

Take a look at [benchmark](https://github.com/stbychkov/AutoLoggerMessage/wiki/Benchmarks) page for more details.

## Known Limitations

* It supports only static `EventId` parameter. If you pass the explicit `EventId` parameter, which basically no one does
  as far as I can tell, it generates a new `EventId` and the existing one will be passed to the formatter state, but it
  won't be logged.
  This limitation comes from the original `LoggerMessage` generator as they don't support the explicit parameter ~~
  yet~~.
* `Log.Define` supports only 6 message
  parameters ([src](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggermessage.define))
  so if you pass more than that, the default `Logger.Log(*, params object[] args)` will be executed.
* As this solution is based on interceptors, only .NET 8+ is supported
* Generic arguments [are not supported](https://github.com/dotnet/extensions/blob/ca2fe808b3d6c55817467f46ca58657456b4a928/docs/list-of-diagnostics.md?plain=1#L66C4-L66C13). If you pass a generic argument to the log function, the default `Logger.Log(*, params object[] args)` will be executed.

## Is something wrong?

If something is not working as expected, you can fall back on the default implementation of ILogger extensions.
To do this, call the extensions directly, for example:

```csharp
logger.LogInformation("Some message"); // instead of this
LoggerExtensions.LogInformation(logger, "Some message"); // use this
```

In such case, the source generator will bypass the log call, ensuring you get the expected behavior.

If you require functionality from the `LoggerMessage` source generator that is not supported by this library,
you can manually create your own source-generated version. Simply define your own partial class and partial method, annotated with the [LoggerMessage] attribute.

```csharp
logger.LogInformation("Some message"); // instead of this

[LoggerMessage(LogLevel = LogLevel.Information, Message = "Some message")]
public partial void LogSomeMessage(); // use this
```

But for both scenarios, it’s recommended to report the issue, as it shouldn't happen under normal circumstances.
Your feedback can help improve the library and address potential shortcomings. Thank you!

## Motivation

Source-generated logging is increasingly recognized as a modern and efficient approach. Check [this](https://github.com/stbychkov/AutoLoggerMessage/wiki/Evolution-of-Logging-Techniques) page to see why.

But let’s be real, when you’ve got a mid-sized project, migrating to the new logging approach is not exactly a simple
task. And even when starting a new project, marking every class as partial or depending on an external partial class for
logging messages can feel disconnected from the code where they are actually used.

This library handles most of the heavy lifting for you, so you can spend your time doing more important things!

But I hope this is a temporary solution — maybe one day, something similar will be added to the core library.
Check [this](https://github.com/dotnet/runtime/discussions/110364) discussion for updates.

## How It Works

This source generator searches for all `logger.Log*` methods in your code and, based on their parameters, automatically
creates partial methods for `LoggerMessage`. It also generates a set of interceptors to forward
the logging calls to the newly generated `LoggerMessage` methods.

For more details, see the [How It Works](https://github.com/stbychkov/AutoLoggerMessage/blob/main/docs/how-it-works.md)
documentation

## Questions?

I bet you have some questions about why things are set up the way they are.
Refer to the [ADR files](https://github.com/stbychkov/AutoLoggerMessage/tree/main/docs/ADR) for detailed explanations of the design decisions behind this package.
