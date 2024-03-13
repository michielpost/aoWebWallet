using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace aoWebWallet.Services
{
    public class BalanceHelper
    {
        public static string FormatBalance(long? balance, int denomination)
        {
            if (!balance.HasValue)
                return string.Empty;
            
            // Convert the long to a decimal
            decimal decimalBalance = Convert.ToDecimal(balance);

            decimal multiplier = (decimal)Math.Pow(10, denomination);

            decimal result = decimalBalance / multiplier;

            string formattedDecimal = string.Format($"{{0:N{denomination}}}", result);

            return formattedDecimal;
        }
    }
}
