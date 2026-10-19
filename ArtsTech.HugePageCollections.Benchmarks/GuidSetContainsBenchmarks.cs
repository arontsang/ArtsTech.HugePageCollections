using System;
using System.Collections.Generic;
using ArtsTech.HugePageCollections.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace ArtsTech.HugePageCollections.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class GuidSetContainsBenchmarks
{
    private const int Capacity = 8_000_000;
    private const int QueryCount = 100_000;
    private const int RandomSeed = 4;

    [Params(25, 50, 75, 100)]
    public int FillFactorPercent { get; set; }

    private HugePagesSet<Guid> _hugePagesSet = null!;
    private HashSet<Guid> _hashSet = null!;
    private Guid[] _guids = null!;
    private Random _random = null!;
    private int _populationCount;

    [GlobalSetup]
    public void Setup()
    {
        _hugePagesSet = new HugePagesSet<Guid>(Capacity);
        _hashSet = new HashSet<Guid>(Capacity);
        _guids = new Guid[QueryCount];
        _populationCount = Capacity * FillFactorPercent / 100;

        _random = new Random(RandomSeed);
        Span<byte> bytes = stackalloc byte[16];
        for (int i = 0; i < _populationCount; i++)
        {
            Guid value = NextGuid(_random, bytes);
            if (i < QueryCount)
            {
                _guids[i] = value;
            }
            _hashSet.Add(value);
            _hugePagesSet.Add(value);
        }

        if (_hashSet.Count != _populationCount || _hugePagesSet.Count != _populationCount)
        {
            throw new InvalidOperationException("The generated GUID sequence must contain unique values.");
        }
    }

    [IterationSetup]
    public void SetupIteration()
    {
        _random = new Random(RandomSeed);
        Span<byte> bytes = stackalloc byte[16];
        for (int i = 0; i < QueryCount; i++)
        {
            _guids[i] = NextGuid(_random, bytes);
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = QueryCount)]
    public int HashSet_ContainsHits()
    {
        int hits = 0;
        for (int i = 0; i < _guids.Length; i++)
        {
            if (_hashSet.Contains(_guids[i]))
            {
                hits++;
            }
        }
        return hits;
    }

    [Benchmark(OperationsPerInvoke = QueryCount)]
    public int HugePagesSet_ContainsHits()
    {
        int hits = 0;
        for (int i = 0; i < _guids.Length; i++)
        {
            if (_hugePagesSet.Contains(_guids[i]))
            {
                hits++;
            }
        }
        return hits;
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
