using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DTOs;
using Npgsql;
using static ServerlessFunctions.Utils;

namespace ServerlessFunctions;

public class AdjustPointsBalance
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        PointsBalanceDto? pointsBalance,
        ILambdaContext context)
    {
        if (ValidateBody(pointsBalance, out var response))
            return response!;

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

            Console.WriteLine(
                $"New points: {pointsBalance.Points} | " +
                $"New ideal points: {pointsBalance.IdealPoints}");
            Console.WriteLine($"Rows affected: {rowsAffected}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");

            return new APIGatewayProxyResponse
            {
                StatusCode = 500
            };
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 204
        };
    }

    private static bool ValidateBody(
        PointsBalanceDto? pointsBalance,
        out APIGatewayProxyResponse? response)
    {
        if (pointsBalance is null)
        {
            const string pointsBalanceIsNull =
                """
                The points balance (body) is null. Expected body:
                {
                    "points": <number>,
                    "idealPoints": <number>
                }
                """;
            Console.WriteLine(pointsBalanceIsNull);

            response = new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = pointsBalanceIsNull
            };

            return true;
        }

        if (!pointsBalance.Points.HasValue && !pointsBalance.IdealPoints.HasValue)
        {
            const string pointsBalanceIsEmpty =
                "The points balance is completely empty. " +
                "At least 'points' or 'idealPoints' must have a value.";
            Console.WriteLine(pointsBalanceIsEmpty);

            response = new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = pointsBalanceIsEmpty,
            };

            return true;
        }

        response = null;

        return false;
    }
}