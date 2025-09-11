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
        var newPoints = pointsBalance.Points;
        var newIdealPoints = pointsBalance.IdealPoints;
        
        try
        {
            var connection = await OpenConnectionToRds();

            const string sql = @"
                UPDATE ""Period"" 
                SET points = @points, ideal_points = @ideal_points
                WHERE id = 1";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@points", newPoints);
            command.Parameters.AddWithValue("@ideal_points", newIdealPoints);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            Console.WriteLine($"New points: {newPoints} | New ideal points: {newIdealPoints}" );
            Console.WriteLine($"Rows affected: {rowsAffected}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}