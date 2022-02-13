using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal.JobProcessorWorker
{
    /// <summary>
    /// Extension method class containing extensions related to logging for the server project.
    /// </summary>
    internal static class LoggingExtensions
    {
        /// <summary>
        /// Logs a debug message for a given server context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="serverContext">The server context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Any arguments to log and interpolate into the message string.</param>
        public static void LogDebug(
            this ILogger logger, 
            ServerContext serverContext, 
            string message, 
            params object[] args
        )
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }
            
            logger.LogInternal(serverContext, LogLevel.Debug, message, null, args);
        }

        /// <summary>
        /// Logs a debug message for a given worker context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="workerContext">The worker context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Any arguments to log and interpolate into the message string.</param>
        public static void LogDebug(
            this ILogger logger, 
            WorkerContext workerContext, 
            string message,
            params object[] args
        )
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }
            
            logger.LogInternal(workerContext, LogLevel.Debug, message, null, args);
        }

        /// <summary>
        /// Logs a debug message for a given job context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="jobContext">The job context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Any arguments to log and interpolate into the message string.</param>
        public static void LogDebug(
            this ILogger logger, 
            JobContext jobContext,
            string message,
            params object[] args
        )
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }
            
            logger.LogInternal(jobContext, LogLevel.Debug, message, null, args);
        }

        /// <summary>
        /// Logs an information message for a given server context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="serverContext">The server context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Any arguments to log and interpolate into the message string.</param>
        public static void LogInformation(
            this ILogger logger, 
            ServerContext serverContext, 
            string message, 
            params object[] args
        )
        {
            if (!logger.IsEnabled(LogLevel.Information))
            {
                return;
            }

            logger.LogInternal(serverContext, LogLevel.Information, message, null, args);
        }

        /// <summary>
        /// Logs an information message for a given worker context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="workerContext">The worker context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Any arguments to log and interpolate into the message string.</param>
        public static void LogInformation(
            this ILogger logger, 
            WorkerContext workerContext, 
            string message, 
            params object[] args
        )
        {
            if (!logger.IsEnabled(LogLevel.Information))
            {
                return;
            }

            logger.LogInternal(workerContext, LogLevel.Information, message, null, args);
        }

        /// <summary>
        /// Logs an information message for a given job context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="jobContext">The job context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Any arguments to log and interpolate into the message string.</param>
        public static void LogInformation(
            this ILogger logger, 
            JobContext jobContext, 
            string message,
            params object[] args
        )
        {
            if (!logger.IsEnabled(LogLevel.Information))
            {
                return;
            }

            logger.LogInternal(jobContext, LogLevel.Information, message, null, args);
        }

        /// <summary>
        /// Logs an error message for a given server context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="serverContext">The server context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An exception to log, if any.</param>
        /// <param name="args">Any arguments to log and interpolate into the message string.</param>
        public static void LogError(
            this ILogger logger,
            ServerContext serverContext,
            string message,
            Exception? exception = null,
            params object[] args
        )
        {
            if (!logger.IsEnabled(LogLevel.Error))
            {
                return;
            }

            logger.LogInternal(serverContext, LogLevel.Error, message, exception, args);
        }

        /// <summary>
        /// Logs an error message for a given worker context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="workerContext">The worker context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An exception to log, if any.</param>
        /// <param name="args">Any arguments to log and interpolate into the message string.</param>
        public static void LogError(
            this ILogger logger,
            WorkerContext workerContext,
            string message,
            Exception? exception = null,
            params object[] args
        )
        {
            if (!logger.IsEnabled(LogLevel.Error))
            {
                return;
            }

            logger.LogInternal(
                workerContext,
                LogLevel.Error,
                message,
                exception,
                args
            );
        }

        /// <summary>
        /// Logs an error message for a given job context.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="jobContext">The job context to use to add information to the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An exception to log, if any.</param>
        /// <param name="args">Any arguments to log and interpolate into the message string.</param>
        public static void LogError(
            this ILogger logger,
            JobContext jobContext,
            string message,
            Exception? exception = null,
            params object[] args
        )
        {
            if (!logger.IsEnabled(LogLevel.Error))
            {
                return;
            }

            logger.LogInternal(
                jobContext,
                LogLevel.Error,
                message,
                exception,
                args
            );
        }

        private static void LogInternal(
            this ILogger logger,
            ServerContext serverContext,
            LogLevel logLevel,
            string message,
            Exception? exception = null,
            params object[] args
        )
        {
            logger.Log(
                logLevel,
                exception,
                "s:{serverId} - " + message,
                new object[] { serverContext.ServerInstance.Id }
                    .Concat(args)
                    .ToArray()
            );
        }

        private static void LogInternal(
            this ILogger logger,
            WorkerContext workerContext,
            LogLevel logLevel,
            string message,
            Exception? exception = null,
            params object[] args
        )
        {
            logger.Log(
                logLevel,
                exception,
                // ReSharper disable once StructuredMessageTemplateProblem
                "s:{serverId} w:{workerId} - " + message,
                new object[] { workerContext.ServerContext.ServerInstance.Id, workerContext.Worker.Id }
                    .Concat(args)
                    .ToArray()
            );
        }

        private static void LogInternal(
            this ILogger logger,
            JobContext jobContext,
            LogLevel logLevel,
            string message,
            Exception? exception = null,
            params object[] args
        )
        {
            logger.Log(
                logLevel,
                exception,
                // ReSharper disable StructuredMessageTemplateProblem
                "s:{serverId} w:{workerId} j:{jobId} - " + message,
                new object[] { jobContext.WorkerContext.ServerContext.ServerInstance.Id, jobContext.WorkerContext.Worker.Id, jobContext.JobDefinition.Id }
                    .Concat(args)
                    .ToArray()
            );
        }
    }
}