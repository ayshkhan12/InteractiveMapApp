using HullCampusMap.Models;
using System.Xml.Linq;

public class SvgParserService
{
    private readonly HttpClient _httpClient;

    public SvgParserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Room>> GetRoomsFromSvg(string svgPath)
    {
        var rooms = new List<Room>();

        try
        {
            var svgContent = await _httpClient.GetStringAsync($"svg/{svgPath}");
            var doc = XDocument.Parse(svgContent);

            // Find all path elements with IDs that look like rooms
            var roomElements = doc.Descendants()
                .Where(e => e.Attribute("id") != null &&
                       (e.Attribute("id").Value.StartsWith("_") ||
                        e.Attribute("data-name")?.Value.StartsWith("_") == true));

            foreach (var element in roomElements)
            {
                var id = element.Attribute("id")?.Value ?? element.Attribute("data-name")?.Value;
                if (!string.IsNullOrEmpty(id))
                {
                    rooms.Add(new Room
                    {
                        Id = id,
                        Name = $"Room {id.TrimStart('_')}",
                        Floor = svgPath
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing SVG: {ex.Message}");
        }

        return rooms;
    }
}