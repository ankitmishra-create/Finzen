using System.Globalization;

namespace FinanceManagement.Application.Utility
{
    public record CurrencyRecords
    {
        public string CurrencySymbol { get; init; }
        public string CurrencyName { get; init; }
        public string CurrencyCode { get; init; }
    }

    public static class CurrencySymbol
    {
        public static List<CurrencyRecords> GetCultures()
        {
            var culture = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            var uniqueCurrencies = culture.Select(
                c =>
                {
                    try
                    {
                        RegionInfo regionInfo = new RegionInfo(c.Name);
                        return new CurrencyRecords
                        {
                            CurrencySymbol = regionInfo.CurrencySymbol,
                            CurrencyName = regionInfo.CurrencyEnglishName,
                            CurrencyCode = regionInfo.ISOCurrencySymbol
                        };
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(x => x != null).Distinct().OrderBy(x => x.CurrencyCode).ToList();
            return uniqueCurrencies;
        }
    }
}
