using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adamijak.Collections.Extensions;

public static class IEnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> values, Action<T> func, CancellationToken cancelToken = default)
    {
        foreach (var value in values)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            func(value);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> values, Func<T, Task> func, CancellationToken cancelToken = default)
    {
        foreach (var value in values)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            await func(value);
        }
    }

    public static Task ForEachAsync<T>(this IEnumerable<T> values, Action<T> func, int maxWorkerCount = 5, CancellationToken cancelToken = default)
        => ForEachAsync<T>(values, i => { func(i); return Task.CompletedTask; }, maxWorkerCount, cancelToken);

    public static async Task ForEachAsync<T>(this IEnumerable<T> values, Func<T, Task> func, int maxWorkerCount = 5, CancellationToken cancelToken = default)
    {
        if(maxWorkerCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxWorkerCount), maxWorkerCount, "Can not be less than 1.");
        }
        var queue = new ConcurrentQueue<T>(values);

        var tasks = new List<Task>();
        foreach (var i in Enumerable.Range(0, Math.Min(queue.Count, maxWorkerCount)))
        {
            tasks.Add(ForEachTaskAsync(queue, func, cancelToken));
        }
        await Task.WhenAll(tasks);
    }

    private static async Task ForEachTaskAsync<T>(ConcurrentQueue<T> queue, Func<T, Task> func, CancellationToken cancelToken = default)
    {
        while (!queue.IsEmpty)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }
            if (queue.TryDequeue(out var value))
            {
                await func(value);
            }
        }
    }
}

