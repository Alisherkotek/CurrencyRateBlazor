using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BlazorTestApp.Data;
using BlazorTestApp.Models;
using BlazorTestApp.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace BlazorTestApp.Services;

public class CurrencyService : ICurrencyService
{
    private readonly CurrencyContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CurrencyService> _logger;
    private readonly CurrencySettings _settings;

    private static readonly Dictionary<string, string> CurrencyNames = new()
    {
        ["USD"] = "US Dollar",
        ["EUR"] = "Euro",
        ["GBP"] = "British Pound",
        ["JPY"] = "Japanese Yen",
        ["AUD"] = "Australian Dollar",
        ["CAD"] = "Canadian Dollar",
        ["CHF"] = "Swiss Franc",
        ["CNY"] = "Chinese Yuan",
        ["SEK"] = "Swedish Krona",
        ["NZD"] = "New Zealand Dollar",
        ["MXN"] = "Mexican Peso",
        ["SGD"] = "Singapore Dollar",
        ["HKD"] = "Hong Kong Dollar",
        ["NOK"] = "Norwegian Krone",
        ["KRW"] = "South Korean Won",
        ["TRY"] = "Turkish Lira",
        ["RUB"] = "Russian Ruble",
        ["INR"] = "Indian Rupee",
        ["BRL"] = "Brazilian Real",
        ["ZAR"] = "South African Rand",
        ["KZT"] = "Kazakhstani Tenge",
        ["UAH"] = "Ukrainian Hryvnia",
        ["AED"] = "UAE Dirham",
        ["SAR"] = "Saudi Riyal",
        ["PLN"] = "Polish Zloty",
        ["CZK"] = "Czech Koruna",
        ["THB"] = "Thai Baht",
        ["IDR"] = "Indonesian Rupiah",
        ["MYR"] = "Malaysian Ringgit",
        ["PHP"] = "Philippine Peso",
        ["DKK"] = "Danish Krone",
        ["HUF"] = "Hungarian Forint",
        ["BGN"] = "Bulgarian Lev",
        ["RON"] = "Romanian Leu",
        ["ILS"] = "Israeli Shekel",
        ["CLP"] = "Chilean Peso",
        ["ISK"] = "Icelandic Krona",
        ["HRK"] = "Croatian Kuna",
        ["ARS"] = "Argentine Peso",
        ["EGP"] = "Egyptian Pound",
        ["AFN"] = "Afghan Afghani",
        ["ALL"] = "Albanian Lek",
        ["AMD"] = "Armenian Dram",
        ["ANG"] = "Netherlands Antillean Guilder",
        ["AOA"] = "Angolan Kwanza",
        ["AWG"] = "Aruban Florin",
        ["AZN"] = "Azerbaijani Manat",
        ["BAM"] = "Bosnia-Herzegovina Convertible Mark",
        ["BBD"] = "Barbadian Dollar",
        ["BDT"] = "Bangladeshi Taka",
        ["BHD"] = "Bahraini Dinar",
        ["BIF"] = "Burundian Franc",
        ["BMD"] = "Bermudan Dollar",
        ["BND"] = "Brunei Dollar",
        ["BOB"] = "Bolivian Boliviano",
        ["BSD"] = "Bahamian Dollar",
        ["BTN"] = "Bhutanese Ngultrum",
        ["BWP"] = "Botswanan Pula",
        ["BYN"] = "Belarusian Ruble",
        ["BZD"] = "Belize Dollar",
        ["CDF"] = "Congolese Franc",
        ["CLF"] = "Chilean Unit of Account",
        ["COP"] = "Colombian Peso",
        ["CRC"] = "Costa Rican Colón",
        ["CUC"] = "Cuban Convertible Peso",
        ["CUP"] = "Cuban Peso",
        ["CVE"] = "Cape Verdean Escudo",
        ["DJF"] = "Djiboutian Franc",
        ["DOP"] = "Dominican Peso",
        ["DZD"] = "Algerian Dinar",
        ["ERN"] = "Eritrean Nakfa",
        ["ETB"] = "Ethiopian Birr",
        ["FJD"] = "Fijian Dollar",
        ["FKP"] = "Falkland Islands Pound",
        ["GEL"] = "Georgian Lari",
        ["GGP"] = "Guernsey Pound",
        ["GHS"] = "Ghanaian Cedi",
        ["GIP"] = "Gibraltar Pound",
        ["GMD"] = "Gambian Dalasi",
        ["GNF"] = "Guinean Franc",
        ["GTQ"] = "Guatemalan Quetzal",
        ["GYD"] = "Guyanaese Dollar",
        ["HNL"] = "Honduran Lempira",
        ["HTG"] = "Haitian Gourde",
        ["IMP"] = "Manx Pound",
        ["IQD"] = "Iraqi Dinar",
        ["IRR"] = "Iranian Rial",
        ["JEP"] = "Jersey Pound",
        ["JMD"] = "Jamaican Dollar",
        ["JOD"] = "Jordanian Dinar",
        ["KES"] = "Kenyan Shilling",
        ["KGS"] = "Kyrgystani Som",
        ["KHR"] = "Cambodian Riel",
        ["KMF"] = "Comorian Franc",
        ["KPW"] = "North Korean Won",
        ["KWD"] = "Kuwaiti Dinar",
        ["KYD"] = "Cayman Islands Dollar",
        ["LAK"] = "Laotian Kip",
        ["LBP"] = "Lebanese Pound",
        ["LKR"] = "Sri Lankan Rupee",
        ["LRD"] = "Liberian Dollar",
        ["LSL"] = "Lesotho Loti",
        ["LYD"] = "Libyan Dinar",
        ["MAD"] = "Moroccan Dirham",
        ["MDL"] = "Moldovan Leu",
        ["MGA"] = "Malagasy Ariary",
        ["MKD"] = "Macedonian Denar",
        ["MMK"] = "Myanma Kyat",
        ["MNT"] = "Mongolian Tugrik",
        ["MOP"] = "Macanese Pataca",
        ["MRO"] = "Mauritanian Ouguiya",
        ["MUR"] = "Mauritian Rupee",
        ["MVR"] = "Maldivian Rufiyaa",
        ["MWK"] = "Malawian Kwacha",
        ["MZN"] = "Mozambican Metical",
        ["NAD"] = "Namibian Dollar",
        ["NGN"] = "Nigerian Naira",
        ["NIO"] = "Nicaraguan Córdoba",
        ["NPR"] = "Nepalese Rupee",
        ["OMR"] = "Omani Rial",
        ["PAB"] = "Panamanian Balboa",
        ["PEN"] = "Peruvian Nuevo Sol",
        ["PGK"] = "Papua New Guinean Kina",
        ["PKR"] = "Pakistani Rupee",
        ["PYG"] = "Paraguayan Guarani",
        ["QAR"] = "Qatari Rial",
        ["RSD"] = "Serbian Dinar",
        ["RWF"] = "Rwandan Franc",
        ["SBD"] = "Solomon Islands Dollar",
        ["SCR"] = "Seychellois Rupee",
        ["SDG"] = "Sudanese Pound",
        ["SHP"] = "Saint Helena Pound",
        ["SLL"] = "Sierra Leonean Leone",
        ["SOS"] = "Somali Shilling",
        ["SRD"] = "Surinamese Dollar",
        ["STD"] = "São Tomé and Príncipe Dobra",
        ["SVC"] = "Salvadoran Colón",
        ["SYP"] = "Syrian Pound",
        ["SZL"] = "Swazi Lilangeni",
        ["TJS"] = "Tajikistani Somoni",
        ["TMT"] = "Turkmenistani Manat",
        ["TND"] = "Tunisian Dinar",
        ["TOP"] = "Tongan Paʻanga",
        ["TTD"] = "Trinidad and Tobago Dollar",
        ["TWD"] = "New Taiwan Dollar",
        ["TZS"] = "Tanzanian Shilling",
        ["UGX"] = "Ugandan Shilling",
        ["UYU"] = "Uruguayan Peso",
        ["UZS"] = "Uzbekistan Som",
        ["VEF"] = "Venezuelan Bolívar Fuerte",
        ["VND"] = "Vietnamese Dong",
        ["VUV"] = "Vanuatu Vatu",
        ["WST"] = "Samoan Tala",
        ["XAF"] = "CFA Franc BEAC",
        ["XCD"] = "East Caribbean Dollar",
        ["XOF"] = "CFA Franc BCEAO",
        ["XPF"] = "CFP Franc",
        ["YER"] = "Yemeni Rial",
        ["ZMW"] = "Zambian Kwacha",
        ["ZWL"] = "Zimbabwean Dollar"
    };

