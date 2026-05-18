using BenchmarkDotNet.Running;
using DotnetCSharpPlayground;

/*
| Method | Mean    | Error    | StdDev   | Gen0        | Gen1        | Gen2      | Allocated |
|------- |--------:|---------:|---------:|------------:|------------:|----------:|----------:|
| Run1   | 4.478 s | 0.0616 s | 0.0576 s | 540000.0000 | 154000.0000 | 8000.0000 |   9.05 GB |
| Run2   | 2.397 s | 0.0276 s | 0.0258 s | 533000.0000 |           - |         - |   8.32 GB |
| Run3   | 1.039 s | 0.0090 s | 0.0085 s | 145000.0000 |           - |         - |   2.26 GB |
*/
BenchmarkRunner.Run<GCCalls>();
new GCCalls().ParseLargeFiles();