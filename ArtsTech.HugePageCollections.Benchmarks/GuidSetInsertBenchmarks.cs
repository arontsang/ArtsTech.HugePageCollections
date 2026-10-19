using System;
using System.Collections.Generic;
using ArtsTech.HugePageCollections.Collections;
using BenchmarkDotNet.Attributes;

namespace ArtsTech.HugePageCollections.Benchmarks;

[MemoryDiagnoser]
public class GuidSetInsertBenchmarks
{
    private const int InitialCapacity = 16_000_000;
    private const int RandomSeed = 4; // Chosen by fair dice roll. Guaranteed to be random.

    private HugePagesSet<Guid> _hugePagesSet = null!;
    private HashSet<Guid> _hashSet = null!;
    private List<Guid> _guids = null!;

    private const int LoopCount = 16_000_000;
    
    [Params(true, false)]
    public bool AlreadyExist { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _hugePagesSet = new HugePagesSet<Guid>(InitialCapacity, ClearMode.Never);
        _hashSet = new HashSet<Guid>(InitialCapacity);
        
        _guids = new List<Guid>(InitialCapacity);
        for (var i = 0; i < InitialCapacity; i++)
        {
            _hashSet.Add(Guid.NewGuid());
        }
    }

    [IterationSetup]
    public void SetupIteration()
    {
        if (!AlreadyExist)
        {
            _hugePagesSet.Clear();
            _hashSet.Clear();
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = InitialCapacity)]
    public void HashSet_AddSingleGuid()
    {
        for (var i = 0; i < LoopCount; i++)
        {
            _hugePagesSet.Clear();
            foreach (var item in _guids)
            {
                _hashSet.Add(item);
            }
        }
    }

    [Benchmark(OperationsPerInvoke = InitialCapacity)]
    public void HugePagesSet_AddSingleGuid()
    {
        for (var i = 0; i < LoopCount; i++)
        {
            _hugePagesSet.Clear();
            foreach (var item in _guids)
            {
                _hugePagesSet.Add(item);
            }
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _hugePagesSet.Dispose();
    }
}
