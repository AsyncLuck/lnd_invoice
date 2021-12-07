using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace lnd_invoice.Blazor.UIService
{
    public sealed class CopyToClipBoardService
    {
        private readonly IJSRuntime _jsRuntime;

        public CopyToClipBoardService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask<string> ReadTextAsync()
        {
            return _jsRuntime.InvokeAsync<string>("navigator.clipboard.readText");
        }

        public ValueTask WriteTextAsync(string text)
        {
            return _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }
}
