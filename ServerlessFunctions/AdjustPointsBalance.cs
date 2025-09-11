using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.Lambda.Core;
using DTOs;
using Npgsql;
using static ServerlessFunctions.Utils;

namespace ServerlessFunctions;

public class AdjustPointsBalance
{
    public async Task FunctionHandler(PointsBalanceDto pointsBalance, ILambdaContext context)
    {
        try
        {
            var connection = await OpenConnectionToRds();

            const string sql = @"
                UPDATE ""Period"" 
                SET points = @points, ideal_points = @ideal_points
                WHERE id = 1";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@points", pointsBalance.Points);
            command.Parameters.AddWithValue("@ideal_points", pointsBalance.IdealPoints);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            Console.WriteLine($"New points: {pointsBalance.Points} | New ideal points: {pointsBalance.IdealPoints}" );
            Console.WriteLine($"Rows affected: {rowsAffected}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}