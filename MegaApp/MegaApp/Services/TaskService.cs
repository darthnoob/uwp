using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using mega;
using MegaApp.Extensions;

namespace MegaApp.Services
{
    public static class TaskService
    {
        public const string CameraUploadTaskEntryPoint = "BackgroundTaskService.CameraUploadTask";
        public const string CameraUploadTaskName = "CameraUploadTask";
        public const int CameraUploadTaskTimeTrigger = 15;

        /// <summary>
        /// Get the value if background task registration is allowed for this app.
        /// Request is needed before you register any task.
        /// </summary>
        /// <returns>True if background task are allowed, False otherwise</returns>
        public static async Task<bool> RequestBackgroundAccessAsync()
        {
            var result = await BackgroundExecutionManager.RequestAccessAsync();
            return result != BackgroundAccessStatus.DeniedByUser &&
                   result != BackgroundAccessStatus.DeniedBySystemPolicy &&
                   result != BackgroundAccessStatus.Unspecified;
        }

        /// <summary>
        /// Register a background task with the specified taskEntryPoint, name, trigger and condition (optional).
        /// Unregisters previous task with the same entry and name.
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="taskName">A name for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        /// <param name="condition">Optional parameter. A conditional event that must be true for the task to fire.</param>
        /// <returns>Background task registration</returns>
        public static BackgroundTaskRegistration RegisterBackgroundTask(
            string taskEntryPoint,
            string taskName,
            IBackgroundTrigger trigger,
            IBackgroundCondition condition)
        {
            // Check for existing registrations of this background task.
            var task = BackgroundTaskRegistration.AllTasks
                .FirstOrDefault(t => t.Value.Name.Equals(taskName));

            // If the task already exists, first unregister, then register new task
            if (!task.IsNull()) task.Value.Unregister(true);
            
            // Register the background task.
            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = taskEntryPoint
            };

            // Set trigger when to start task
            builder.SetTrigger(trigger);

            // Add optional conditions for trigger
            if (condition != null) builder.AddCondition(condition);

            // Register and return the task registration
            return builder.Register();
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
            var task = BackgroundTaskRegistration.AllTasks
                .FirstOrDefault(t => t.Value.Name.Equals(taskName));

            // No existing registration found of this background task
            if (task.IsNull()) return false;

            // if found, unregister and return True
            task.Value.Unregister(true);
            return true;
        }

        /// <summary>
        /// Get a background task with the specified taskEntryPoint and name.
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point of the background task.</param>
        /// <param name="taskName">The name of the background task.</param>
        /// <returns>Background task registration if exists, else NULL</returns>
        public static BackgroundTaskRegistration GetBackgroundTask(
            string taskEntryPoint,
            string taskName)
        {
            // Check for existing registrations of this background task.
            var task = BackgroundTaskRegistration.AllTasks
                .FirstOrDefault(t => t.Value.Name.Equals(taskName));
            
        // Return task if found, else return null
            if (!task.IsNull()) return (BackgroundTaskRegistration)task.Value;
            return null;
        }

        public static bool IsBackGroundTaskActive(
            string taskEntryPoint,
            string taskName)
        {
            var task = GetBackgroundTask(taskEntryPoint, taskName);
            return task != null;
        }

        /// <summary>
        /// Reset the "Camera Uploads" service if is enabled
        /// </summary>
        public static void ResetCameraUploadsTask()
        {
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Resetting CAMERA UPLOADS service...");

            if (!IsBackGroundTaskActive(CameraUploadTaskEntryPoint, CameraUploadTaskName))
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "CAMERA UPLOADS service is currently disabled");
                return;
            }

            try
            {
                UnregisterBackgroundTask(CameraUploadTaskEntryPoint, CameraUploadTaskName);
                RegisterBackgroundTask(CameraUploadTaskEntryPoint, CameraUploadTaskName,
                    new TimeTrigger(CameraUploadTaskTimeTrigger, false), null);
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error resetting CAMERA UPLOADS service", e);
            }
        }
    }
}
