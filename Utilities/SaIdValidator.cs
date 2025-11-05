using System.Globalization;

namespace ERecruitment.Web.Utilities;

public static class SaIdValidator
{
    public static bool IsValid(string? idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != 13 || !idNumber.All(char.IsDigit))
        {
            return false;
        }

        if (!DateTime.TryParseExact(idNumber[..6], "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return false;
        }

        return PassesLuhn(idNumber);
    }

    private static bool PassesLuhn(string idNumber)
    {
        var sum = 0;
        var alternate = false;

        for (var i = idNumber.Length - 1; i >= 0; i--)
        {
            var n = idNumber[i] - '0';
            if (alternate)
            {
                n *= 2;
                if (n > 9)
                {
                    n = (n % 10) + 1;
                }
            }
            sum += n;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }
}
