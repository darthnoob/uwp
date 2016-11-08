using Windows.ApplicationModel.Background;

namespace MegaApp.Services
{
    public static class TaskService
    {
        /// <summary>
        /// Register a background task with the specified taskEntryPoint, name, trigger and condition (optional).
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="taskName">A name for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        /// <param name="condition">Optional parameter. A conditional event that must be true for the task to fire.</param>
        /// <returns></returns>
        public static BackgroundTaskRegistration RegisterBackgroundTask(
            string taskEntryPoint,
            string taskName,
            IBackgroundTrigger trigger,
            IBackgroundCondition condition)
        {
            
            // Check for existing registrations of this background task.
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {

                if (cur.Value.Name == taskName)
                {
                    // The task is already registered.
                    return (BackgroundTaskRegistration) cur.Value;
                }
            }
            
            // Register the background task.
            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = taskEntryPoint
            };

            builder.SetTrigger(trigger);

            if (condition != null)
                builder.AddCondition(condition);

            var task = builder.Register();

            return task;
        }

        /// <summary>
        /// Unregister a background task with the specified taskEntryPoint and name.
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point of the background task.</param>
        /// <param name="taskName">The name of the background task.</param>
        /// <returns>True if found and False if not found</returns>
        public static bool UnregisterBackgroundTask(
            string taskEntryPoint, 
            string taskName)
        {

            // Check for existing registrations of this background task.
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name != taskName) continue;
                // if found, unregister and return True
                cur.Value.Unregister(true);
                return true;
            }

            // No existing registration found of this background task
            return false;
        }
    }
}
