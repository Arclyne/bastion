using System;
using System.IO;
using Bastion.Contracts;
using Bastion.Server.Adapters;
using CoreWCF.Configuration;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        ArgumentNullException.ThrowIfNull(args);

        ConfigureLogging();
        AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
        var options = new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        };
        WebApplicationBuilder builder = WebApplication.CreateBuilder(options);
        builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);

        string connectionString = GetConnectionString(builder.Configuration);
        int netTcpPort = GetNetTcpPort(builder.Configuration);
        using var certificate = ServerCertificate.Load(builder.Configuration);

        builder.Logging.ClearProviders();
        var log4NetOptions = new Log4NetProviderOptions
        {
            ExternalConfigurationSetup = true,
        };
        builder.Logging.AddLog4Net(log4NetOptions);
        builder.WebHost.UseNetTcp(netTcpPort);
        builder.Services.AddServiceModelServices();
        builder.Services.AddServiceModelConfigurationManagerFile(
            Path.Combine(AppContext.BaseDirectory, ServiceModelConfigurationFileName));
        builder.Services.AddBastionAdapters(connectionString);
        builder.Services.AddBastionApplication();
        builder.Services.AddBastionHost();

        using WebApplication app = builder.Build();
        app.UseServiceModel(serviceBuilder => ConfigureServiceModel(serviceBuilder, certificate));
        app.Start();
        _logger.Info($"Game server started. NetTcpPort={netTcpPort}");
        app.WaitForShutdown();
    }

    private static void ConfigureLogging()
    {
        var configurationFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFileName));
        XmlConfigurator.Configure(LogManager.GetRepository(typeof(Program).Assembly), configurationFile);
    }

    private static void ConfigureServiceModel(IServiceBuilder serviceBuilder, ServerCertificate certificate)
    {
        serviceBuilder.AddBastionServices();
        certificate.ApplyTo(serviceBuilder);
    }

    private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        _logger.Fatal("The game server stopped because of an unhandled exception.", exception);
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
        if (!ServiceEndpoints.IsRegisteredPort(netTcpPort))
        {
            throw new InvalidOperationException(
                $"The net.tcp port is missing or is not a registered port. Key={NetTcpPortKey}; Port={netTcpPort}; "
                + $"FirstPort={ServiceEndpoints.FirstRegisteredPort}; LastPort={ServiceEndpoints.LastRegisteredPort}");
        }

        return netTcpPort;
    }
}
