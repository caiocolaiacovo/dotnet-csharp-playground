# dotnet-csharp-playground

C# / .NET 8 playground for experimentation. This README documents only the `DotnetCSharpPlayground/GCCalls.cs` file.

## GCCalls.cs

Benchmark class that compares three approaches for computing the average rating of a specific movie (`movieId = 110`) from the `ratings.csv` file of the [MovieLens 32M](https://grouplens.org/datasets/movielens/) dataset, measuring execution time, allocations, and Garbage Collector counts.

Uses [BenchmarkDotNet](https://benchmarkdotnet.org/) with `[MemoryDiagnoser(displayGenColumns: true)]` to expose per-generation GC columns and allocation metrics.

### Expected CSV format

```
userId,movieId,rating,timestamp
1,17,4.0,944249077
```

File path used by the benchmarks: `ratings.csv` (relative to the project's working directory).

### Benchmarks

Each method is decorated with `[Benchmark]`. `ParseLargeFiles` is the entry point that picks which implementation to run and prints manual metrics (time via `Stopwatch`, collection counts via `GC.CollectionCount`, heap via `GC.GetTotalMemory`, working set via `Process.WorkingSet64`).

| Method  | Strategy                                        | Time     | GC Gen0 | GC Gen1 | GC Gen2 | Heap       |
|---------|-------------------------------------------------|----------|---------|---------|---------|------------|
| `Run1`  | `File.ReadAllLines` + `string.Split`            | ~5.70 s  | 540     | 154     | 8       | ~2828 MB   |
| `Run2`  | Streaming with `StreamReader` + `Split`         | ~2.90 s  | 533     | 1       | 0       | ~13 MB     |
| `Run3`  | Streaming + `ReadOnlySpan<char>` (zero-alloc)   | ~1.85 s  | 145     | 0       | 0       | ~1.8 MB    |

### Learning points

- **`Run1` (naive):** loads the entire file into memory at once. Causes high GC pressure (reaches Gen2) and consumes several GB of heap.
- **`Run2` (streaming):** reads line by line, keeping the heap low. `Split` still allocates a string array per line, producing many Gen0 collections.
- **`Run3` (span-based):** avoids `Split` by using `ReadOnlySpan<char>` and `IndexOf` to locate columns and parse the slice directly, without allocating intermediate strings. Result: minimal heap and almost no GC activity.

### How to run

```bash
dotnet run -c Release --project DotnetCSharpPlayground
```

BenchmarkDotNet reports are generated under `DotnetCSharpPlayground/BenchmarkDotNet.Artifacts/results/`.
