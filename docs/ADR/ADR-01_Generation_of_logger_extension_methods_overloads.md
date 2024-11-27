### Title: ADR-01 Generation of LoggerMessage extension methods overloads
### Status: Accepted
### Context:

In order to improve performance and avoid boxing of primitive types during logging, we need to generate generic overloads for logger methods.
The challenge here is making sure these overloads are applied everywhere across the solution automatically while maintaining control over when and how they are generated.

I initially considered generating these overloads during the PostOutputInitializationStep, but at that point, I don't have access to the configuration.
This would make it impossible to disable or selectively generate the overloads, and if I always generated them, it would result in too many methods being created for every assembly using the source generator.

I also can't generate the overloads during the source output generation stage because I need them to be available before the source generator begins processing the code.

Given these constraints, the best solution is to generate these overloads in advance and add them to the project as a public class.
By doing so, the overloads will be applied **everywhere** within the source generator assembly through parameter specificity, without generating unnecessary methods for each target assembly.

### Decision:

To avoid boxing primitive types and ensure proper overload application across the entire solution, I will:

* Generate the generic overloads in advance and place them in a public class within the project.
* This ensures that the overloads are globally accessible across the source generator assembly, applying automatically due to parameter specificity.
* The overloads will not be generated in every assembly, reducing unnecessary method generation.

By placing these overloads in a public class and generating them up front, I can guarantee that they are available before the source generator begins processing, without introducing excess methods in each assembly or having to manage complex configurations.

### Consequences:

* **Long-term**: This approach adds additional 144 extension methods that ensure the overloads will replace standard calls everywhere thanks to parameter specificity.
* **Risks**: 
  * Need to check if unused extension methods can be trimmed in this scenario for AOT.
  * To compile source generator, ILogger class has to be resolved, which require us to add this library as a reference, so there might be some problems with package version.

### Alternatives Considered

* **Generate overloads during PostOutputInitializationStep**: This would allow flexibility in enabling or disabling the overloads at a later stage.
  However, it’s not possible because I don’t have access to configuration during that step, meaning I can’t control whether to generate the overloads or not.

* **Generate overloads during Source Output Generation Stage**: This would only work if the overloads were available before the source generator starts processing.
  However, I need these overloads to be available upfront, making this approach impractical.

