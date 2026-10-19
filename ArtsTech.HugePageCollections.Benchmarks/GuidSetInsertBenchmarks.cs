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
    private Random _random = null!;

    [GlobalSetup]
    public void Setup()
    {
        _random = new Random(RandomSeed);
        _hugePagesSet = new HugePagesSet<Guid>(InitialCapacity, ClearMode.Never);
        _hashSet = new HashSet<Guid>(InitialCapacity);
    }

    [IterationSetup]
    public void SetupIteration()
    {
        _random = new Random(RandomSeed);
        _hugePagesSet.Clear();
        _hashSet.Clear();
    }

    [Benchmark(Baseline = true)]
    public bool HashSet_AddSingleGuid() => _hashSet.Add(_random.NextGuid());

    [Benchmark]
    public bool HugePagesSet_AddSingleGuid() => _hugePagesSet.Add(_random.NextGuid());

    [GlobalCleanup]
    public void Cleanup()
    {
        _hugePagesSet.Dispose();
    }
}
