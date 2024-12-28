using ClipLazor;
using ClipLazor.Components;
using MudBlazor;

namespace aoWebWallet.Services
{
    public class ClipboardService(IClipLazor clipboard, ISnackbar snackbar)
    {
        public async Task CopyToClipboard(string? text, string? itemName = "Address")
        {
            bool isSupported = await clipboard.IsClipboardSupported();
            bool isWritePermitted = await clipboard.IsPermitted(PermissionCommand.Write);
            if (isSupported && !string.IsNullOrEmpty(text))
            {
                if (isWritePermitted)
                {
                    var isCopied = await clipboard.WriteTextAsync(text.AsMemory());
                    if (isCopied)
                    {
                        snackbar.Add($"{itemName} copied to clipboard", Severity.Success);
                    }
                }
            }
        }
    }
}
