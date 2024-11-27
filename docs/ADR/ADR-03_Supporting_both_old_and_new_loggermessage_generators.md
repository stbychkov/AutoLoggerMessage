### Title: ADR-03 Supporting Both Old and New LoggerMessage Generators
### Status: Accepted
### Context:

The `LoggerMessage` generators in the `Microsoft.Extensions.Logging.Abstractions` package have been around for a while,
but with the introduction of the new `Microsoft.Extensions.Telemetry.Abstractions` package, there's a shift toward a more
efficent approach to logging. Since it's unclear how many people are using the new logging capabilities, it seems reasonable to support both the old and the new generators to ensure broad compatibility.

Initially, I considered creating two separate versions of the source generator: one for the old logging package and one for the new one.
However, this would complicate the setup process for users. Instead, I decided to keep things simple and automatically detect which logging package is being used in the project.
By analyzing the project’s dependencies, I can determine whether the new logging capabilities are available and adjust the behavior of the source generator accordingly.
This approach works seamlessly and simplifies the setup for the user.

### Decision:

Instead of creating separate versions of the source generator for the old and new LoggerMessage generators, I will:

* Analyze the project dependencies during the source generation process.
* Detect whether the new Microsoft.Extensions.Telemetry.Abstractions package is available.
* Automatically adjust the behavior of the source generator based on the presence of the new package, falling back to the old Microsoft.Extensions.Logging.Abstractions generator if the new one isn’t found.

This way, users don’t need to worry about configuring which version of the generator they need — the system will automatically choose the right one based on their project setup.

### Consequences:

* **Short-term**: The source generator will analyze project dependencies, which adds a bit of logic to the setup process.
However, it greatly simplifies the user experience by removing the need for manual configuration.
* **Long-term**: Users will benefit from an automatic detection approach that ensures compatibility with both old and new logging generators, without requiring them to manually choose which version to use.
* **Risks**: There’s a small risk that the automatic detection might not work perfectly in all edge cases (for example, if dependencies are managed in unusual ways).
However, this is minimal, and the fallback to the old logging package ensures the system is still functional in those cases.
* **Maintenance**: If future versions of the logging packages introduce breaking changes or new features, the automatic detection logic might need to be updated.
But this is a manageable task that keeps the solution flexible without requiring multiple versions of the source generator.

### Alternatives Considered

* **Create Separate Versions of the Source Generator**: I could create two different versions of the generator — one for each logging package. However, this would complicate the setup and configuration for users, leading to a more cumbersome experience.
* **Require Manual Configuration by the User**: Another option was to require users to specify whether they were using the old or new logging package through configuration. However, this increases the setup complexity and can lead to errors, so it was rejected in favor of automatic detection.

