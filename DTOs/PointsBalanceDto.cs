namespace DTOs;

public record PointsBalanceDto
{
    public static PointsBalanceDto Empty() => new(0, 0);

    public PointsBalanceDto(int? Points, int? IdealPoints)
    {
        this.Points = Points;
        this.IdealPoints = IdealPoints;
    }

    public int? Points { get; init; }
    public int? IdealPoints { get; init; }
    public int PointsInAbsolute => Math.Abs(Points!.Value);

    public void Deconstruct(out int? Points, out int? IdealPoints)
    {
        Points = this.Points;
        IdealPoints = this.IdealPoints;
    }

    public PointsBalanceDto ToDecrement()
    {
        return new PointsBalanceDto(
            ToNegative(Points!.Value),
            ToNegative(IdealPoints!.Value));
    }

    private static int ToNegative(int number) => Math.Abs(number) * -1;

    /// <returns> True if the balance is of a contribution; false if it is a miss. </returns>
    public bool IsContribution() => Points >= 0;
}