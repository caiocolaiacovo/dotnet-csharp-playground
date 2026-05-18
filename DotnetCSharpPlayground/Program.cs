using BenchmarkDotNet.Running;
using DotnetCSharpPlayground;

// BenchmarkRunner.Run<GCCalls>();

var gcCalls = new GCCalls();
gcCalls.ParseLargeFiles();