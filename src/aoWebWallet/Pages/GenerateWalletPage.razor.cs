using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace aoWebWallet.Pages
{
    public partial class GenerateWalletPage : MvvmComponentBase<MainViewModel>
    {
        private string Password { get; set; } = string.Empty;
        private string ConfirmPassword { get; set; } = string.Empty;
        private bool PasswordVisible { get; set; } = false;
        private InputType PasswordInput { get; set; } = InputType.Password;
        private string PasswordInputIcon { get; set; } = Icons.Material.Filled.VisibilityOff;
        private string PasswordError { get; set; } = string.Empty;
        private string ConfirmPasswordError { get; set; } = string.Empty;

        private void TogglePasswordVisibility()
        {
            if (PasswordVisible)
            {
                PasswordVisible = false;
                PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                PasswordInput = InputType.Password;
            }
            else
            {
                PasswordVisible = true;
                PasswordInputIcon = Icons.Material.Filled.Visibility;
                PasswordInput = InputType.Text;
            }
        }

        private void GenerateWallet()
        {
            PasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;

            if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                PasswordError = "Please enter and confirm your password.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ConfirmPasswordError = "Passwords do not match.";
                return;
            }

            if (Password.Length < 6)
            {
                PasswordError = "Password must be at least 6 characters long.";
                return;
            }

            if(BindingContext.WalletList.Data?.Any(x => x.NeedsUnlock) ?? true)
            {
                PasswordError = "Unable to set a new password. Please unlock your wallet first.";
                return;
            }

            BindingContext.SecretKey = Password;

            Snackbar.Add("Password set successfully!", Severity.Success);
        }
    }
}
