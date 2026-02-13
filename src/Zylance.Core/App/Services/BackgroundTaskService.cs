using Zylance.Contract.Api.Background;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.App.Services;

/// <summary>
///     Service for managing and reporting background task progress.
///     Emits events to notify the UI about task lifecycle and progress.
/// </summary>
public class BackgroundTaskService(ZylanceCore zylanceCore)
{
    /// <summary>
    ///     Notifies that a background task has started.
    /// </summary>
    /// <param name="taskId">Unique identifier for the task</param>
    /// <param name="description">Optional human-readable description of the task</param>
    public void NotifyWorkStart(string taskId, string? description = null)
    {
        var evt = new BackgroundWorkStartEvt { TaskId = taskId, Description = description };
        zylanceCore.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    /// <summary>
    ///     Notifies about progress update for a background task.
    /// </summary>
    /// <param name="taskId">Unique identifier for the task</param>
    /// <param name="progress">Progress value between 0.0 and 1.0</param>
    /// <param name="description">Optional updated description of the task</param>
    public void NotifyWorkProgress(string taskId, float progress, string? description = null)
    {
        var evt = new BackgroundWorkProgressEvt
        {
            TaskId = taskId,
            Progress = Math.Clamp(progress, 0.0f, 1.0f),
            Description = description,
        };
        zylanceCore.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    /// <summary>
    ///     Notifies that a background task has finished.
    /// </summary>
    /// <param name="taskId">Unique identifier for the task</param>
    /// <param name="description">Optional final description or completion message</param>
    public void NotifyWorkFinish(string taskId, string? description = null)
    {
        var evt = new BackgroundWorkFinishEvt { TaskId = taskId, Description = description };
        zylanceCore.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    /// <summary>
    ///     Wraps an async operation with automatic background task lifecycle
    ///     management.
    ///     Handles starting, error, and finish events automatically.
    /// </summary>
    /// <param name="description">Initial description of the task</param>
    /// <param name="work">The work to perform, receives a SetProgress callback</param>
    /// <typeparam name="T">Return type of the wrapped operation</typeparam>
    /// <returns>The result of the wrapped operation</returns>
    public async Task<T> WithProgress<T>(string description, Func<Action<float, string?>, Task<T>> work)
    {
        var taskId = Guid.NewGuid().ToString();
        NotifyWorkStart(taskId, description);

        try
        {
            var result = await work(
                (progress, progressDescription) =>
                {
                    NotifyWorkProgress(taskId, progress, progressDescription);
                }
            );

            NotifyWorkFinish(taskId, "Completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            NotifyWorkFinish(taskId, $"Failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Wraps an async operation with automatic background task lifecycle
    ///     management.
    ///     Handles starting, error, and finish events automatically. For operations
    ///     with no return value.
    /// </summary>
    /// <param name="description">Initial description of the task</param>
    /// <param name="work">The work to perform, receives a SetProgress callback</param>
    public async Task WithProgress(string description, Func<Action<float, string?>, Task> work)
    {
        var taskId = Guid.NewGuid().ToString();
        NotifyWorkStart(taskId, description);

        try
        {
            await work(
                (progress, progressDescription) =>
                {
                    NotifyWorkProgress(taskId, progress, progressDescription);
                }
            );

            NotifyWorkFinish(taskId, "Completed successfully");
        }
        catch (Exception ex)
        {
            NotifyWorkFinish(taskId, $"Failed: {ex.Message}");
            throw;
        }
    }
}
