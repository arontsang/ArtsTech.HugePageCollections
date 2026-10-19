using System;
using System.Collections.Generic;
using ArtsTech.HugePageCollections.Collections;
using BenchmarkDotNet.Attributes;

namespace ArtsTech.HugePageCollections.Benchmarks;

[MemoryDiagnoser]
public class GuidSetInsertBenchmarks
{
    private const int InitialCapacity = 8_000_000;
    private const int RandomSeed = 4; // Chosen by fair dice roll. Guaranteed to be random.

    private HugePagesSet<Guid> _hugePagesSet = null!;
    private HashSet<Guid> _hashSet = null!;
    private List<Guid> _guids = null!;

    private const int OperationsPerInvoke = 8_000_000;

    [GlobalSetup]
    public void Setup()
    {
        _hugePagesSet = new HugePagesSet<Guid>(InitialCapacity, ClearMode.Never);
        _hashSet = new HashSet<Guid>(InitialCapacity);
        
        _guids = new List<Guid>(OperationsPerInvoke);
        for (var i = 0; i < OperationsPerInvoke; i++)
        {
            _hashSet.Add(Guid.NewGuid());
        }
    }

    [IterationSetup]
    public void SetupIteration()
    {
        _hugePagesSet.Clear();
        _hashSet.Clear();
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
    public void HashSet_AddSingleGuid()
    {
        foreach (var item in _guids)
        {
            _hashSet.Add(item);
        }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void HugePagesSet_AddSingleGuid()
    {
        foreach (var item in _guids)
        {
            _hashSet.Add(item);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _hugePagesSet.Dispose();
    }
}
