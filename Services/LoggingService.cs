using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SysMax2._1.Models;

namespace SysMax2._1.Services
{
    /// <summary>
    /// Service for logging application events and errors
    /// </summary>
    public class LoggingService
    {
        private static LoggingService? _instance;
        private readonly SemaphoreSlim _logLock = new SemaphoreSlim(1, 1);
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();
        private readonly string _logFilePath;
        private const int MaxInMemoryLogs = 1000;
        private bool _isLoggingEnabled = true;

        // Public property to control logging enable state
        public bool IsLoggingEnabled
        {
            get => _isLoggingEnabled;
            set
            {
                if (_isLoggingEnabled != value)
                {
                    _isLoggingEnabled = value;
                    // Log the change itself (if logging is still enabled!)
                    Log(LogLevel.Info, $"Logging has been {(value ? "enabled" : "disabled")}.");
                }
            }
        }

        // Event for when a new log entry is added
        public event EventHandler<LogEntry>? LogAdded;

        /// <summary>
        /// Creates a new instance of the LoggingService
        /// </summary>
        private LoggingService()
        {
            // Create logs directory if it doesn't exist
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SysMax");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            // Set log file path
            _logFilePath = Path.Combine(appDataPath, "SysMax.log");

            // Load initial logging state from settings? Or default to true?
            // Assuming default is true for now.
            IsLoggingEnabled = true; // Set initial state

            // Log service initialization (only if enabled)
            Log(LogLevel.Info, "Logging service initialized");
        }

        /// <summary>
        /// Gets the singleton instance of the LoggingService
        /// </summary>
        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoggingService();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Logs a message with the specified level
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Log message</param>
        /// <param name="source">Log source</param>
        public void Log(LogLevel level, string message, string source = "SysMax")
        {
            // Check if logging is enabled globally before proceeding
            if (!IsLoggingEnabled && level != LogLevel.Critical) // Always log Critical? Or respect setting fully?
            {
                // Optionally log to Debug console even if file logging is off
                System.Diagnostics.Debug.WriteLine($"[LOG DISABLED] {level}: {message} ({source})");
                return; // Skip logging if disabled (except maybe critical)
            }

            LogEntry entry = new LogEntry(level, message, source);
            Task.Run(() => AddLogEntryAsync(entry));
        }

        /// <summary>
        /// Adds a log entry asynchronously
        /// </summary>
        /// <param name="entry">Log entry to add</param>
        private async Task AddLogEntryAsync(LogEntry entry)
        {
            await _logLock.WaitAsync();

            try
            {
                // Add to in-memory collection (always? or only if enabled?)
                _logEntries.Add(entry);
                if (_logEntries.Count > MaxInMemoryLogs)
                {
                    _logEntries.RemoveRange(0, _logEntries.Count - MaxInMemoryLogs);
                }

                // Write to log file ONLY IF ENABLED
                // This check is redundant now if Log() checks first, but good for safety.
                if (IsLoggingEnabled)
                {
                    await WriteToFileAsync(entry);
                }

                // Raise event (always? or only if enabled?)
                LogAdded?.Invoke(this, entry);
            }
            finally
            {
                _logLock.Release();
            }
        }

        /// <summary>
        /// Writes a log entry to the log file
        /// </summary>
        /// <param name="entry">Log entry to write</param>
        private async Task WriteToFileAsync(LogEntry entry)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, true))
                {
                    await writer.WriteLineAsync(entry.ToString());
                }
            }
            catch
            {
                // Silently fail if we can't write to the log file
                // We don't want logging failures to crash the application
            }
        }

        /// <summary>
        /// Gets all log entries
        /// </summary>
        /// <returns>Collection of log entries</returns>
        public List<LogEntry> GetLogEntries()
        {
            lock (_logEntries)
            {
                return new List<LogEntry>(_logEntries);
            }
        }

        /// <summary>
        /// Gets log entries filtered by level
        /// </summary>
        /// <param name="level">Log level to filter by</param>
        /// <returns>Collection of filtered log entries</returns>
        public List<LogEntry> GetLogEntries(LogLevel level)
        {
            lock (_logEntries)
            {
                return _logEntries.FindAll(e => e.Level == level);
            }
        }

        /// <summary>
        /// Gets log entries from a specific source
        /// </summary>
        /// <param name="source">Source to filter by</param>
        /// <returns>Collection of filtered log entries</returns>
        public List<LogEntry> GetLogEntriesFromSource(string source)
        {
            lock (_logEntries)
            {
                return _logEntries.FindAll(e => e.Source == source);
            }
        }

        /// <summary>
        /// Clears all in-memory log entries
        /// </summary>
        public void ClearLogs()
        {
            lock (_logEntries)
            {
                _logEntries.Clear();
            }
        }
    }
}