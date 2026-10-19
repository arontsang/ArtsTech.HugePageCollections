using System;
using System.Collections.Generic;
using ArtsTech.HugePageCollections.Collections;
using BenchmarkDotNet.Attributes;

namespace ArtsTech.HugePageCollections.Benchmarks;

[MemoryDiagnoser]
public class GuidSetContainsBenchmarks
{
    private const int Capacity = 8_000_000;
    private const int RandomSeed = 4;

    private HugePagesSet<Guid> _hugePagesSet = null!;
    private HashSet<Guid> _hashSet = null!;
    private Guid[] _guids = null!;
    private Random _random = null!;

    [GlobalSetup]
    public void Setup()
    {
        _hugePagesSet = new HugePagesSet<Guid>(Capacity);
        _hashSet = new HashSet<Guid>(Capacity);
        _guids = new Guid[Capacity];

        _random = new Random(RandomSeed);
        Span<byte> bytes = stackalloc byte[16];
        for (int i = 0; i < Capacity; i++)
        {
            Guid value = NextGuid(_random, bytes);
            _guids[i] = value;
            _hashSet.Add(value);
            _hugePagesSet.Add(value);
        }

        if (_hashSet.Count != Capacity || _hugePagesSet.Count != Capacity)
        {
            throw new InvalidOperationException("The generated GUID sequence must contain unique values.");
        }
    }

    [IterationSetup]
    public void SetupIteration()
    {
        _random = new Random(RandomSeed);
        Span<byte> bytes = stackalloc byte[16];
        for (int i = 0; i < Capacity; i++)
        {
            _guids[i] = NextGuid(_random, bytes);
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = Capacity)]
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

    [Benchmark(OperationsPerInvoke = Capacity)]
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
