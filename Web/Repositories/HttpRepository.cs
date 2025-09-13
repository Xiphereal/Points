using System.Net;
using DTOs;

namespace Web.Repositories;

public class HttpRepository(string baseUrl)
{
    private readonly HttpClient httpClient = new();
    private PointsBalanceDto? cachedPointsBalance;

    public async Task<PointsBalanceDto> GetPointsBalance()
    {
        var response = await httpClient.GetAsync($"{baseUrl}/GetPointsBalance");

        var pointsBalance = 
            cachedPointsBalance =
                await response.Content.ReadAsAsync<PointsBalanceDto>();

        return pointsBalance;
    }

    public async Task ContributeWith(int points)
    {
        var previousPointsBalance = await GetPointsBalanceWithReadThroughCache();

        var adjustedPointsBalance = new PointsBalanceDto(
            Points: previousPointsBalance.Points + points,
            IdealPoints: previousPointsBalance.IdealPoints + points);

        await httpClient.PutAsJsonAsync(
            $"{baseUrl}/AdjustPointsBalance",
            adjustedPointsBalance);
    }

    private async Task<PointsBalanceDto> GetPointsBalanceWithReadThroughCache()
    {
        if (cachedPointsBalance is not null)
            return cachedPointsBalance;

        return await GetPointsBalance();
    }

    public async Task AddIdealPoints(int howMany)
    {
        var previousPointsBalance = await GetPointsBalanceWithReadThroughCache();

        var adjustedPointsBalance = new PointsBalanceDto(
            Points: previousPointsBalance.Points,
            IdealPoints: previousPointsBalance.IdealPoints + howMany);

        await httpClient.PutAsJsonAsync(
            $"{baseUrl}/AdjustPointsBalance",
            adjustedPointsBalance);
    }

    public async Task ResetPoints()
    {
        await httpClient.PutAsJsonAsync(
            $"{baseUrl}/AdjustPointsBalance",
            PointsBalanceDto.Empty());
    }
}