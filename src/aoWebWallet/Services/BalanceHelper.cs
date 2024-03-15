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

            if (balance == 0)
                return string.Format($"{0:N1}", 0);
            
            // Convert the long to a decimal
            decimal decimalBalance = Convert.ToDecimal(balance);

            decimal multiplier = (decimal)Math.Pow(10, denomination);

            decimal result = decimalBalance / multiplier;

            string formattedDecimal = string.Format($"{{0:N{denomination}}}", result);

            return formattedDecimal;
        }

        public static long DecimalToTokenAmount(decimal amount, int denomination)
        {
            decimal multiplier = (decimal)Math.Pow(10, denomination);

            decimal result = amount * multiplier;
            var longResult = Convert.ToInt64(result);
           

            return longResult;
        }
    }
}
