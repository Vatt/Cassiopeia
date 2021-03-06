using Microsoft.Extensions.Logging;

namespace Cassiopeia.Connections.Transport.Abstractions;

internal static class ConnectionCompletion
{
    public static Task FireOnCompletedAsync(ILogger logger, Stack<KeyValuePair<Func<object, Task>, object>>? onCompleted)
    {
        if (onCompleted == null || onCompleted.Count == 0)
        {
            return Task.CompletedTask;
        }

        return CompleteAsyncMayAwait(logger, onCompleted);
    }

    private static Task CompleteAsyncMayAwait(ILogger logger, Stack<KeyValuePair<Func<object, Task>, object>> onCompleted)
    {
        while (onCompleted.TryPop(out var entry))
        {
            try
            {
                var task = entry.Key.Invoke(entry.Value);
                if (!task.IsCompletedSuccessfully)
                {
                    return CompleteAsyncAwaited(task, logger, onCompleted);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred running an IConnectionCompleteFeature.OnCompleted callback.");
            }
        }

        return Task.CompletedTask;
    }

    private static async Task CompleteAsyncAwaited(Task currentTask, ILogger logger, Stack<KeyValuePair<Func<object, Task>, object>> onCompleted)
    {
        try
        {
            await currentTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred running an IConnectionCompleteFeature.OnCompleted callback.");
        }

        while (onCompleted.TryPop(out var entry))
        {
            try
            {
                await entry.Key.Invoke(entry.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred running an IConnectionCompleteFeature.OnCompleted callback.");
            }
        }
    }
}
