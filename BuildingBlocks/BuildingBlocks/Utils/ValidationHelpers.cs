using System.Linq;

namespace BuildingBlocks.Utils;

public static class ValidationHelpers
{
    public static bool IsValidIranianNationalCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 10 || !code.All(char.IsDigit))
            return false;

        var digits = code.Select(c => int.Parse(c.ToString())).ToArray();
        var check = digits[9];
        var sum = 0;

        for (int i = 0; i < 9; i++)
        {
            sum += digits[i] * (10 - i);
        }

        var remainder = sum % 11;
        return (remainder < 2 && check == remainder) ||
               (remainder >= 2 && check == (11 - remainder));
    }
}
