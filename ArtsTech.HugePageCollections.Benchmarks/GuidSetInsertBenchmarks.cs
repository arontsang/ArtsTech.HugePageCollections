using System;
using System.Collections.Generic;
using ArtsTech.HugePageCollections.Collections;
using BenchmarkDotNet.Attributes;

namespace ArtsTech.HugePageCollections.Benchmarks;

[MemoryDiagnoser]
public class GuidSetInsertBenchmarks
{
    private const int InitialCapacity = 8_000_000;

    private HugePagesSet<Guid> _hugePagesSet = null!;
    private HashSet<Guid> _hashSet = null!;
    private Guid _value;

    [GlobalSetup]
    public void Setup()
    {
        _hugePagesSet = new HugePagesSet<Guid>(InitialCapacity);
        _hashSet = new HashSet<Guid>(InitialCapacity);
    }

    [IterationSetup]
    public void SetupIteration()
    {
        _value = Guid.NewGuid();
    }

    [Benchmark(Baseline = true)]
    public bool HashSet_AddSingleGuid() => _hashSet.Add(_value);

    [Benchmark]
    public bool HugePagesSet_AddSingleGuid() => _hugePagesSet.Add(_value);

    [GlobalCleanup]
    public void Cleanup()
    {
        _hugePagesSet.Dispose();
    }
}
