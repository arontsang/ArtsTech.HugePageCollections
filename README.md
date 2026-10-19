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

The BenchmarkDotNet project compares single `Guid` insertions and `Contains` hits for `HugePagesSet<Guid>` and `HashSet<Guid>`. Both sets are initialized with a capacity of 8 million. The benchmarks generate GUIDs from a seeded `Random` (seed `4`), so both implementations receive the same sequence. The `Contains` benchmark prepopulates both sets to the selected fill factor (25%, 50%, 75%, or 100%) and scans 100,000 matching GUIDs per iteration. It uses one warmup and three measurement iterations to keep run times shorter.

Run the benchmarks in Release configuration:

```sh
dotnet run -c Release --project ArtsTech.HugePageCollections.Benchmarks/ArtsTech.HugePageCollections.Benchmarks.csproj -- --filter '*GuidSet*Benchmarks*'
```

Sample insertion results:

| Method                     | Mean      | Error     | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |----------:|----------:|---------:|------:|--------:|----------:|------------:|
| HashSet_AddSingleGuid      |  7.758 us | 0.4795 us | 1.360 us |  1.03 |    0.27 |     736 B |        1.00 |
| HugePagesSet_AddSingleGuid | 12.178 us | 0.5743 us | 1.591 us |  1.62 |    0.37 |     400 B |        0.54 |
Sample Set.Contains results:

| Method                    | FillFactorPercent | Mean       | Error       | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------------- |------------------ |-----------:|------------:|----------:|------:|--------:|----------:|------------:|
| HashSet_ContainsHits      | 25                |   872.8 ns |  3,225.3 ns | 176.79 ns |  1.03 |    0.24 |         - |          NA |
| HugePagesSet_ContainsHits | 25                |   913.2 ns |  3,284.4 ns | 180.03 ns |  1.07 |    0.25 |         - |          NA |
|                           |                   |            |             |           |       |         |           |             |
| HashSet_ContainsHits      | 50                |   858.4 ns |    415.1 ns |  22.76 ns |  1.00 |    0.03 |         - |          NA |
| HugePagesSet_ContainsHits | 50                |   998.4 ns |  2,517.5 ns | 137.99 ns |  1.16 |    0.14 |         - |          NA |
|                           |                   |            |             |           |       |         |           |             |
| HashSet_ContainsHits      | 75                |   989.2 ns |    414.8 ns |  22.74 ns |  1.00 |    0.03 |         - |          NA |
| HugePagesSet_ContainsHits | 75                |   891.3 ns |    420.8 ns |  23.07 ns |  0.90 |    0.03 |         - |          NA |
|                           |                   |            |             |           |       |         |           |             |
| HashSet_ContainsHits      | 100               | 1,712.3 ns | 12,176.5 ns | 667.44 ns |  1.09 |    0.50 |         - |          NA |
| HugePagesSet_ContainsHits | 100               |   962.4 ns |    316.2 ns |  17.33 ns |  0.61 |    0.18 |         - |          NA |

Benchmark results vary with hardware, runtime, operating-system configuration, and load.
