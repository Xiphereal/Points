using System.Net;
using DTOs;

namespace Web.Repositories;

public class HttpRepository
{
    private readonly HttpClient httpClient = new();

    public async Task<PointsBalanceDto> GetPointsBalance()
    {
        var response = await httpClient.GetAsync($"{GetBaseUrl()}/GetPointsBalance");

        return await response.Content.ReadAsAsync<PointsBalanceDto>();
    }

    public async Task ContributeWith(int points)
    {
        var previousPointsBalance = await GetPointsBalance();

        var adjustedPointsBalance = new PointsBalanceDto(
            Points: previousPointsBalance.Points + points,
            IdealPoints: previousPointsBalance.IdealPoints + points);

        await httpClient.PostAsJsonAsync(
            $"{GetBaseUrl()}/AdjustPointsBalance",
            adjustedPointsBalance);
    }

    private static string? GetBaseUrl()
    {
        return Environment.GetEnvironmentVariable("BASE_URL");
    }

    public async Task AddIdealPoints(int howMany)
    {
        var previousPointsBalance = await GetPointsBalance();

        var adjustedPointsBalance = new PointsBalanceDto(
            Points: previousPointsBalance.Points,
            IdealPoints: previousPointsBalance.IdealPoints + howMany);

        await httpClient.PostAsJsonAsync(
            $"{GetBaseUrl()}/AdjustPointsBalance",
            adjustedPointsBalance);
    }

    public async Task ResetPoints()
    {
        await httpClient.PostAsJsonAsync(
            $"{GetBaseUrl()}/AdjustPointsBalance",
            PointsBalanceDto.Empty());
    }
}