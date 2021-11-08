using System;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// Extension method class containing extensions related to logging for the server project.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Logs a debug message for a given server context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="serverContext">The server context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        public static void LogDebug(this ILogger logger, ServerContext serverContext, string message)
        {
            logger.LogInternal(serverContext, LogLevel.Debug, message);
        }
        
        /// <summary>
        /// Logs a debug message for a given worker context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="workerContext">The worker context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        public static void LogDebug(this ILogger logger, WorkerContext workerContext, string message)
        {
            logger.LogInternal(workerContext, LogLevel.Debug, message);
        }
        
        /// <summary>
        /// Logs a debug message for a given job context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="jobContext">The job context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        public static void LogDebug(this ILogger logger, JobContext jobContext, string message)
        {
            logger.LogInternal(jobContext, LogLevel.Debug, message);
        }
        
        /// <summary>
        /// Logs an information message for a given server context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="serverContext">The server context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        public static void LogInformation(this ILogger logger, ServerContext serverContext, string message)
        {
            logger.LogInternal(serverContext, LogLevel.Information, message);
        }
        
        /// <summary>
        /// Logs an information message for a given worker context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="workerContext">The worker context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        public static void LogInformation(this ILogger logger, WorkerContext workerContext, string message)
        {
            logger.LogInternal(workerContext, LogLevel.Information, message);
        }
        
        /// <summary>
        /// Logs an information message for a given job context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="jobContext">The job context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        public static void LogInformation(this ILogger logger, JobContext jobContext, string message)
        {
            logger.LogInternal(jobContext, LogLevel.Information, message);
        }

        /// <summary>
        /// Logs an error message for a given server context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="serverContext">The server context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An exception to log, if any.</param>
        public static void LogError(
            this ILogger logger, 
            ServerContext serverContext, 
            string message, 
            Exception exception = null
        )
        {
            logger.LogInternal(serverContext, LogLevel.Error, message, exception);
        }
        
        /// <summary>
        /// Logs an error message for a given worker context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="workerContext">The worker context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An exception to log, if any.</param>
        public static void LogError(
            this ILogger logger, 
            WorkerContext workerContext, 
            string message, 
            Exception exception = null
        )
        {
            logger.LogInternal(
                workerContext,
                LogLevel.Error,
                message, 
                exception
            );
        }
        
        /// <summary>
        /// Logs an error message for a given job context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="jobContext">The job context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An exception to log, if any.</param>
        public static void LogError(
            this ILogger logger, 
            JobContext jobContext, 
            string message, 
            Exception exception = null
        )
        {
            logger.LogInternal(
                jobContext,
                LogLevel.Error,
                message,
                exception
            );
        }

        private static void LogInternal(
            this ILogger logger, 
            ServerContext serverContext, 
            LogLevel logLevel, 
            string message,
            Exception exception = null
        )
        {
            logger.Log(
                logLevel, 
                $"s:{serverContext.ServerInstance.Id} - {message}", 
                exception
            );
        }

        private static void LogInternal(
            this ILogger logger, 
            WorkerContext workerContext, 
            LogLevel logLevel, 
            string message,
            Exception exception = null
        )
        {
            logger.Log(
                logLevel, 
                $"s:{workerContext.ServerContext.ServerInstance.Id} w:{workerContext.Worker.Id} - {message}", 
                exception
            );
        }

        private static void LogInternal(
            this ILogger logger, 
            JobContext jobContext, 
            LogLevel logLevel, 
            string message,
            Exception exception = null
        )
        {
            logger.Log(
                logLevel, 
                $"s:{jobContext.WorkerContext.ServerContext.ServerInstance.Id} w:{jobContext.WorkerContext.Worker.Id} j:{jobContext.JobDefinition.Id} - {message}{(exception != null ? $" - {exception.Message}" : "")}",
                exception
            );
        }
    }
}