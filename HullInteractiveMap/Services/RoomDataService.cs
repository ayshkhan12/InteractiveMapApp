namespace HullInteractiveMap.Services;

public class RoomDataService
{
    public List<Room> Rooms { get; } = new()
    {
        new Room("022", 1, "Classroom", "Computer Science"),
        new Room("017", 1, "Lecture Hall", "General"),
        // Add all your rooms here
    };

    public Room? GetRoom(string id) =>
        Rooms.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}

public record Room(string Id, int Floor, string Type, string Department);