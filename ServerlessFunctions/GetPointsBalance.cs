using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.Lambda.Core;
using DTOs;
using Npgsql;
using static ServerlessFunctions.Utils;

namespace ServerlessFunctions;

public class GetPointsBalance
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<PointsBalanceDto?> FunctionHandler(
        JsonNode input,
        ILambdaContext context)
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

            return new PointsBalanceDto(Points: points, IdealPoints: idealPoints);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return null;
    }
}