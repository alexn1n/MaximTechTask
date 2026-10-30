using BenchmarkDotNet.Running;
using MaximTech.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(FindNearestBenchmarks).Assembly).Run(args);
