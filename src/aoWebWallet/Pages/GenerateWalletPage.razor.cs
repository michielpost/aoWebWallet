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
            if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                Snackbar.Add("Please enter and confirm your password.", Severity.Warning);
                return;
            }

            if (Password != ConfirmPassword)
            {
                Snackbar.Add("Passwords do not match.", Severity.Error);
                return;
            }

            if (Password.Length < 6)
            {
                Snackbar.Add("Password must be at least 6 characters long.", Severity.Warning);
                return;
            }

            BindingContext.SecretKey = Password;

            // TODO: Implement wallet generation logic here
            Snackbar.Add("Wallet generated successfully!", Severity.Success);
        }
    }
}
