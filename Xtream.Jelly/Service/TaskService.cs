using System;
using System.Linq;
using MediaBrowser.Model.Tasks;

namespace Xtream.Jelly.Service;

/// <summary>
/// A service for triggering Jellyfin tasks.
/// </summary>
/// <param name="taskManager">Instance of the <see cref="ITaskManager"/> interface.</param>
public class TaskService(ITaskManager taskManager)
{
    private static Type? FindType(string assembly, string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a =>
                !a.IsDynamic &&
                (a.FullName?.StartsWith($"{assembly},", StringComparison.InvariantCulture) ?? false))
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t?.FullName == fullName);
    }

    /// <summary>
    /// Executes a task from the given assembly and name.
    /// </summary>
    /// <param name="assembly">The name of the assembly to search in for the type.</param>
    /// <param name="fullName">The full name of the task type.</param>
    public void CancelIfRunningAndQueue(string assembly, string fullName)
    {
        Type refreshType = FindType(assembly, fullName) ?? throw new ArgumentException("Refresh task not found");

        typeof(ITaskManager)
            .GetMethod(nameof(ITaskManager.CancelIfRunningAndQueue), 1, [])?
            .MakeGenericMethod(refreshType)?
            .Invoke(taskManager, []);
    }
}
