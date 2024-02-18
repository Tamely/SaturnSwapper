# C# Coding Style

The general rule we follow is "use Visual Studio defaults".

1. We use [Allman style](https://en.wikipedia.org/wiki/Allman_style) braces, where each brace begins on a new line. A single line statement block can go without braces but the block must be properly indented on its own line and must not be nested in other statement blocks that use braces (See rule 18 for more details).
2. We use 1 tab for indentation.
3. We use `_camelCase` for internal and private fields and use `readonly` where possible. Prefix internal and private instances with `_`. When used on static fields, `readonly` should come after `static` (e.g. `static readonly` not `readonly static`). Public fields should be used sparingly and should be using PascalCasing with no prefix when used.
4. We avoid `this` with very few exceptions (only time it is used is in cosmetic generation when passing the SwapperService).
5. We always specify the visibility, even if it's the default. (e.g. `private string _foo` not `string _foo`). This should also always be the first modifier (e.g. `public abstract` not `abstract public`).
6. Namespace imports should be specified at teh top of the file _outside_ of the `namespace` declarations, and should be sorted alphabetically, with the exception of `System` and `Microsoft` imports which are placed on top of all others.
7. Avoid more than one empty line at a time.
8. Avoid trailing free space. For example, avoid `if (true) |||` where | represents a space.
9. If a file happens to differ from these guidelines, use the existing style from the file.
10. We use all language keywords instead of BCL types (e.g. `int, string, float` instead of `Int32, String, Single`) for both type references as well as method calls (e.g. `int.Parse(string)` instead of `Int32.Parse(string)`).
11. We use PascalCasing to name all our constant local variables and fields.
12. We use PascalCasing for all method names, including local functions.
