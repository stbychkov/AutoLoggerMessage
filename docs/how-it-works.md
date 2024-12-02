# Internals: How the Source Generator Works

## Overview

The source generator automatically creates `LoggerMessage` methods for high-performance logging.
This process is designed to simplify logging and ensure that logging code doesn't require manual maintenance or updates.
The generator builds upon the existing `LoggerMessage` generators and adds functionality to support both old and new logging packages (`Microsoft.Extensions.Logging.Abstractions` and `Microsoft.Extensions.Telemetry.Abstractions`).

### Step 0: Create a set of generic logger overloads (up to 6 parameters)

The first step involves generating **196** generic extension methods for `ILogger` class that will override the default logging methods ([why?](./ADR/ADR-01_Generation_of_logger_extension_methods_overloads.md)).
`LoggerMessage.Define` supports up to 6 parameters, so we can limit the methods only with this amount.

### Step 1: Find all Log* methods belonging to ILogger class

The generator looks for any `Log+` method (like `LogInformation`, `Log`, etc.) and captures the parameters passed to these methods.

### Step 2: Generate a virtual partial class with partial LoggerMessage methods

After identifying the relevant logging methods, the generator creates a virtual class that will exist only in the readonly snapshot of the compilation process.
This class contains partial LoggerMessage methods, which are virtual but will not be included in the final output.
The virtual class serves solely to trigger the `LoggerMessage` source generator, which will generate the actual methods used for logging.
At this point, the generator doesn't produce the methods directly but ensures that the code structure is in place to trigger the existing `LoggerMessage` generators.

### Step 3: Use existing LoggerMessage generator to generate the rest

The LoggerMessage source generator from `Microsoft.Extensions.Logging.Abstractions` or `Microsoft.Extensions.Telemetry.Abstractions` will now work its magic.
It takes the parameters from the virtual methods and generates the actual `LoggerMessage` methods that the application will use for logging.
This works similarly to how `LoggerMessage` is typically generated, but we are leveraging a virtual class that exists only during compilation to trigger it.

### Step 4: Post-process the result with minor modifications

Once the source generator produces the methods, the process enters a post-processing phase. Here, we make necessary adjustments:

* Remove the partial keyword: Since the virtual class doesn't exist in the output, the generator would have attempted to declare methods as partial.
In the post-processing step, we remove the partial keyword from these methods to ensure that the generated code is valid and compilable.
* Backtrack diagnostic locations: During code generation, diagnostics are often tied to the temporary virtual class.
In this phase, we backtrack any diagnostics to the original Log* calls by mapping the generated method to the location in the original source code.
If this mapping fails, we fall back to an empty location.

### Step 5: Creating interceptors

After the LoggerMessage methods are successfully generated, we proceed to create a set of interceptors.
These interceptors act as a bridge between the logging calls and the generated LoggerMessage methods.
The interceptors forward the logging requests to the correct generated methods, ensuring the logging system operates smoothly.
