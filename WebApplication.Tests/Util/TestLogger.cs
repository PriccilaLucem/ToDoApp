using System;
using System.IO;
using Xunit.Abstractions;

namespace WebApplication.WebApplication.Tests.Util
{
    public class TestLogger
    {
        private readonly ITestOutputHelper _output;
        private readonly string _logDirectory;

        public TestLogger(ITestOutputHelper output)
        {
            _output = output;
            var projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            _logDirectory = Path.Combine(projectDirectory, "logs", "test");

            Directory.CreateDirectory(_logDirectory);
            _output.WriteLine($"TestLogger instantiated at {_logDirectory}");

        }

        public void LogInfo(string message)
        {
            var logMessage = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            _output.WriteLine(logMessage);
            WriteToFile(logMessage);
        }
        public void LogError(string message, Exception ex)
        {
            var logMessage = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            if (ex != null)
            {
                logMessage += $"\nException: {ex}";
            }
            
            _output.WriteLine(logMessage);
            WriteToFile(logMessage, "error");
        }


        private void WriteToFile(string message, string type = "info")
        {
            try
            {
                var fileName = $"test_{DateTime.Now:yyyyMMdd}_{type}.log";
                var filePath = Path.Combine(_logDirectory, fileName);
                File.AppendAllText(filePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to write to log file: {ex}");
            }
        }
    }
}