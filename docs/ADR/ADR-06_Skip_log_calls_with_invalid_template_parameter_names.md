### Title: ADR-06 Skip log calls with invalid template parameter names
### Status: Accepted
### Context:

Template message parameters are parsed to generate logging code dynamically.
The template parameters' names are critical for synchronizing the log message format with the corresponding parameters in the code.
If a parameter name is invalid, the generated code may produce unexpected results or fail to align with the user's intended behavior.

### Decision:

I will not process log calls if the template message parameters contain invalid names.
Instead, I will filter these calls and report a diagnostic warning indicating the invalid parameter name.

### Consequences:

* **Positive**:
  * Users will be notified of issues in their template messages.
  * The generator remains simple and reliable, reducing potential bugs and unexpected behavior.

* **Negative**:
  * Users must fix their invalid parameter names before the log calls are processed. This might require additional effort upfront but is offset by the long-term benefits of correctness.

### Alternatives Considered

Automatically rename invalid parameter names to conform to a valid format (e.g., replacing spaces with underscores).

**Rejected Because**:
* It introduces unexpected behavior for the user.
* It adds some complexity to the generator and increases the risk of synchronization (template parameters <-> template message) errors.
