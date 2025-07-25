using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using DockerHubBackend.Services.Interface;

public class LogService : BackgroundService, ILogService
{
    private static long lastReadPosition = 0; // offset
    private static readonly string logsDirectoryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Logs");
    private static Timer _timer;
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly Dictionary<string, string> LevelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "level:warning", "level:wrn" },
        { "level:error", "level:err" },
        { "level:information", "level:inf" },
    };

    private static readonly HashSet<string> LogicalOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        "and", "or", "not"
    };

    private static readonly HashSet<string> KnownFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "level", "message", "timestamp", "exception"
    };

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(AsyncProcessLogs, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        stoppingToken.Register(() => _timer.Dispose());

        return Task.CompletedTask;
    }

    private static string FindLogsDirectory()
    {
        if (!Directory.Exists(logsDirectoryPath))
        {
            Console.WriteLine($"Logs directory not found: {logsDirectoryPath}");
            return null;
        }

        return logsDirectoryPath;
    }

    private static string GetLatestLogFilePath()
    {
        string logsDirectory = FindLogsDirectory();
        if (logsDirectory == null)
        {
            return null;
        }

        var latestLogFile = Directory.GetFiles(logsDirectory, "log-*.log")
                                     .OrderByDescending(f => f)
                                     .FirstOrDefault();

        if (latestLogFile == null)
        {
            Console.WriteLine("No log files found.");
            return null;
        }

        return latestLogFile;
    }

    private static async void AsyncProcessLogs(object state)
    {
        try
        {
            var logFilePath = GetLatestLogFilePath();
            if (logFilePath == null)
            {
                return;
            }

            using (var stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.Seek(lastReadPosition, SeekOrigin.Begin);

                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        await SendLogToElasticsearch(line);
                    }

                    lastReadPosition = stream.Position;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing logs: {ex.Message}");
        }
    }

    private static async Task SendLogToElasticsearch(string logLine)
    {
        try
        {
            var logEntry = ParseLogLine(logLine);

            if (logEntry == null)
            {
                return;
            }

            var (timestamp, level, message) = logEntry.Value;

            var jsonLog = new
            {
                timestamp = timestamp.ToString("o"), // ISO8601 date format
                level = level,
                message = message
            };

            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(jsonLog),
                Encoding.UTF8,
                "application/json"
            );

            // Elasticsearch endpoint
            var elasticsearchUri = "http://localhost:9200/logstash-logs/_doc";

            var response = await _httpClient.PostAsync(elasticsearchUri, jsonContent);

            if (!response.IsSuccessStatusCode)
            { 
                Console.WriteLine($"Failed to send log to Elasticsearch. Status code: {response.StatusCode}");
                var responseBody = await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send log to Elasticsearch: {ex.Message}");
        }
    }

    private static (DateTime Timestamp, string Level, string Message)? ParseLogLine(string logLine)
    {
        try
        {
            var parts = logLine.Split(new[] { ' ' }, 5, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 5)
                return null;

            var timestamp = DateTime.Parse($"{parts[0]} {parts[1]}");

            var level = parts[3].Trim('[', ']');

            var message = parts[4];

            return (timestamp, level, message);
        }
        catch
        {
            return null;
        }
    }

    public string NormalizeQuery(string query)
    {
        query = SetLevel(query);
        var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var resultTokens = new List<string>();

        foreach (var token in tokens)
        {
            if (IsLogicalOperator(token))
            {
                resultTokens.Add(token.ToUpper());
            }
            else if (IsFieldToken(token, out var fieldName, out var fieldValue))
            {
                var normalizedField = KnownFields.Contains(fieldName)
                    ? fieldName.ToLower()
                    : fieldName;

                resultTokens.Add($"{normalizedField}:{fieldValue}");
            }
            else
            {
                resultTokens.Add(token);
            }
        }

        return string.Join(' ', resultTokens);
    }

    private static string SetLevel(string query)
    {
        foreach (var entry in LevelMap)
        {
            query = query.Replace(entry.Key, entry.Value, StringComparison.OrdinalIgnoreCase);
        }

        return query;
    }

    private static bool IsLogicalOperator(string token)
    {
        return LogicalOperators.Contains(token);
    }

    private static bool IsFieldToken(string token, out string fieldName, out string fieldValue)
    {
        var parts = token.Split(':', 2);
        if (parts.Length == 2)
        {
            fieldName = parts[0];
            fieldValue = parts[1];
            return true;
        }

        fieldName = string.Empty;
        fieldValue = string.Empty;
        return false;
    }
}