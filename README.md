# TUnit, the underdog C# testing suite

## Abstract

## Introduction

### The JIT vs AOT duality

C# and [dotnet](https://dotnet.microsoft.com/en-us/) (the C# SDK and runtime), at least by default, are all about JIT. It stands for Just In Time and refers to a compilation and execution philosophy that turns C# into something halfway between a compiled and a parsed programming language.

It works as follows:
- Compile the code down to IR (Intermediate Representation)
- Actually run your IR with a runtime - basically a parser - which is dotnet for C#.
- During runtime, hotpaths - code that is frequently executed - is compiled down to binary and kept in memory to be executed directly the next time.

This keeps C# fast to both compile and to execute and allows to simply and efficiently provide features like reflection and garbage collection.

C# is also capable of Native AOT (Ahead Of Time) compilation, which is basically compiling the code down to machine code for each system (architecture and OS) independently and produce self-sufficient binary files (the executable + objects). When you need high-performance computation, you need to use specific features of C#, like [stackalloc](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc) (which bypasses the Garbage Collector), AOT and others we will get into next.

### Source Generation

An important feature that slows things down and is incompatible with AOT is [reflection](https://en.wikipedia.org/wiki/Reflective_programming), heavily used in C# especially in Testing. This incompatibility has been solved for a long time however with [Source Generation](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/). It can be used to achieve the same goals but does everything at compile time, as opposed to reflection at run time.

Although this technique has exited for a long time (since dotnet 5, which came out in November 2020), it is sadly not yet adopted by Testing platforms, at least as the default behaviour.

### The thread pool

One of C#'s strengths is its thread pool. It is a very powerful and easy-to-use feature that allows you to run functions concurrently or in parallel without worrying too much about thread safety and scaling. This core feature also applies when using Native AOT.

### Testing platforms

Unlike testing frameworks that help you define your tests, testing platforms manage the testing environment. They let you:
- Host the tests on one process
- Control the testing process
- Expose tests to the IDE (works amazingly well with Visual Studio and JetBrains Rider).

Historically, MSTest is THE official and viable testing platform for C#. Since the shift to dotnet Core in 2016, this platform started showing shortcomings. MTP was later created in 2024 to replace it. It added diagnostics, lowered overheads, added controls and options but most importantly added Native AOT support.

### TUnit

Let's review the elements we introduced so far, and figure out how TUnit finds its place amongst it all.

If you want your C# application to be fast, lightweight and native (you do not need to install a program to run your app, in this case dotnet), you want to avoid using some features, mostly JIT and reflection. You also want to parallelise using `async` and `await` and Tasks, which is very easy in C# (in my opinion even easier than in Go).

With MTP, you can now natively compile your tests to run them faster. Tests have a short execution time compared to other programs and rarely change, which lowers the value JIT brings in. MTP also has a single dependency which reduces the amount of recompilations.

Surprisingly, big testing frameworks in C# were or still are stuck to VSTest - the older testing platform - and even if they aren't, they still can't or don't use features like native AOT or parallelisation by default. This is where [TUnit](https://tunit.dev/) comes in.

In the next section, we will see how TUnit utilises these features to improve testing performance.


## Testing in C#

### Dependency Injection and mocking

The flavour of C# commonly used heavily depends on [Dependency Injection](https://www.youtube.com/watch?v=J1f5b4vcxCQ). Not as much as Java and its `FeatureAbstractFactoryProviderFactory` tough.

Let's take an example and work from it. Suppose we have a good old Todo Web API app, without authentication. Let's only say we have a couple of endpoints to implement basic CRUD operations:
- [GET] `/api/{guid}` to get an element
- [GET] `/api/all/{window}` to get a list of 50 elements in window (0 means elements 1-50, 1 means 51-100, etc)
- [POST] `/api/new` to create a new element
- [PATCH] `/api/{guid}` to update an element
- [DELETE] `/api/{guid}` to remove an element
