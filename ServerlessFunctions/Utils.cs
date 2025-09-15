using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using DTOs;
using Npgsql;

namespace ServerlessFunctions;

public static class Utils
{
    public static async Task<NpgsqlConnection> OpenConnectionToRds()
    {
        var connectionString = await BuildConnectionStringForRds();

        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        return await dataSource.OpenConnectionAsync();
    }

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
                Pooling = true,
                MaxPoolSize = 20,
            }.ConnectionString;
        return connectionString;
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

    private static string GetStringProperty(JsonDocument jsonDocument, string propertyName)
    {
        return jsonDocument.RootElement.GetProperty(propertyName).GetString()!;
    }

    private static int GetIntProperty(JsonDocument jsonDocument, string propertyName)
    {
        return jsonDocument.RootElement.GetProperty(propertyName).GetInt32()!;
    }

    public static async Task<PointsBalanceDto> GetPointsBalanceFromDatabase(NpgsqlConnection connection)
    {
        const string sql = @"SELECT * FROM ""Period""";
        await using var command = new NpgsqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new ArgumentException("There is no row");

        if (reader.Rows > 1)
            throw new ArgumentException("There are more than a single row");

        var points = reader.GetInt32(1);
        var idealPoints = reader.GetInt32(2);

        return new PointsBalanceDto(points, idealPoints);
    }
}