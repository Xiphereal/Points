namespace DTOs;

public record EventDto
{
    public EventDto(string content, DateTime occurredAt)
    {
        Content = content;
        OccurredAt = occurredAt;
    }

    public string Content { get; }
    public DateTime OccurredAt { get; }

    public void Deconstruct(out string content, out DateTime occurredAt)
    {
        content = Content;
        occurredAt = OccurredAt;
    }

    public override string ToString()
    {
        return $"[{OccurredAt.ToLocalTime()}] {Content}";
    }
}