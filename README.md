# Light.Undefine

### What does it do?

Light.Undefine is a .NET Standard 2.0 library allowing you to parse and remove the C# preprocessor directives from your source code. 
To do this, simply instantiate `Light.UndefineTransformation` and call its `Undefine` method where you pass in the source code as well as the defined preprocessor symbols.

```csharp
var transformation = new UndefineTransformation();
Memory<char> cleanedSourceCode = transformation.Undefine(sourceCode, "NETSTANDARD2_0", "NET45");
```

Light.Undefine will then parse each line of your source code and will remove all C# preprocessor directives (namely `#if`, `#else`, `#elif`, and `#endif`) as well as all blocks of code that belong to directives not evaluating to true with the defined preprocessor symbols.

A simple example:

```csharp
public static readonly bool IsFlagsEnum =
#if NET45 || NETSTANDARD2_0
    typeof(T).GetCustomAttribute(Types.FlagsAttributeType) != null;
#elif NETSTANDARD1_0
    typeof(T).GetTypeInfo().GetCustomAttribute(Types.FlagsAttributeType) != null;
#else
    typeof(T).GetCustomAttributes(Types.FlagsAttributeType, false).FirstOrDefault() != null;
#endif
```

will become

```csharp
public static readonly bool IsFlagsEnum =
    typeof(T).GetCustomAttribute(Types.FlagsAttributeType) != null;  
```

### How can I use it?

Light.Undefine is available [as a NuGet package](https://www.nuget.org/packages/Light.Undefine/) or you can download the source code from this repository. It runs on all platforms that support .NET Standard 2.0 and the [System.Memory](https://www.nuget.org/packages/System.Memory/) package.

### Some important things to know

- Personally, I use this library to create publishable source code of my Open Source libraries where I use preprocessor directives. These directives should (usually) not be included in the resulting package because the client projects might not define the same preprocessor symbols when building.
- Light.Undefine should not be used to verify that source code contains valid C# preprocessor directives. It is optimized for speed and directives that do not evaluate to true might not be parsed fully.
- The library makes use of the new `Span<T>` and `Memory<T>` types that enable high performance scenarios when parsing.
