using System.Text.Json.Nodes;
using Amazon.Lambda.Core;
using DTOs;
using Npgsql;
using static ServerlessFunctions.Utils;

namespace ServerlessFunctions;

public class GetPointsBalance
{
    public async Task<PointsBalanceDto?> FunctionHandler(
        JsonNode input,
        ILambdaContext context)
    {
        NpgsqlConnection? connection = null;
        
        try
        {
            connection = await OpenConnectionToRds();

            return await GetPointsBalanceFromDatabase(connection);
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
}