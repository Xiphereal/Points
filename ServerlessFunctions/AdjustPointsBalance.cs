using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Npgsql;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET
// class.
[assembly:
    LambdaSerializer(
        typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ServerlessFunctions;

public class AdjustPointsBalance
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(JsonNode input, ILambdaContext context)
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

        try
        {
            var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
            var connection = await dataSource.OpenConnectionAsync();

            const string sql = @"SELECT * FROM ""Period""";
            await using var command = new NpgsqlCommand(sql, connection);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                throw new ArgumentException("There is no row");

            if (reader.Rows > 1)
                throw new ArgumentException("There are more than a single row");

            var points = reader.GetInt32(1);
            var idealPoints = reader.GetInt32(2);
            Console.WriteLine($"{points}/{idealPoints}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static string GetStringProperty(JsonDocument jsonDocument, string propertyName)
    {
        return jsonDocument.RootElement.GetProperty(propertyName).GetString()!;
    }

    private static int GetIntProperty(JsonDocument jsonDocument, string propertyName)
    {
        return jsonDocument.RootElement.GetProperty(propertyName).GetInt32()!;
    }

    private static async Task<string> GetRdsSecret()
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
}