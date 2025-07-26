### Title: ADR-08 Message Template Syntax Highlighting

### Status: Accepted

### Context:

When writing log messages using structured templates (like `"User {UserId} logged in"`), it's good to have some syntax highlighting to see if it's a raw string literal or a template parameter. IDE support could help a lot here by offering syntax highlighting out of the box.

There are two main options we looked at:

1. **`[SyntaxAttribute]` from Microsoft** — this is supported in Visual Studio and JetBrains Rider.
2. **`[StructuredMessageTemplate]` from `JetBrains.Annotations`** — works only in JetBrains tools, like Rider and ReSharper.

`SyntaxAttribute` seemed promising at first, but we hit a few issues:

* It’s only available starting from **.NET 7**, and even though our target app is .NET 8+ (because we're using interceptors), source generators themselves still need to be compiled against `netstandard2.0` for Visual Studio compatibility. So, we’d have to wrap every usage in `#if NET8_0_OR_GREATER`, which would make the code messy.
* The only `SyntaxAttribute` that somewhat matches our case is `SyntaxAttribute(SyntaxKind.CompositeFormat)`, which was designed for `string.Format`-style messages. That doesn't map well to structured logging templates, which can use named placeholders like `{UserId}` — not just `{0}`, `{1}`, etc.

Then we looked at `StructuredMessageTemplate` from `JetBrains.Annotations`. It's not built into the .NET SDK, but:

* It’s a tiny library with **no extra dependencies**.
* It targets **.NET Standard 1.0**, so it works fine with our source generator build setup.

The downside is that it’s **only useful in Rider/ReSharper** — Visual Studio (Code) won’t recognize it. But that’s fine: it’s better than nothing, and we shouldn't introduce any breakage.

### Decision:

We’re going to add `[StructuredMessageTemplate]` from the `JetBrains.Annotations` package to mark message template parameters in the generated logging code.

Specifically:

* Add the `[StructuredMessageTemplate]` attribute to all generated `message` parameters in `Log*` and `BeginScope` generic overloads.
* Pull in the latest version of `JetBrains.Annotations` as a compile-time-only dependency.

### Consequences:

* **Short-term**:
    * Improved developer experience in Rider/ReSharper.
    * Helps catch common mistakes in message templates at design time.

* **Long-term**:
    * No negative impact for Visual Studio users, but they won’t benefit from it either.
    * Keeps the generic overloads clean and tidy.

* **Risks**:
    * Adds a soft dependency (though very minimal and safe).

* **Maintenance**:
    * Easy to keep up — JetBrains.Annotations is stable and tiny.
    * If Microsoft adds a better-suited `SyntaxAttribute` in future versions, we can revisit this.

### Alternatives Considered:

* **Use `SyntaxAttribute(SyntaxKind.CompositeFormat)`**:
  Clean and native, but doesn’t fit well in our use case at this moment.
* **Do nothing**:
  Simple, but leaves developers without any IDE help for message templates, which is a missed opportunity.