    public CurrencyService(
        CurrencyContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<CurrencyService> logger,
        CurrencySettings settings)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _settings = settings;
    }

    public async Task<List<Currency>> GetAllCurrenciesAsync()
    {
        try
        {
            return await _context.Currencies
                .OrderBy(c => c.Code)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting list of currencies from database");
            throw;
        }
    }

    public async Task<bool> UpdateCurrencyRatesAsync()
    {
        try
        {
            _logger.LogInformation(
                "We are starting to update exchange rates in {Time} ({TimeZone})",
                DateTime.UtcNow.AddHours(_settings.DefaultTimeZoneOffset).ToString("yyyy-MM-dd HH:mm:ss"),
                _settings.DefaultTimeZoneName);

            var httpClient = _httpClientFactory.CreateClient("CurrencyApi");

            var response = await httpClient.GetStringAsync("");
            var data = JsonConvert.DeserializeObject<ExchangeRateApiResponse>(response);

            if (data?.Rates == null)
            {
                _logger.LogWarning("API did not return exchange rate data");
                return false;
            }

            _logger.LogInformation("Received {Count} currencies from API", data.Rates.Count);

            foreach (var rate in data.Rates)
            {
                await UpdateOrCreateCurrency(
                    rate.Key,
                    GetCurrencyName(rate.Key),
                    (decimal)rate.Value
                );
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Exchange rates have been successfully updated in {Time} ({TimeZone})",
                DateTime.UtcNow.AddHours(_settings.DefaultTimeZoneOffset).ToString("yyyy-MM-dd HH:mm:ss"),
                _settings.DefaultTimeZoneName);

            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while getting exchange rates");
            return false;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error when processing exchange rates");
            return false;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error saving exchange rates to the database");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating exchange rates");
            throw;
        }
    }

    private async Task UpdateOrCreateCurrency(string code, string name, decimal rate)
    {
        var existingCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code == code);

        if (existingCurrency == null)
        {
            var newCurrency = new Currency
            {
                Code = code,
                Name = name,
                Rate = rate,
                LastUpdated = DateTime.UtcNow
            };
            _context.Currencies.Add(newCurrency);

            _logger.LogDebug("New currency added: {Code} - {Name}", code, name);
        }
        else
        {
            existingCurrency.Name = name;
            existingCurrency.Rate = rate;
            existingCurrency.LastUpdated = DateTime.UtcNow;

            _logger.LogDebug("Обновлена валюта: {Code} - {Name}, новый курс: {Rate}", code, name, rate);
        }
    }

    private static string GetCurrencyName(string code)
    {
        return CurrencyNames.TryGetValue(code, out var name) ? name : code;
    }

    public async Task<byte[]> ExportToExcelAsync(decimal baseAmount = 1, string baseCurrencyCode = "USD",
        int timeZoneOffset = 0)
    {
        try
        {
            var currencies = await GetAllCurrenciesAsync();
            var baseCurrency = currencies.FirstOrDefault(c => c.Code == baseCurrencyCode);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Currency Rates");

            worksheet.Cells[1, 1].Value = "Code";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = $"Rate ({baseAmount} {baseCurrencyCode})";
            worksheet.Cells[1, 4].Value = "Last Updated";

            using (var range = worksheet.Cells[1, 1, 1, 4])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            int row = 2;
            foreach (var currency in currencies)
            {
                worksheet.Cells[row, 1].Value = currency.Code;
                worksheet.Cells[row, 2].Value = currency.Name;

                decimal relativeRate = baseAmount;
                if (baseCurrency != null && currency.Code != baseCurrencyCode)
                {
                    relativeRate = baseAmount * (currency.Rate / baseCurrency.Rate);
                }

                worksheet.Cells[row, 3].Value = relativeRate;

                var localTime = currency.LastUpdated.AddHours(timeZoneOffset);
                worksheet.Cells[row, 4].Value = localTime.ToString("yyyy-MM-dd HH:mm:ss");
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            _logger.LogInformation("Excel file created successfully with {Count} currencies", currencies.Count);

            return package.GetAsByteArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Excel file");
            throw;
        }
    }

    public async Task<Currency?> GetCurrencyByCodeAsync(string code)
    {
        try
        {
            return await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when receiving currency by code: {Code}", code);
            throw;
        }
    }
}

public class ExchangeRateApiResponse
{
    [JsonProperty("base")] public string Base { get; set; } = string.Empty;

    [JsonProperty("date")] public string Date { get; set; } = string.Empty;

    [JsonProperty("rates")] public Dictionary<string, double> Rates { get; set; } = new();
}