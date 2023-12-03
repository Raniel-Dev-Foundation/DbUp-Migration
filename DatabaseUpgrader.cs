using DbUp;
using DbUp.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMigrationWithDBUp;

public static class DatabaseUpgrader
{
    public static void RunDbUpgradeActivities(string connectionString, bool forceEnsureDatabase, bool createMockData)
    {
        try
        {
            // Only try and create database if we're not running in the release pipeline (In Azure DB it is already created for us by Terraform)
            if (forceEnsureDatabase || Environment.GetEnvironmentVariable("TF_BUILD") == null)
            {
                EnsureDatabase.For.SqlDatabase(connectionString);
            }
            var sw = Stopwatch.StartNew();

            var mockResult = createMockData ? RunDbActivity("Mock Data Creation", () => Mocker.CreateMockData(connectionString)) : null;
            Console.WriteLine($"Mock Data Creation took {sw.Elapsed.TotalSeconds} seconds");
            sw.Restart();

            var dbScriptVersioner = new DatabaseScriptVersioner(connectionString);
            if (dbScriptVersioner.IsDatabaseAtCurrentScriptVersion())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Database up to date - skipping migration");
                return;
            }

            var migrationResult = RunDbActivity("Migration", () => Migrator.Migrate(connectionString));
            Console.WriteLine($"Migration took {sw.Elapsed.TotalSeconds} seconds");
            sw.Restart();


            if (migrationResult.Successful 
                && (!createMockData || mockResult is { Successful: true}))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success!");
                Console.ResetColor();

                dbScriptVersioner.UpdateScriptVersion();
            }
        }
        catch(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static DatabaseUpgradeResult RunDbActivity(string activityName, Func<DatabaseUpgradeResult> action)
    {
        var result = action();

        if (result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{activityName} complete");
            Console.ResetColor();
        }else
        {
            throw result.Error;
        }
        return result;
    }
}
