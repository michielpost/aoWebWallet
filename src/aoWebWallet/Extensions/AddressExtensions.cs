namespace aoWebWallet.Extensions
{
    public static class AddressExtensions
    {
        public static string ToShortAddress(this string address)
        {
            return address[0..7] + "..." + address[^7..];
        }
    }
}
