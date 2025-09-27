using System.Text.Json.Nodes;
using Amazon.Lambda.Core;
using DTOs;
using Npgsql;
using static ServerlessFunctions.Utils;

namespace ServerlessFunctions;

public class GetActivity
{
    public async Task<ActivityDto?> FunctionHandler(
        JsonNode input,
        ILambdaContext context)
    {
        NpgsqlConnection? connection = null;

        try
        {
            connection = await OpenConnectionToRds();

            return await GetActivityFromDatabase(connection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await connection?.CloseAsync()!;
        }

        return null;
    }

    private async Task<ActivityDto?> GetActivityFromDatabase(NpgsqlConnection connection)
    {
        const string sql = @"SELECT * FROM ""Activity"" WHERE period_id = 1";
        await using var command = new NpgsqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync();

        List<EventDto> events = [];
        while (await reader.ReadAsync())
            events.Add(new EventDto(
                content: reader.GetString(2),
                occurredAt: reader.GetDateTime(1)));

        return new ActivityDto(events);
    }
}