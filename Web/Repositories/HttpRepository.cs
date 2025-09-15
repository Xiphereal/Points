using System.Net;
using DTOs;

namespace Web.Repositories;

public class HttpRepository(string baseUrl)
{
    private readonly HttpClient httpClient = new();

    public async Task<PointsBalanceDto> GetPointsBalance()
    {
        var response = await httpClient.GetAsync($"{baseUrl}/GetPointsBalance");

        return await response.Content.ReadAsAsync<PointsBalanceDto>();
    }

    public async Task ContributeWith(int points)
    {
        var pointsBalanceIncrement = new PointsBalanceDto(
            Points: points,
            IdealPoints: points);

        await httpClient.PutAsJsonAsync(
            $"{baseUrl}/AdjustPointsBalance",
            pointsBalanceIncrement);
    }

    public async Task AddIdealPoints(int howMany)
    {
        var pointsBalanceIncrement = new PointsBalanceDto(
            Points: 0,
            IdealPoints: howMany);

        await httpClient.PutAsJsonAsync(
            $"{baseUrl}/AdjustPointsBalance",
            pointsBalanceIncrement);
    }

    public async Task ResetPoints()
    {
        var existingPointsBalance = await GetPointsBalance();
        var pointsBalanceDecrement = existingPointsBalance.ToDecrement();
        
        await httpClient.PutAsJsonAsync(
            $"{baseUrl}/AdjustPointsBalance",
            pointsBalanceDecrement);
    }
}