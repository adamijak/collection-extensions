using System.Collections.Concurrent;
using System.Diagnostics;
using Adamijak.Collections.Extensions;

namespace CollectionExtensionsTests;

[TestClass]
public class ForEachTests
{
    private const int Delay = 1000;
    private const int ValueCount = 1000;
    private readonly IEnumerable<int> Values = Enumerable.Range(0, ValueCount);

    [TestMethod]
    public async Task ProcessesAll()
    {
        var rnd = new Random();
        var concurrentList = new ConcurrentBag<int>();
        await Values.ForEachAsync(async i =>
        {
            await Task.Delay(rnd.Next(0, 200));
            concurrentList.Add(i);
        }, 100);
        Assert.AreEqual(ValueCount, concurrentList.Count);
    }

    [TestMethod]
    public async Task RunsConcurrently()
    {
        var watch = Stopwatch.StartNew();
        await Values.ForEachAsync(async i =>
        {
            await Task.Delay(Delay);
        }, 200);
        watch.Stop();
        Assert.IsTrue(Delay * 4.5 < watch.Elapsed.TotalMilliseconds);
        Assert.IsTrue(watch.Elapsed.TotalMilliseconds < Delay * 5.5);
    }

    [TestMethod]
    [DataTestMethod]
    [DataRow(5)]
    [DataRow(1)]
    [DataRow(0)]
    [DataRow(-5)]
    public async Task MaxWorkerCount(int maxWorkerCount)
    {
        if (maxWorkerCount < 1)
        {
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => Values.ForEachAsync(i => Task.CompletedTask, maxWorkerCount));
        }
        else
        {
            await Values.ForEachAsync(i => Task.CompletedTask, maxWorkerCount);
        }
    }
}
