namespace RuralHousingApp.Services
{
    public static class FrenchNumberConverter
    {
        public static string ToFrenchWords(decimal amount)
        {
            long val = (long)Math.Floor(amount);
            if (val == 0) return "ZERO DINARS";
            
            string[] units = { "", "UN", "DEUX", "TROIS", "QUATRE", "CINQ", "SIX", "SEPT", "HUIT", "NEUF", "DIX",
                "ONZE", "DOUZE", "TREIZE", "QUATORZE", "QUINZE", "SEIZE", "DIX-SEPT", "DIX-HUIT", "DIX-NEUF" };
            string[] tens = { "", "", "VINGT", "TRENTE", "QUARANTE", "CINQUANTE", "SOIXANTE", "SOIXANTE", "QUATRE-VINGT", "QUATRE-VINGT" };
            
            string ConvertHundred(long n)
            {
                string res = "";
                if (n >= 100) { res += (n == 100 ? "CENT" : $"{units[n / 100]} CENT{(n % 100 == 0 && n > 100 ? "S" : "")} "); n %= 100; }
                if (n >= 20) { int t = (int)n / 10; res += tens[t] + ((t == 7 || t == 9) ? (n % 10 == 1 && t == 7 ? " ET " : "-") : " ") + ((t == 8 && n % 10 == 0) ? "" : units[(int)n % 10]); }
                else if (n > 0) res += units[n];
                return res.Trim();
            }

            string res = "";
            if (val >= 1000000) { res += ConvertHundred(val / 1000000) + " MILLION" + (val % 1000000 == 0 ? "" : " ") ; val %= 1000000; }
            if (val >= 1000) { res += ConvertHundred(val / 1000) + " MILLE "; val %= 1000; }
            if (val > 0) res += ConvertHundred(val);
            return (res + " DINARS").Trim().Replace("  ", " ");
        }
    }
}