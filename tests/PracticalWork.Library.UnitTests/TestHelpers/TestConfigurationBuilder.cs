using Microsoft.Extensions.Configuration;

namespace PracticalWork.Library.UnitTests.TestHelpers;

internal static class TestConfigurationBuilder
{
    public static IConfiguration Build(Dictionary<string, string?>? values = null)
    {
        var defaults = new Dictionary<string, string?>
        {
            ["App:Redis:Books:VersionKey"] = "books:cache",
            ["App:Redis:Books:BooksList:Prefix"] = "books:list",
            ["App:Redis:Books:BooksList:TtlInMinutes"] = "10",
            ["App:Redis:Books:LibraryBooks:Prefix"] = "library:books",
            ["App:Redis:Books:LibraryBooks:TtlInMinutes"] = "5",
            ["App:Redis:Reports:VersionKey"] = "reports:cache",
            ["App:Redis:Reports:ReportsList:Prefix"] = "reports:list",
            ["App:Redis:Reports:ReportsList:TtlInMinutes"] = "1440",
            ["App:RabbitMQ:Reports:Exchange"] = "reports.exchange",
            ["App:RabbitMQ:Reports:RoutingKey"] = "reports.create.key",
        };

        if (values != null)
        {
            foreach (var kv in values)
                defaults[kv.Key] = kv.Value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(defaults)
            .Build();
    }
}
