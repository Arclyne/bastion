using System;
using System.IO;
using log4net;
using log4net.Config;

namespace Bastion.Client;

public static class Program
{
    private const string LogConfigurationFileName = "log4net.config";

    public static void Main()
    {
        ConfigureLogging();

        using var game = new BastionGame();
        game.Run();
    }

    private static void ConfigureLogging()
    {
        var configurationFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFileName));
        XmlConfigurator.Configure(LogManager.GetRepository(typeof(Program).Assembly), configurationFile);
    }
}
