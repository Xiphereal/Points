using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DTOs;
using Npgsql;
using static ServerlessFunctions.Utils;

namespace ServerlessFunctions;

public class AdjustPointsBalance
{
    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (ValidateBody(request, out var response))
            return response!;

        var pointsBalance = DeserializePointsBalance(request);

        NpgsqlConnection? connection = null;
        try
        {
            connection = await OpenConnectionToRds();

            var existingPointsBalance = await GetPointsBalanceFromDatabase(connection);

            var adjustedPoints = existingPointsBalance.Points! + pointsBalance.Points!;
            var adjustedIdealPoints =
                existingPointsBalance.IdealPoints! + pointsBalance.IdealPoints!;

            await PersistAdjustedPointsBalanceToDatabase(
                connection,
                adjustedPoints,
                adjustedIdealPoints);
            await RegisterEventInTheActivity(
                connection,
                pointsBalance);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");

            return new APIGatewayProxyResponse
            {
                StatusCode = 500
            };
        }
        finally
        {
            await connection?.CloseAsync()!;
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 204
        };
    }

    private async Task RegisterEventInTheActivity(
        NpgsqlConnection connection,
        PointsBalanceDto pointsBalance)
    {
        var content = pointsBalance.IsContribution()
            ? $"{pointsBalance.Points} have been contributed."
            : $"{pointsBalance.PointsInAbsolute} have been missed...";
        
        const string sql = @"
            INSERT INTO ""Activity"" 
            (content, period_id)
            VALUES (@content, 1)";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@content", content);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();

        Console.WriteLine($"Registered Event: {content}");
        Console.WriteLine($"Rows affected: {rowsAffected}");
    }

    private static async Task PersistAdjustedPointsBalanceToDatabase(
        NpgsqlConnection connection,
        [DisallowNull] int? adjustedPoints,
        [DisallowNull] int? adjustedIdealPoints)
    {
        const string sql = @"
            UPDATE ""Period"" 
            SET points = @points, ideal_points = @ideal_points
            WHERE id = 1";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@points", adjustedPoints);
        command.Parameters.AddWithValue("@ideal_points", adjustedIdealPoints);

        var rowsAffected = await command.ExecuteNonQueryAsync();

        Console.WriteLine(
            $"New points: {adjustedPoints} | " +
            $"New ideal points: {adjustedIdealPoints}");
        Console.WriteLine($"Rows affected: {rowsAffected}");
    }

    private static PointsBalanceDto DeserializePointsBalance(APIGatewayProxyRequest request)
    {
        if (string.IsNullOrEmpty(request.Body))
            throw new ArgumentException();

        return JsonSerializer.Deserialize<PointsBalanceDto>(
            request.Body,
            JsonSerializerOptions)!;
    }

    private static bool ValidateBody(
        APIGatewayProxyRequest request,
        out APIGatewayProxyResponse? response)
    {
        Console.WriteLine("Body" + request.Body);
        PointsBalanceDto? pointsBalance = null;

        if (!string.IsNullOrEmpty(request.Body))
        {
            pointsBalance = DeserializePointsBalance(request);
        }

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