### Title: ADR-04 Generation of `LoggerMessage` methods
### Status: Accepted
### Context:

The existing `LoggerMessage` generators rely on the attribute that needs to be applied to methods in the code to trigger the code generation.
However, this attribute-based approach is designed to work on code that will eventually be compiled into the output.
In my case, I need to trigger the code generator to create the necessary `LoggerMessage` methods, but I don’t want to include this code in the final output.

After evaluating the options, I decided to create a virtual temporary class that exists only in the readonly snapshot of the compilation process.
This class is not included in the final output, but it will be enough to trigger the existing `LoggerMessage` source generator and ensure the necessary methods are generated.

The process involves automatically transforming the log calls in the code into virtual methods with the `LoggerMessage` attribute.
These virtual methods will exist solely to trigger the code generation.
The one challenge with this approach is that the existing source generator generates methods as partial.
Since the temporary virtual class doesn't exist in the output, the generated partial declaration would be invalid.
To fix this, I will need to adjust the code during the post-processing stage by removing the partial keyword from the final result.

### Decision:

To trigger the `LoggerMessage` source generator without modifying the existing generators:

* I will create a virtual temporary class that only exists in the readonly snapshot of the compilation process.
* This temporary class will contain virtual methods decorated with the `LoggerMessage` attribute, which will be enough to trigger the source generator.

Since the `LoggerMessage` generator creates methods as partial, and the temporary class won’t be output, I will remove the partial keyword in the post-processing stage to ensure valid method declarations.

### Consequences:

* **Short-term**: This approach requires the introduction of a virtual class that will only exist during compilation.
The post-processing stage will also be responsible for removing the partial keyword, which introduces some additional complexity to the process.
* **Long-term**: The benefit of this approach is that it allows me to trigger the `LoggerMessage` generator without adding unnecessary code to the final output.
This also maintains compatibility with the existing logging system without requiring major changes.
* **Risks**: The main risk is that the post-processing step to remove the partial keyword introduces an extra stage that could introduce bugs if not handled carefully.
However, this risk is manageable and should be relatively easy to fix in case of issues.
* **Maintenance**: This approach allows for future updates to the source generator while keeping the solution flexible.
The only maintenance required would be to ensure the post-processing step works correctly if the underlying generator is updated.

### Alternatives Considered

* **Adjust the Existing LoggerMessage Generators**: I could have modified the existing `LoggerMessage` generators to accommodate my needs.
  [Here](./ADR-01_Reusing_the_existing_loggermessage_generators.md) is why I don't consider it.
