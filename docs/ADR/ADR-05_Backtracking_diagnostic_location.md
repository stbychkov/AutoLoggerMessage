### Title: ADR-05 Backtracking diagnostic location
### Status: Accepted
### Context:

As part of the solution, I create virtual classes ([why?](./ADR-04_Generation_of_loggermessage_methods.md)) that only exist in the readonly snapshot of the compilation process to trigger the `LoggerMessage` source generators.
These virtual classes will generate diagnostic reports, but the diagnostics will be tied to a class that doesn’t actually exist in the source code.
This creates a situation where the diagnostics will be reported from the virtual class, making it difficult to trace errors back to the original Log* calls in the code, where the actual issue occurred.

To address this, I need to backtrack the diagnostic location and map it back to the original Log* calls. This involves:
* Catching all diagnostic errors from the source generator.
* Rewriting the diagnostic location to point to the original location of the Log* call rather than the temporary virtual class.

### Decision:

To backtrack the diagnostic location, I will:

* Map the location of the original Log* calls to the corresponding generated method.
* Add a 1-to-1 comment before each generated method that contains the mapped location of the original Log* call. This will serve as a reference for the location in case diagnostics need to be traced back.
* When a diagnostic error is triggered, I will use this mapped location to rewrite the diagnostic location to the closest method where the error occurred.
This ensures that the diagnostic reports are more useful and point to the actual source where the issue originated.
* If this method of backtracking fails, I will fall back on setting the diagnostic location to an empty location, which is also acceptable but less helpful.

### Consequences:

* **Short-term**: This approach requires adding comments to the generated methods, which introduces a bit of extra work. However, this ensures that the diagnostic reports are meaningful and point to the correct source location.
* **Long-term**: The mapped location comments allow for better tracing of errors to the correct source code, making the debugging process easier and more accurate for users.
* **Risks**: If the mapping doesn’t work as expected or the backtracking fails, it could result in diagnostics that are harder to interpret. The fallback of an empty location is acceptable, but not ideal. I'll need to ensure the backtracking logic is solid to avoid missing any errors.
* **Maintenance**: This solution should work for future updates of the source generator, as long as the core logic of generating LoggerMessage methods and triggering diagnostics remains the same. Adjustments may be necessary if the structure of diagnostics changes.

### Alternatives Considered

* **Ignore Diagnostic Location**: I could have simply ignored the location of diagnostics coming from the virtual classes and accepted the fact that the error reports would be disconnected from the original source code.
This would have been simpler but would result in less useful diagnostics, making debugging more difficult.
* **Create Real Classes Instead of Virtual Ones**: An alternative would be to create real classes instead of virtual ones, ensuring that the generated methods exist in the source code and diagnostics could be directly linked to them.
Here is [why?](./ADR-04_Generation_of_loggermessage_methods.md) I don't consider it.
