using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBMigrationWithDBUp;

internal static class Mocker
{
    public static DatabaseUpgradeResult CreateMockData(string connectionString)
    {
        var runtimeEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT");
        if (string.IsNullOrWhiteSpace(runtimeEnvironment))
        {
            // If no environment is set then return successful noop
            Console.WriteLine("No environment detected for mock data.");
            return new DatabaseUpgradeResult(Enumerable.Empty<SqlScript>(), true, null, null);
        }

        var environments = new List<string> { "Common" }; // Always run Common mock scripts
        if (!environments.Any(e => e.ToLowerInvariant().Equals(runtimeEnvironment.ToLowerInvariant())))
        {
            // Apply mock overrides per environment
            environments.Add(runtimeEnvironment);
        }

        Console.WriteLine($"Executing mock scrpts for '{runtimeEnvironment}' environment");

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsAndCodeEmbeddedInAssembly(Assembly.GetExecutingAssembly(), (s => ShouldRunScript(s, environments)))
            .JournalTo(new NullJournal())
            .LogToConsole()
            .Build();
        return upgrader.PerformUpgrade();
    }

    private static bool ShouldRunScript (string script, IEnumerable<string> environments)
    {
        var mockFolders = environments.Select(e => $"DBMigrationWithDBUp.Mock.{e}");
        return mockFolders.Any(f => script.StartsWith(f, StringComparison.OrdinalIgnoreCase));
    }
}
