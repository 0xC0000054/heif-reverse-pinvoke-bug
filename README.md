# heif-reverse-pinvoke-bug

This repository demonstrates a reverse P/Invoke marshaling bug that occurs when calling `heif_context_write` on
the x86 version of .NET Core 3.1.10.
This issue does not occur on the x86 version of .NET 5.0 or the x86 version of .NET Framework 4.8.

### Technical details

The `write` callback in the `heif_writer` structure looses the `ctx` parameter when it is marshaled to managed code on the x86 version of .NET Core 3.1.10.

The `heif_writer` [write callback method](https://github.com/strukturag/libheif/blob/667eeabb553ce73094eb29faea3f31fb8610fec2/libheif/heif.h#L1015) looks like this:

```c++
struct heif_error (* write)(struct heif_context* ctx,
                              const void* data,
                              size_t size,
                              void* userdata);
```

My managed code represents that callback using the following [delegate](https://github.com/0xC0000054/libheif-sharp/blob/de4545eb7643c3e3f53ee123c96defb74316b961/src/Interop/LibHeif/IO/heif_writer.cs#L28):

```c#
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate heif_error WriteDelegate(IntPtr ctx, IntPtr data, UIntPtr size, IntPtr userData);
```

Both the native callback and my managed delegate are using the CDecl calling convention.
This works correctly on the x64 version of .NET Core 3.1.10, but on the x86 version the reverse P/Invoke call marshaling is incorrect.

The `data` parameter is marshaled as the `ctx` parameter, the `size` parameter is marshaled as the `data` parameter and the `userData` parameter is marshaled as the `size` parameter.

## Steps to reproduce

1. Open the solution file.
2. Run it in debug mode.

The debugger will break when the Heif write callback is executed (the `Write` method in `LibHeifSharp/IO/HeifWriter.cs`).

The `size` parameter should have a value of `0x00000091`, if it is zero and the data parameter has a value of `0x00000091`
than this bug has occurred.

