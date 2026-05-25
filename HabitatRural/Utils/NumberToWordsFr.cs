namespace HabitatRural.Utils;

/// <summary>Converts a decimal amount to French words (Algerian administrative style).</summary>
public static class NumberToWordsFr
{
    private static readonly string[] _units =
    {
        "", "UN", "DEUX", "TROIS", "QUATRE", "CINQ", "SIX", "SEPT", "HUIT", "NEUF",
        "DIX", "ONZE", "DOUZE", "TREIZE", "QUATORZE", "QUINZE", "SEIZE",
        "DIX-SEPT", "DIX-HUIT", "DIX-NEUF"
    };

    private static readonly string[] _tens =
        { "", "DIX", "VINGT", "TRENTE", "QUARANTE", "CINQUANTE", "SOIXANTE" };

    public static string Convert(decimal amount)
    {
        if (amount == 0) return "ZÉRO DINARS";
        long intPart = (long)Math.Floor(amount);
        int  cents   = (int)Math.Round((amount - intPart) * 100);

        var result = ToWords(intPart) + (intPart > 1 ? " DINARS" : " DINAR");
        if (cents > 0)
            result += " ET " + ToWords(cents) + (cents > 1 ? " CENTIMES" : " CENTIME");
        return result;
    }

    private static string ToWords(long n)
    {
        if (n == 0) return "";
        if (n < 0)  return "MOINS " + ToWords(-n);

        var parts = new List<string>();

        if (n >= 1_000_000_000)
        {
            long b = n / 1_000_000_000;
            parts.Add(b == 1 ? "UN MILLIARD" : Hundreds((int)b) + " MILLIARDS");
            n %= 1_000_000_000;
        }
        if (n >= 1_000_000)
        {
            long m = n / 1_000_000;
            parts.Add(m == 1 ? "UN MILLION" : Hundreds((int)m) + " MILLIONS");
            n %= 1_000_000;
        }
        if (n >= 1_000)
        {
            long k = n / 1_000;
            parts.Add(k == 1 ? "MILLE" : Hundreds((int)k) + " MILLE");
            n %= 1_000;
        }
        if (n > 0)
            parts.Add(Hundreds((int)n));

        return string.Join(" ", parts);
    }

    private static string Hundreds(int n)
    {
        var parts = new List<string>();
        if (n >= 100)
        {
            int h = n / 100;
            n %= 100;
            if (h == 1)
                parts.Add(n == 0 ? "CENT" : "CENT");
            else
                parts.Add(_units[h] + (n == 0 ? " CENTS" : " CENT"));
        }
        if (n >= 20)
        {
            int t = n / 10, u = n % 10;
            switch (t)
            {
                case 7: // 70–79  →  SOIXANTE-DIX...
                    parts.Add(u == 1 ? "SOIXANTE-ET-ONZE"
                                     : "SOIXANTE-" + _units[10 + u]);
                    break;
                case 8: // 80–89
                    parts.Add(u == 0 ? "QUATRE-VINGTS"
                                     : "QUATRE-VINGT-" + _units[u]);
                    break;
                case 9: // 90–99
                    parts.Add("QUATRE-VINGT-" + _units[10 + u]);
                    break;
                default:
                    string ten = _tens[t];
                    if (u == 0)        parts.Add(ten);
                    else if (u == 1)   parts.Add(ten + "-ET-UN");
                    else               parts.Add(ten + "-" + _units[u]);
                    break;
            }
        }
        else if (n > 0)
        {
            parts.Add(_units[n]);
        }
        return string.Join(" ", parts);
    }
}
