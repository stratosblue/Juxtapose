using System;
using System.IO;

using Juxtapose.SourceGenerator;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Serilog;
using Serilog.Extensions.Logging;

namespace Juxtapose.Test
{
    [IllusionClass(typeof(IGreeter), typeof(Greeter), "Juxtapose.Test.GreeterAsIGreeterIllusion")]
    [IllusionStaticClass(typeof(StaticGreeter), "Juxtapose.Test.StaticGreeterIllusion")]
    public partial class GreeterJuxtaposeContext : JuxtaposeContext
    {
        #region Public 构造函数

        public GreeterJuxtaposeContext()
        {
            FastSetConsoleLoggerFactory(LogLevel.Trace);
        }

        #endregion Public 构造函数

        #region Protected 方法

        protected override IExternalProcessActivator CreateExternalProcessActivator()
        {
            return LocalExternalProcessActivator.FastCreate("Juxtapose.Test.RunningHost");
        }

        #endregion Protected 方法

        #region Public 方法

        public ILoggerFactory UnSetConsoleLoggerFactory()
        {
            return LoggerFactory = NullLoggerFactory.Instance;
        }

        public ILoggerFactory FastSetConsoleLoggerFactory(LogLevel logLevel)
        {
            var logger = new LoggerConfiguration().WriteTo.Async(ic => ic.Console((Serilog.Events.LogEventLevel)logLevel, "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
                                                  .WriteTo.Debug((Serilog.Events.LogEventLevel)logLevel)
                                                  .MinimumLevel.Verbose()
                                                  .CreateLogger();

            return LoggerFactory = new SerilogLoggerFactory(logger);
        }

        public ILoggerFactory FastSetFileLoggerFactory(LogLevel logLevel)
        {
            var logger = new LoggerConfiguration().WriteTo.Async(ic => ic.File(Path.Combine(Environment.CurrentDirectory, "logs", $"{DateTimeOffset.Now.ToUnixTimeSeconds()}-{Environment.ProcessId}.log"), (Serilog.Events.LogEventLevel)logLevel))
                                                  .WriteTo.Async(ic => ic.Console((Serilog.Events.LogEventLevel)logLevel, "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
                                                  .WriteTo.Debug((Serilog.Events.LogEventLevel)logLevel)
                                                  .MinimumLevel.Verbose()
                                                  .CreateLogger();

            return LoggerFactory = new SerilogLoggerFactory(logger);
        }

        #endregion Public 方法
    }
}