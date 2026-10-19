# ArtsTech.HugePageCollections

ArtsTech.HugePageCollections provides `HugePagesSet<T>`, a hash-based set implementing `ISet<T>` and related collection interfaces. It is designed for unmanaged value types and stores its buckets and slots in memory allocated by the project’s `HugePageMemoryPool<T>`.

## Highlights

- `HugePagesSet<T>` supports the standard set operations: union, intersection, difference, symmetric difference, and set-relation checks.
- `T` must be an unmanaged type, such as `int`, `Guid`, or a custom unmanaged struct.
- The set implements `IDisposable`. Dispose it when finished so its native memory mappings are released.
- On Linux, backing memory is allocated with `mmap` and advised with `MADV_HUGEPAGE`. The allocation can use standard pages when transparent huge pages are unavailable; reserving explicit huge pages is not required.
- The current memory-pool implementation supports Linux. Other operating systems are not currently supported.

## Usage

```csharp
using System;
using ArtsTech.HugePageCollections.Collections;

using var ids = new HugePagesSet<Guid>(capacity: 1_024);
Guid lookupId = Guid.NewGuid();
Guid[] otherIds = { Guid.NewGuid(), Guid.NewGuid() };

ids.Add(lookupId);
ids.Add(Guid.NewGuid());

if (ids.Contains(lookupId))
{
    // ...
}

ids.UnionWith(otherIds);
```

`HugePagesSet<T>` also accepts an `IEqualityComparer<T>` and a `ClearMode`. Use `ClearMode` to control whether removed values are cleared from internal storage.

## Build and test

The library targets .NET 6.0. The unit-test and benchmark projects target .NET 9.0.

```sh
dotnet build ArtsTech.HugePageCollections.slnx
dotnet test ArtsTech.HugePageCollections.UnitTests/ArtsTech.HugePageCollections.UnitTests.csproj
```

## Benchmarks

The BenchmarkDotNet project compares single `Guid` insertions and full-set `Contains` hits for `HugePagesSet<Guid>` and `HashSet<Guid>`. Both sets are initialized with a capacity of 8 million. The benchmarks generate GUIDs from a seeded `Random` (seed `4`), so both implementations receive the same sequence. The `Contains` benchmark prepopulates both sets with 8 million GUIDs and scans the same sequence on each iteration.

Run the benchmarks in Release configuration:

```sh
dotnet run -c Release --project ArtsTech.HugePageCollections.Benchmarks/ArtsTech.HugePageCollections.Benchmarks.csproj -- --filter '*GuidSet*Benchmarks*'
```

Sample insertion results:

| Method                     | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |---------:|---------:|---------:|---------:|------:|--------:|----------:|------------:|
| HashSet_AddSingleGuid      | 43.05 us | 2.995 us | 8.300 us | 43.49 us |  1.13 |    1.05 |     448 B |        1.00 |
| HugePagesSet_AddSingleGuid | 10.71 us | 0.868 us | 2.517 us | 11.70 us |  0.28 |    0.27 |     736 B |        1.64 |

Benchmark results vary with hardware, runtime, operating-system configuration, and load.
