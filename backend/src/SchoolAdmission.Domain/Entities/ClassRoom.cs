using SchoolAdmission.Domain.Exceptions;

namespace SchoolAdmission.Domain.Entities;

public sealed class ClassRoom
{
    private ClassRoom()
    {
    }

    public ClassRoom(string id, string name, int totalSeats, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException("Class id is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Class name is required.");
        }

        if (name.Trim().Length > 200)
        {
            throw new DomainException("Class name must be 200 characters or fewer.");
        }

        if (totalSeats <= 0)
        {
            throw new DomainException("Total seats must be greater than zero.");
        }

        Id = id;
        Name = name.Trim();
        TotalSeats = totalSeats;
        CreatedAt = createdAt;
    }

    public string Id { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public int TotalSeats { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public ICollection<Hold> Holds { get; private set; } = new List<Hold>();
}
