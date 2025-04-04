using Microsoft.JSInterop;

namespace HullInteractiveMap.Services;

public class JsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public JsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/map.js");
    }

    public async Task HighlightRoom(string roomId)
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("highlightRoom", roomId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}