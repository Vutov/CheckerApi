using CheckerApi.Models.Config;

namespace CheckerApi.Utils
{
    public static class DenominationHelper
    {
        public static Denomination ToDenomination(string str)
        {
            var data = str.ToLower();
            switch (data)
            {
                case "ks/s":
                    return Denomination.Ksol;
                case "ms/s":
                    return Denomination.Msol;
                default:
                    return Denomination.Sol;
            }
        }

        public static double ToMSol(double value, Denomination? denomination)
        {
            switch (denomination)
            {
                case Denomination.Msol:
                    return value;
                case Denomination.Ksol:
                    return value / 1000;
                case Denomination.Sol:
                    return value / 1000 / 1000;
                default: return value;
            }
        }

        // Possible Method Convert(value, from, to) or new class with value and denomination + convert to other
    }
}
