using BenchmarkDotNet.Running;
using ArtsTech.HugePageCollections.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(GuidSetInsertBenchmarks).Assembly).Run(args);
