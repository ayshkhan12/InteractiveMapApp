using System.Xml.Linq;
using System.Xml.XPath;
using HullCampusMap.Models;

namespace HullCampusMap.Services;

public class SvgParserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SvgParserService> _logger;
    private readonly Dictionary<string, string> _roomDescriptions;

    public SvgParserService(HttpClient httpClient, ILogger<SvgParserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Predefined descriptions for special rooms
        _roomDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["stairs"] = "Main staircase between floors",
            ["toilets"] = "Public restroom facilities",
            ["toilets_"] = "Public restroom facilities",
            ["main_stairs"] = "Primary building staircase"
        };
    }

    public async Task<List<Room>> GetRoomsFromSvg(string svgPath)
    {
        try
        {
            // Load SVG content with cache busting
            var svgContent = await _httpClient.GetStringAsync($"svg/{svgPath}?v={DateTime.Now.Ticks}");
            var doc = XDocument.Parse(svgContent);

            // Process all path elements that represent rooms
            var rooms = doc.XPathSelectElements("//path[@id or @data-name]")
                .Where(IsRoomElement)
                .Select(element => CreateRoomFromElement(element, svgPath))
                .Where(room => room != null)
                .ToList()!;

            _logger.LogInformation("Found {RoomCount} rooms in {SvgPath}", rooms.Count, svgPath);
            return rooms;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing SVG file: {SvgPath}", svgPath);
            throw;
        }
    }

    private bool IsRoomElement(XElement element)
    {
        var id = element.Attribute("id")?.Value;
        var dataName = element.Attribute("data-name")?.Value;

        // Include elements that start with underscore or are special facilities
        return (!string.IsNullOrWhiteSpace(id) && (id.StartsWith("_") || IsSpecialFacility(id))) ||
               (!string.IsNullOrWhiteSpace(dataName) && dataName.StartsWith("_"));
    }

    private bool IsSpecialFacility(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;

        return id.Contains("stairs", StringComparison.OrdinalIgnoreCase) ||
               id.Contains("toilet", StringComparison.OrdinalIgnoreCase) ||
               id.Contains("elevator", StringComparison.OrdinalIgnoreCase);
    }

    private Room? CreateRoomFromElement(XElement element, string svgPath)
    {
        try
        {
            var id = element.Attribute("id")?.Value ?? element.Attribute("data-name")?.Value;
            if (string.IsNullOrWhiteSpace(id)) return null;

            var room = new Room
            {
                Id = id,
                Name = GenerateRoomName(id),
                Floor = svgPath,
                PathData = element.Attribute("d")?.Value,
                Transform = element.Attribute("transform")?.Value,
                BoundingBox = CalculateBoundingBox(element),
                Description = GetRoomDescription(id)
            };

            // Handle duplicate rooms (e.g., "_022-2")
            if (id.Contains('-') && int.TryParse(id.Split('-').Last(), out _))
            {
                room.AlternateIds.Add(id);
                room.Id = id.Split('-').First(); // Use base ID as primary
            }

            return room;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing SVG element with ID: {ElementId}",
                element.Attribute("id")?.Value);
            return null;
        }
    }

    private string GenerateRoomName(string id)
    {
        if (IsSpecialFacility(id))
        {
            return id switch
            {
                string s when s.Contains("stairs") => "Staircase",
                string s when s.Contains("toilet") => "Restroom",
                _ => id.Replace('_', ' ').Trim()
            };
        }

        return $"Room {id.TrimStart('_')}";
    }

    private string GetRoomDescription(string roomId)
    {
        if (_roomDescriptions.TryGetValue(roomId, out var description))
        {
            return description;
        }

        return roomId.StartsWith('_')
            ? $"Classroom {roomId.TrimStart('_')} at University of Hull"
            : $"{GenerateRoomName(roomId)} facility";
    }

    private string? CalculateBoundingBox(XElement element)
    {
        try
        {
            // First try to get explicit bounds if they exist
            var bounds = element.Attribute("bounds")?.Value
                      ?? element.Attribute("data-bounds")?.Value;

            if (!string.IsNullOrWhiteSpace(bounds)) return bounds;

            // Calculate from path data if needed
            var pathData = element.Attribute("d")?.Value;
            if (string.IsNullOrWhiteSpace(pathData)) return null;

            // Simple extraction of first point for demo purposes
            // In production, you'd want full SVG path parsing
            var firstMove = pathData.Split('M').LastOrDefault()?.Split(' ').FirstOrDefault();
            if (firstMove != null && firstMove.Contains(','))
            {
                var coords = firstMove.Split(',');
                if (coords.Length >= 2)
                {
                    return $"{coords[0]},{coords[1]}";
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}