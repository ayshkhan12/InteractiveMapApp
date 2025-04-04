using System.Text.RegularExpressions;

namespace HullInteractiveMap.Services;

public class SvgProcessingService
{
    public string ProcessSvg(string svgContent, int floor)
    {
        var processed = Regex.Replace(svgContent,
            @"id=""_(\w+)""",
            match => $@"id=""floor{floor}_{match.Groups[1].Value}"" 
                      class=""room"" 
                      data-room=""{match.Groups[1].Value}""");

        return processed.Replace("class=\"cls-1\"", "class=\"room\"");
    }
}