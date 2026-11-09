namespace SchoolAdmission.Domain.Entities;

public sealed class ClassRoom
{
    private ClassRoom()
    {
    }

    public ClassRoom(string id, string name, int totalSeats, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        TotalSeats = totalSeats;
        CreatedAt = createdAt;
    }

    public string Id { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public int TotalSeats { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public ICollection<Hold> Holds { get; private set; } = new List<Hold>();
}
