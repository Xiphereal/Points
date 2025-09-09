using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Npgsql;

namespace ServerlessFunctions;

public static class Utils
{
    public static async Task<string> BuildConnectionStringForRds()
    {
        var rdsSecretAsJson = JsonDocument.Parse(await GetRdsSecret());
        var connectionString =
            new NpgsqlConnectionStringBuilder
            {
                Host = GetStringProperty(rdsSecretAsJson, "host"),
                Port = GetIntProperty(rdsSecretAsJson, "port"),
                Database = GetStringProperty(rdsSecretAsJson, "dbInstanceIdentifier"),
                Username = GetStringProperty(rdsSecretAsJson, "username"),
                Password = GetStringProperty(rdsSecretAsJson, "password"),
                SslMode = SslMode.Require,
            }.ConnectionString;
        return connectionString;
    }

    public static async Task<string> GetRdsSecret()
    {
        var secretName = Environment.GetEnvironmentVariable("RDS_SECRET_NAME");
        var region = Environment.GetEnvironmentVariable("AWS_REGION");

        var client =
            new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));
        var secret = await client.GetSecretValueAsync(
            new GetSecretValueRequest
            {
                SecretId = secretName
            });

        return secret.SecretString;
    }

    public static string GetStringProperty(JsonDocument jsonDocument, string propertyName)
    {
        return jsonDocument.RootElement.GetProperty(propertyName).GetString()!;
    }

    public static int GetIntProperty(JsonDocument jsonDocument, string propertyName)
    {
        return jsonDocument.RootElement.GetProperty(propertyName).GetInt32()!;
    }
}