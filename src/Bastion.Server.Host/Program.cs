using System;
using System.IO;
using CoreWCF.Configuration;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bastion.Server.Adapters;

namespace Bastion.Server.Host;

public static class Program
{
    private const string ConnectionStringName = "Bastion";
    private const string NetTcpPortKey = "Server:NetTcpPort";
    private const string LogConfigurationFileName = "log4net.config";
    private const string ServiceModelConfigurationFileName = "ServiceModel.config";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

    public static void Main(string[] args)
    {
        ConfigureLogging();
        var options = new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory };
        WebApplicationBuilder builder = WebApplication.CreateBuilder(options);
        builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);

        string connectionString = GetConnectionString(builder.Configuration);
        int netTcpPort = GetNetTcpPort(builder.Configuration);
        var certificate = ServerCertificate.Load(builder.Configuration);

        builder.Logging.ClearProviders();
        builder.Logging.AddLog4Net(new Log4NetProviderOptions { ExternalConfigurationSetup = true });
        builder.WebHost.UseNetTcp(netTcpPort);
        builder.Services.AddServiceModelServices();
        builder.Services.AddServiceModelConfigurationManagerFile(
            Path.Combine(AppContext.BaseDirectory, ServiceModelConfigurationFileName));
        builder.Services.AddBastionAdapters(connectionString);
        builder.Services.AddBastionApplication();
        builder.Services.AddBastionServiceModel();

        WebApplication app = builder.Build();
        app.UseServiceModel(certificate.ConfigureServices);
        _logger.Info($"Game server started. NetTcpPort={netTcpPort}");
        app.Run();
    }

    private static void ConfigureLogging()
    {
        var configurationFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFileName));
        XmlConfigurator.Configure(LogManager.GetRepository(typeof(Program).Assembly), configurationFile);
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"The connection string is not configured. ConnectionStringName={ConnectionStringName}");
        }

        return connectionString;
    }

    private static int GetNetTcpPort(IConfiguration configuration)
    {
        int netTcpPort = configuration.GetValue<int>(NetTcpPortKey);
        if (netTcpPort <= 0)
        {
            throw new InvalidOperationException($"The net.tcp port is not configured. Key={NetTcpPortKey}");
        }

        return netTcpPort;
    }
}
