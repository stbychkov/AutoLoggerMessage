### Title: ADR-07 Generation of `BeginScope` methods
### Status: Accepted

### Context:

The existing `ILogger.BeginScope` method has the same problems as the `ILogger.Log*` methods:
1. Lack of compile-time checks for mismatches between template parameters and the actual arguments, which can lead to runtime exceptions.
2. Performance overhead due to the need to create a new scope object every time.
3. Unnecessary allocations due to boxing of template parameters, even with identical arguments.

`ILogger.BeginScope` is backed by `LoggerMessage.DefineScope`, which allows generation of strongly-typed, precompiled delegates with up to 6 parameters. This enables high-performance and allocation-free scope creation, similar to how `LoggerMessage.Define` is used for logging methods.

The main difference is that `ILogger.BeginScope` **with only one parameter** is an instance method, not an extension method. This means we cannot intercept or replace calls that only use a single message argument (i.e., `BeginScope("Starting operation")`) because the instance method takes precedence over extension methods. Therefore, generation will be limited to `BeginScope` calls with one or more structured parameters (i.e., key-value pairs or anonymous objects), which typically benefit the most from strongly-typed scope generation.
[Reference test](https://github.com/stbychkov/AutoLoggerMessage/blob/main/tests/AutoLoggerMessageGenerator.UnitTests/MethodSpecificityRules/InstanceCallVsExtensionCallTests.cs)

### Decision:

Extend the AutoLoggerMessage source generator to support generation of strongly-typed scope delegates using `LoggerMessage.DefineScope`.

Specifically:
- Identify all usages of `ILogger.BeginScope` in the codebase where the call includes **at least one structured argument** (excluding pure string messages).
- For each identified scope usage:
    - Generate a static readonly field that contains the compiled scope delegate using `LoggerMessage.DefineScope`.
    - Generate an extension method (or internal interceptor method) that redirects the original `BeginScope` call to the generated delegate, preserving structure and performance.
- Ensure that the generated methods follow the same naming, visibility, and partial class strategy as existing `Log*` method interceptors.

### Consequences:

* **Short-term**:
    - Improves performance and reduces allocations for scoped logging with parameters.
    - Introduces new source generation complexity; testing must be extended to validate generated scopes.

* **Long-term**:
    - Moves the library closer to complete compile-time safety for all common logging patterns (`Log*` and `BeginScope`).

* **Risks**:
    - May cause confusion if developers attempt to use `BeginScope(string message)` expecting interception (which is not supported).
    - Reliance on exact call shapes (number and types of arguments) may introduce fragility unless thoroughly tested.

* **Maintenance**:
    - Must track and test against future changes in `LoggerMessage.DefineScope` API (currently supports up to 6 parameters).
    - Increases the surface area of generated code, potentially impacting future refactors or compatibility with downstream tools.

### Alternatives Considered

* **Do nothing**: Keep relying on `ILogger.BeginScope` as is. This maintains simplicity but misses out on performance and compile-time safety.
