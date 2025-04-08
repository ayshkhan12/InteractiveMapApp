using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HullCampusMap.Services
{
    public class RoomInterop : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference? _module;
        private DotNetObjectReference<RoomInterop>? _dotNetRef;
        private bool _disposed;

        public RoomInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        public async Task InitializeAsync(ElementReference svgObject, DotNetObjectReference<RoomInterop> dotNetRef)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomInterop));
            if (svgObject.Equals(default)) throw new ArgumentNullException(nameof(svgObject));
            if (dotNetRef == null) throw new ArgumentNullException(nameof(dotNetRef));

            try
            {
                _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./svgInterop.js");

                _dotNetRef = dotNetRef;  // Use the passed-in reference
                await _module.InvokeVoidAsync("initialize", svgObject, dotNetRef);
            }
            catch (JSException jsEx)
            {
                throw new InvalidOperationException("Failed to initialize SVG interop module", jsEx);
            }
        }

        public async Task HighlightRoomAsync(ElementReference svgObject, string roomId, bool scrollToView = true)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomInterop));
            if (svgObject.Equals(default)) throw new ArgumentNullException(nameof(svgObject));
            if (string.IsNullOrWhiteSpace(roomId)) throw new ArgumentNullException(nameof(roomId));

            try
            {
                if (_module != null)
                {
                    await _module.InvokeVoidAsync("highlightRoom", svgObject, roomId, scrollToView);
                }
            }
            catch (JSException jsEx)
            {
                throw new InvalidOperationException($"Failed to highlight room {roomId}", jsEx);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                if (_module != null)
                {
                    await _module.DisposeAsync();
                }
                _dotNetRef?.Dispose();
            }
            catch (JSException jsEx)
            {
                Console.Error.WriteLine($"Error disposing RoomInterop: {jsEx.Message}");
            }
        }
    }
}