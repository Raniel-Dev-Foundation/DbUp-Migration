using System.CommandLine;
using System.Diagnostics;
using DBMigrationWithDBUp;
using Microsoft.Extensions.Configuration;

internal class Program
{
    static int Main(string[] args)
    {
        var connectionStringOption = new Option<string>(name: "--connection-string", description: "Override connection string from config");
        connectionStringOption.AddAlias("-c");
        connectionStringOption.SetDefaultValueFactory(GetConnectionStringFromConfig);

        var forceEnsureDatabaseOption = new Option<bool>(name: "--force-ensure-database", description: "Create the database if it does not exists");
        forceEnsureDatabaseOption.AddAlias("-f");

        var mockDataOption = new Option<bool>(name: "--mock-data", description: "Create mock data for non-production systems");
        mockDataOption.AddAlias("-m");

        SetDefaultOptionsWhenDebugging(mockDataOption, forceEnsureDatabaseOption);

        var rootCommand = new RootCommand("Database Migraiton");
        rootCommand.AddOption(connectionStringOption);
        rootCommand.AddOption(forceEnsureDatabaseOption);
        rootCommand.AddOption(mockDataOption);

        rootCommand.SetHandler(DatabaseUpgrader.RunDbUpgradeActivities, connectionStringOption, forceEnsureDatabaseOption, mockDataOption);

        return rootCommand.Invoke(args);
    }

    private static string? GetConnectionStringFromConfig()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

        return configuration.GetConnectionString("ApplicationConnString_DBOwner");
    }

    [Conditional("DEBUG")]
    private static void SetDefaultOptionsWhenDebugging(Option mockDataOption, Option forceEnsureDatabaseOption)
    {
        // Sensible defaults while debugging that must be explict when deployed
        mockDataOption.SetDefaultValue(true);
        forceEnsureDatabaseOption.SetDefaultValue(true);
    }
}