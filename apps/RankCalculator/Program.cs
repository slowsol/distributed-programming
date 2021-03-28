using System;
using RepositoryLibrary;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace RankCalculator
{
    public static class ApplicationLogging
    {
        public static ILoggerFactory Factory { get; } = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug);
        });

        public static ILogger<T> CreateLogger<T>() => Factory.CreateLogger<T>();
    }

    public class Program
    {
        static ILogger<Program> ProgramLogger { get; } = ApplicationLogging.CreateLogger<Program>();
        static ILogger<RankCalculator> RankCalculatorLogger { get; } = ApplicationLogging.CreateLogger<RankCalculator>();
        static ILogger<IRepository> RepositoryLogger { get; } = ApplicationLogging.CreateLogger<IRepository>();

        public static void Main(string[] args)
        {
            ProgramLogger.LogDebug("Start Calculating.");

            var calculator = new RankCalculator(RankCalculatorLogger, new RedisRepository(RepositoryLogger));

            calculator.Calculate();

            ProgramLogger.LogDebug("Finish Calculating.");
        }
    }
}
