# FastDelegate.Net
A replacement for .Net's slow Delegate.DynamicInvoke. (Requires .Net 3.5 or newer)

Delegate.DynamicInvoke uses late binding which is quite slow. FastDelegate generates, compiles, and caches code that is *almost* as fast as a regular method call. It works with any method, instance or static, void return or otherwise. All methods are bound the same way:

```
using FastDelegate.Net;

System.Reflection.MethodInfo myMethodInfo;
// fill in myMethodInfo
// ...

Func<Object, Object[], Object> delegate = myMethodInfo.Bind();
```

When myMethodInfo represents an instance method, the first Func parameter is the object instance to call the method on. When myMethodInfo represents a static method, the first parameter should be null. The return value of the Func is the value returned by the method, or null in the case of void.
