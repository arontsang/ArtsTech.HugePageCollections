using System;
using System.Collections.Generic;
using ArtsTech.HugePageCollections.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;

namespace ArtsTech.HugePageCollections.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(Config))]
public class GuidSetInsertBenchmarks
{
    private class Config : ManualConfig
    {
        public Config()
        {
            AddColumn(
                StatisticColumn.P0,
                StatisticColumn.P25,
                StatisticColumn.P50,
                StatisticColumn.P67,
                StatisticColumn.P80,
                StatisticColumn.P85,
                StatisticColumn.P90,
                StatisticColumn.P95,
                StatisticColumn.P100);
        }
    }
    
    private const int InitialCapacity = 16_000_000;
    private const int RandomSeed = 4; // Chosen by fair dice roll. Guaranteed to be random.

    private HugePagesSet<Guid> _hugePagesSet = null!;
    private HashSet<Guid> _hashSet = null!;
    private List<Guid> _guids = null!;

    private const int LoopCount = 16_000_000;
    
    [Params(true, false)]
    public bool AlreadyExist { get; set; }
    
    [Params(true, false)]
    public bool Preallocated { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _hugePagesSet = Preallocated 
            ? new HugePagesSet<Guid>(InitialCapacity, ClearMode.Never)
            : new HugePagesSet<Guid>(ClearMode.Never);
        _hashSet = Preallocated
            ? new HashSet<Guid>(InitialCapacity)
            : new HashSet<Guid>();
        
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

        if (!Preallocated)
        {
            _hugePagesSet.TrimExcess();
            _hashSet.TrimExcess();
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
