using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using System.Reflection;

namespace DBMigrationWithDBUp;

internal static class Seeder
{
    public static DatabaseUpgradeResult Seed(string connectionString)
    {
        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithExecutionTimeout(TimeSpan.FromMinutes(5))
            .WithScriptsAndCodeEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.StartsWith("DBMigrationWithDBUp.Seed"))
            .JournalTo(new NullJournal())
            .LogToConsole()
            .LogScriptOutput()
            .Build();

        return upgrader.PerformUpgrade();
    }
}
