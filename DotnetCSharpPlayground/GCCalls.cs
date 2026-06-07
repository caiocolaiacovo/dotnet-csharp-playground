namespace DotnetCSharpPlayground;

using System.Diagnostics;
using System.Globalization;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser(displayGenColumns: true)] // Enables GC and Allocation tracking
public class GCCalls
{
    [Benchmark]
    public void ParseLargeFiles()
    {
        var gc1before = GC.CollectionCount(0);
        var gc2before = GC.CollectionCount(1);
        var gc3before = GC.CollectionCount(2);
        var stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        // File format:
        // userId,movieId,rating,timestamp
        // 1,17,4.0,944249077
        // Run1();
        // Run2();
        Run3();

        stopwatch.Stop();
        Console.WriteLine($"Time taken: {stopwatch.Elapsed.TotalSeconds} seconds");
        Console.WriteLine($"GC Gen 0: {GC.CollectionCount(0) - gc1before}");
        Console.WriteLine($"GC Gen 1: {GC.CollectionCount(1) - gc2before}");
        Console.WriteLine($"GC Gen 2: {GC.CollectionCount(2) - gc3before}");
        Console.WriteLine($"Heap size: {GC.GetTotalMemory(false) / 1024.0 / 1024.0} MB");
        Console.WriteLine($"Total memory: {Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0} MB");
    }

    /*
        Average rating for movie 110 is 3,9886301488155205
        Time taken: 5,7041805 seconds
        GC Gen 0: 540
        GC Gen 1: 154
        GC Gen 2: 8
        Heap size: 2828,358413696289 MB
        Total memory: 3081,16015625 MB
    */
    [Benchmark]
    public void Run1()
    {
        Console.WriteLine("Run1 -> naive solution");
        var lines = File.ReadAllLines("ratings.csv");
        var sum = 0d;
        var count = 0;

        foreach (var line in lines)
        {
            var part = line.Split(',');

            if (part[1] == "110")
            {
                sum += double.Parse(part[2], CultureInfo.InvariantCulture);
                count++;
            }
        }

        Console.WriteLine($"Average rating for movie 110 is {sum / count}");
    }

    /*
        Time taken: 2,9026069 seconds
        GC Gen 0: 533
        GC Gen 1: 1
        GC Gen 2: 0
        Heap size: 13,130088806152344 MB
        Total memory: 53,671875 MB
    */
    [Benchmark]
    public void Run2()
    {
        Console.WriteLine("Run2 -> streaming solution (best solution?)");

        var sum = 0d;
        var count = 0;

        using var fs = File.OpenRead("ratings.csv");
        using var sr = new StreamReader(fs);

        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            var part = line.Split(',');

            if (part[1] == "110")
            {
                sum += double.Parse(part[2], CultureInfo.InvariantCulture);
                count++;
            }
        }

        Console.WriteLine($"Average rating for movie 110 is {sum / count}");
    }

    /*
        Time taken: 1,8508126 seconds
        GC Gen 0: 145
        GC Gen 1: 0
        GC Gen 2: 0
        Heap size: 1,8041839599609375 MB
        Total memory: 53,33203125 MB
    */
    [Benchmark]
    public void Run3()
    {
        Console.WriteLine("Run3 -> span-based solution");

        var sum = 0d;
        var count = 0;
        string? line;

        var lookingForMovieId = "110".AsSpan(); // Using Span to avoid allocations

        using var fs = File.OpenRead("ratings.csv");
        using var sr = new StreamReader(fs);
        while ((line = sr.ReadLine()) != null)
        {
            var span = line.AsSpan(line.IndexOf(',') + 1); //ignore everything before the first comma
            
            // Find the movieId (second column)
            var nextColumnPos = span.IndexOf(','); // find the next comma (column rating)
            var movieId = span.Slice(0, nextColumnPos); // get the movieId as a Span
            if (!movieId.SequenceEqual(lookingForMovieId))
            {
                continue;
            }
            
            // If we found the movieId, parse the rating
            span = span.Slice(nextColumnPos + 1); // move to the rating column
            nextColumnPos = span.IndexOf(','); // find the next comma (column timestamp)
            var rating = double.Parse(span.Slice(0, nextColumnPos), CultureInfo.InvariantCulture); // parse the rating
            sum += rating;
            count++;
        }

        Console.WriteLine($"Average rating for movie 110 is {sum / count}");
    }
}