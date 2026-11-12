using System;
using System.IO;
using log4net;
using log4net.Config;

namespace Bastion.Client;

public static class Program
{
    private const string LogConfigurationFileName = "log4net.config";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

    public static void Main()
    {
        ConfigureLogging();
        AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;

        using var game = new BastionGame();
        game.Run();
    }

    private static void ConfigureLogging()
    {
        var configurationFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFileName));
        XmlConfigurator.Configure(LogManager.GetRepository(typeof(Program).Assembly), configurationFile);
    }

    private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        _logger.Fatal("The game stopped because of an unhandled exception.", exception);
    }
}
