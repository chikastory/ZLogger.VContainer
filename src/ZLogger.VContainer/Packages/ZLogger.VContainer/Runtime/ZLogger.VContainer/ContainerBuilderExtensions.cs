using System;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using Microsoft.Extensions.Logging;
using VContainer;

namespace ZLogger {
    public static class ContainerBuilderExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterZLoggerFactory(this IContainerBuilder builder) =>
            builder.RegisterInstance(UnityLoggerFactory.Create(logging => {
                logging.SetMinimumLevel(LogLevel.Trace);
#if UNITY_EDITOR
                logging.AddZLoggerUnityDebug(options => {
                    var prefixFormat = ZString.PrepareUtf8<LogLevel, string>("<color={1}><b>{0}</b></color> ");
                    options.PrefixFormatter = (writer, info) => {
                        switch (info) {
                            case { LogLevel: LogLevel.Trace }:
                            case { LogLevel: LogLevel.Debug }:
                            case { LogLevel: LogLevel.Information }:
                                prefixFormat.FormatTo(ref writer, info.LogLevel, "green");
                                break;
                            case { LogLevel: LogLevel.Warning }:
                            case { LogLevel: LogLevel.Critical }:
                                prefixFormat.FormatTo(ref writer, info.LogLevel, "yellow");
                                break;
                            case { LogLevel: LogLevel.Error }:
                                prefixFormat.FormatTo(ref writer, info.LogLevel, "red");
                                break;
                        }
                    };
                });
                logging.AddZLoggerFile("Logs/ZLogger.log", "file-plain",
                    x => {
                        var prefixFormat = ZString.PrepareUtf8<LogLevel, DateTime>("[{1}] [{0}] ");
                        x.PrefixFormatter = (writer, info) =>
                            prefixFormat.FormatTo(ref writer, info.LogLevel, info.Timestamp.ToLocalTime().DateTime);
                    });
                logging.AddZLoggerRollingFile((dt, x) => $"Logs/ZLogger-{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log",
                    x => x.ToLocalTime().Date, 1024);
#else
                logging.AddZLoggerUnityDebug();
#endif
            }));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRegisterZLoggerFactory(this IContainerBuilder builder) {
            if (builder.Exists(typeof(ILoggerFactory))) return false;
            builder.RegisterZLoggerFactory();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterZLogger(this IContainerBuilder builder) {
            builder.TryRegisterZLoggerFactory();
            return builder.Register(resolver => resolver.Resolve<ILoggerFactory>().CreateLogger("Global"),
                Lifetime.Singleton);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRegisterZLogger(this IContainerBuilder builder) {
            if (builder.Exists(typeof(ILogger))) return false;
            builder.RegisterZLogger();
            return true;
        }
    }
}
