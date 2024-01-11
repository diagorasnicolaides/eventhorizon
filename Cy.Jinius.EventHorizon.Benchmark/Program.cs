using BenchmarkDotNet.Running;
using Cy.Jinius.EventHorizon.Benchmark.Tests;

await Task.Run(() =>
{
    var summary = BenchmarkRunner.Run<CreateAsyncTests>();
    Console.WriteLine(summary);
});