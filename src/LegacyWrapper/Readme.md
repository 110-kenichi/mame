# LegacyWrapper

## About

LegacyWrapper uses a wrapper process to call dlls from a process of the opposing architecture (X86 or AMD64).

Since you can't load a dll of another architecture directly, the wrapper utilizes a named pipe to abstract the call. You won't notice this though, because all the magic is hidden behind a single static method.

## NuGet Package

There is a NuGet package available here: [Codefoundry.LegacyWrapper @ nuget.org](https://www.nuget.org/packages/Codefoundry.LegacyWrapper/)

## Usage

If you want to compile the LegacyWrapper yourself, make sure to place both the wrapper executable, LegacyWrapperClient.dll and LegacyWrapper.Common.dll in your directory.

```csharp
// Define delegate matching DLL function
private delegate int GetSystemMetrics(int index);

// Create new WrapperClient
// Remember to ensure a call to the Dispose()-Method!
using (var client = new WrapperClient("User32.dll"))
{
    // Make calls providing library name, function name, and parameters
    int x = (int)client.Invoke<GetSystemMetrics>("GetSystemMetrics", new object[] { 0 });
    int y = (int)client.Invoke<GetSystemMetrics>("GetSystemMetrics", new object[] { 1 });
}
```

The constructor takes an optional second parameter where you can specify the target architecture (it defaults to X86):

```csharp
using (var client = new WrapperClient(TestDllPath, TargetArchitecture.Amd64))
{
    result = (int)client.Invoke<TestStdCallDelegate>("TestStdCall", new object[] { input });
}
```

Please note that loading a 64bit dll will only work on 64bit operating systems.

### ref parameters

If your delegate contains `ref` parameters, the object array passed as parameters to the `Invoke<T>` method will contain the updated values afterwards.

## Todo (maybe)

* Support for Attributes like `[CallingConvention]`.
* Type safe usage of generics in Call<T>-Method

## Further reading

View [this blog post](https://codefoundry.de/programming/2015/09/28/legacy-wrapper-invoking-an-unmanaged-32bit-library-out-of-a-64bit-process.html) to obtain a basic understanding of how the library works internally.

## Contributing

Feel free to submit any [suggestions/issues](https://github.com/CodefoundryDE/LegacyWrapper/issues) and contribute to LegacyWrapper.

## License

Copyright (c) 2017, Franz Wimmer. (MIT License)

See LICENSE for more info.

## Credits

This library includes [Nuane.Interop](https://github.com/lukaaash/Nuane.Interop) written by Lukas Pokorny.
