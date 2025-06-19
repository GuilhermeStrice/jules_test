namespace SAFT.Validation
{
    public static class PortugueseNifValidator
    {
        public static bool IsValid(string nif)
        {
            if (string.IsNullOrWhiteSpace(nif) || nif.Length != 9 || !long.TryParse(nif, out _))
                return false;
            int sum = 0;
            for (int i = 0; i < 8; i++)
                sum += (nif[i] - '0') * (9 - i);
            int checkDigit = 11 - (sum % 11);
            if (checkDigit >= 10) checkDigit = 0;
            return checkDigit == (nif[8] - '0');
        }
    }
} 