using System;
using System.Collections.Generic;
using ArtsTech.HugePageCollections.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace ArtsTech.HugePageCollections.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 1, iterationCount: 3, invocationCount: 10_000)]
public class GuidSetContainsBenchmarks
{
    private const int Capacity = 8_000_000;
    private const int QueryCount = 100_000;
    private const int RandomSeed = 4;

    [Params(25, 50, 75, 100)]
    public int FillFactorPercent { get; set; }

    private HugePagesSet<Guid> _hugePagesSet = null!;
    private HashSet<Guid> _hashSet = null!;
    private Random _random = null!;
    private int _populationCount;
    
    
    private const int OperationsPerInvoke = 8_000_000 / 4;

    [GlobalSetup]
    public void Setup()
    {
        _hugePagesSet = new HugePagesSet<Guid>(Capacity);
        _hashSet = new HashSet<Guid>(Capacity);
        _populationCount = Capacity * FillFactorPercent / 100;

        _random = new Random(RandomSeed);
        Span<byte> bytes = stackalloc byte[16];
        for (int i = 0; i < _populationCount; i++)
        {
            Guid value = NextGuid(_random, bytes);
            _hashSet.Add(value);
            _hugePagesSet.Add(value);
        }

        if (_hashSet.Count != _populationCount || _hugePagesSet.Count != _populationCount)
        {
            throw new InvalidOperationException("The generated GUID sequence must contain unique values.");
        }
    }



    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
    public void HashSet_ContainsHits()
    {
        var random = new Random(RandomSeed);
        for (var i = 0; i < OperationsPerInvoke; i++)
        {
            _ = _hashSet.Contains(random.NextGuid());
        }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void HugePagesSet_ContainsHits()
    {
        var random = new Random(RandomSeed);
        for (var i = 0; i < OperationsPerInvoke; i++)
        {
            _ = _hugePagesSet.Contains(random.NextGuid());
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _hugePagesSet.Dispose();
    }

    private static Guid NextGuid(Random random, Span<byte> bytes)
    {
        random.NextBytes(bytes);
        return new Guid(bytes);
    }
}
