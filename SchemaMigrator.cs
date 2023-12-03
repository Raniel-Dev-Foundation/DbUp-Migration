using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using System.Reflection;
namespace DBMigrationWithDBUp;

internal static class SchemaMigrator
{
    public static DatabaseUpgradeResult Migrate (string connectionString)
    {
        const string dropProgrammabilityObjectsScript = "DBMigrationWithDBUp.SchemaScripts.DropProgrammabilityObjects.sql";
        const string sharedFolder = "DBMigrationWithDBUp.SchemaScripts.Shared";
        const string storedProceduresFolder = "DBMigrationWithDBUp.SchemaScripts.StoredProcedures";
        const string viewsFolder = "DBMigrationWithDBUp.SchemaScripts.Views";
        const string tvfFolder = "DBMigrationWithDBUp.SchemaScripts.TVFs";

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .JournalTo(new NullJournal())

            // Order is important - there can be dependecies between objects
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
                s => s == dropProgrammabilityObjectsScript,
                new SqlScriptOptions { RunGroupOrder = 0 })
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
                s => s == sharedFolder,
                new SqlScriptOptions { RunGroupOrder = 1 })
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
                s => s == tvfFolder,
                new SqlScriptOptions { RunGroupOrder = 2 })
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
                s => s == viewsFolder,
                new SqlScriptOptions { RunGroupOrder = 2 })
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
                s => s == storedProceduresFolder,
                new SqlScriptOptions { RunGroupOrder = 3 })
            .LogToConsole()
            .Build();

        return upgrader.PerformUpgrade();
    }
}
