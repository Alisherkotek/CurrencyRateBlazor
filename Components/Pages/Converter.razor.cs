using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using BlazorTestApp.Models;
using BlazorTestApp.Services;

namespace BlazorTestApp.Components.Pages
{
    public partial class Converter
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        private decimal _amount = 100;
        private string _fromCurrency = "USD";
        private string _toCurrency = "KZT";
        private decimal? _result = null;

        // Жёстко заданный список валют, можно позже заменить на динамический
        private readonly List<CurrencyOption> currencies = new()
        {
            new CurrencyOption { Code = "USD", DisplayName = "USD - US Dollar" },
            new CurrencyOption { Code = "KZT", DisplayName = "KZT - Kazakhstani Tenge" },
            new CurrencyOption { Code = "RUB", DisplayName = "RUB - Russian Ruble" },
            new CurrencyOption { Code = "EUR", DisplayName = "EUR - Euro" }
        };

        // Простая проверка — нужна сумма и обе валюты
        private bool CanConvert => _amount > 0 && !string.IsNullOrEmpty(_fromCurrency) && !string.IsNullOrEmpty(_toCurrency);

        protected override async Task OnInitializedAsync()
        {
            // Убедимся, что данные о валютах загружены
            var allCurrencies = await CurrencyService.GetAllCurrenciesAsync();
            if (allCurrencies.Count == 0)
            {
                await CurrencyService.UpdateCurrencyRatesAsync();
            }
        }

        private async Task ConvertCurrency()
        {
            try
            {
                // Если валюты одинаковые — возвращаем ту же сумму
                if (_fromCurrency == _toCurrency)
                {
                    _result = _amount;
                    return;
                }

                var fromCurrencyData = await CurrencyService.GetCurrencyByCodeAsync(_fromCurrency);
                var toCurrencyData = await CurrencyService.GetCurrencyByCodeAsync(_toCurrency);

                if (fromCurrencyData == null || toCurrencyData == null)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "Предупреждение",
                        Detail = "Данные о валюте не найдены. Обновите курсы.",
                        Duration = 4000
                    });
                    return;
                }

                // Перевод через RUB (новая базовая валюта)
                decimal amountInRub = _amount * fromCurrencyData.Rate;
                _result = amountInRub / toCurrencyData.Rate;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка",
                    Detail = "Не удалось выполнить конвертацию валют",
                    Duration = 4000
                });
                _result = null;
            }
        }

        // Вспомогательный класс для отображения валют в выпадающем списке
        private class CurrencyOption
        {
            public string Code { get; set; }
            public string DisplayName { get; set; }
        }
    }
}
