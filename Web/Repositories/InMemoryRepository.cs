namespace Web.Repositories;

public class InMemoryRepository
{
    public int Points { get; private set; }
    public int IdealPoints { get; private set; }

    public void ContributeWith(int points)
    {
        Points += points;
        IdealPoints += points;
    }

    public void AddIdealPoints(int howMany)
    {
        IdealPoints += howMany;
    }

    public void ResetPoints()
    {
        Points = IdealPoints = 0;
    }
}