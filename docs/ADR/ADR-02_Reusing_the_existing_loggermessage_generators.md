### Title: ADR-02 Reusing the Existing LoggerMessage Generators
### Status: Accepted
### Context:

The goal here is to automatically generate `LoggerMessage` methods for efficient logging, but without reinventing the wheel.
I would love to chain source generators, but here is a catch: they cannot be chained.
> Run un-ordered, each generator will see the same input compilation, with no access to files created by other source generators.

This means I can't just hook my generator into the existing one seamlessly.
Also I need to have access to internal classes and some parts of the original generator that aren't exposed publicly.
So, in order to make everything work, I had to copy the generator code into my solution and leave it untouched. This way, I can take advantage of its functionality but without the risk of breaking anything when updates happen.

While this approach isn't the most efficient or ideal, it allows for easier maintenance and future updates since I won't have to worry about keeping track of changes in the original source code.

### Decision:

Instead of modifying the existing `LoggerMessage` generator, I copied the generator into my solution and left it unchanged. This decision allows me to:

* Reuse the existing functionality without modifying the original code, ensuring future updates are easy to integrate.
* Work around the limitation that source generators can't be chained by duplicating the generator code into my own solution.
* Ensure that any updates to the original `LoggerMessage` generator can be applied by simply replacing or merging the copied code, without worrying about custom modifications.

While it’s a bit of extra work up front, this approach ensures that the source generator can evolve with future updates, without the need for extensive rework.

### Consequences:

* **Short-term**: I have to duplicate the existing source generator code, which is less efficient than reusing it directly.
* **Long-term**: The solution is more maintainable because it avoids direct modifications to the original `LoggerMessage` generator.
  Any updates to the generator can be applied by simply replacing the copied code with newer versions, making future updates less painful.
* **Risks**: If the existing generator undergoes major changes, I may have to adapt my solution to fit.
  But since I’m using the original code as-is, the risk of breaking changes is minimized.

### Alternatives Considered

Create and modify my own `LoggerMessage` generator. It gives me full control which is good, but I'm lazy for maintaining that solution, so building on top of that sounds more easier.
