using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using BlazorTestApp.Models;
using BlazorTestApp.Services;
using BlazorTestApp.Configuration;

namespace BlazorTestApp.Components.Pages
{
    public partial class Index
    {
        private const int NotificationDuration = 4000;

        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Inject] private ICurrencyService CurrencyService { get; set; } = default!;
        [Inject] private CurrencySettings Settings { get; set; } = default!; 
        
        
        private RadzenDataGrid<Currency>? _grid;

        private List<Currency> _currencies = new();
        private List<Currency> _filteredCurrencies = new();

        private bool _isLoading = true;
        private bool _isRefreshing = false;
        private bool _isDownloading = false;

        private DateTime? _lastUpdate;
        private string _userTimeZoneName = "UTC";
        private int _userTimeZoneOffset = 0;

        private string _codeSearchText = string.Empty;
        private string _nameSearchText = string.Empty;

        private decimal _baseAmount = 1;
        private string _baseCurrency = "USD";
        private Currency? _baseCurrencyData;
        private List<Currency> _allAvailableCurrencies = new();

        protected override async Task OnInitializedAsync()
        {
            await GetUserTimeZone();
            await LoadAvailableCurrencies();
            await LoadData();
        }

        private async Task GetUserTimeZone()
        {
            try
            {
                _userTimeZoneOffset = await JSRuntime.InvokeAsync<int>("eval", "new Date().getTimezoneOffset()");
                _userTimeZoneOffset = -_userTimeZoneOffset / 60; 
                
                _userTimeZoneName = _userTimeZoneOffset == Settings.DefaultTimeZoneOffset 
                    ? Settings.DefaultTimeZoneName 
                    : (_userTimeZoneOffset >= 0 ? $"UTC+{_userTimeZoneOffset}" : $"UTC{_userTimeZoneOffset}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to get user's timezone, using default value");
                _userTimeZoneOffset = Settings.DefaultTimeZoneOffset;
                _userTimeZoneName = Settings.DefaultTimeZoneName;
            }
        }

        private async Task LoadAvailableCurrencies()
        {
            try
            {
                _allAvailableCurrencies = await CurrencyService.GetAllCurrenciesAsync();
                _baseCurrencyData = _allAvailableCurrencies.FirstOrDefault(c => c.Code == _baseCurrency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currency list");
                _allAvailableCurrencies = new List<Currency>();
                ShowNotification(NotificationSeverity.Error, "Error", "Failed to load currency list");
            }
        }

        private async Task LoadData()
        {
            _isLoading = true;

            try
            {
                _currencies = await CurrencyService.GetAllCurrenciesAsync();
                ApplyFilters();
                
                var firstCurrency = _currencies.FirstOrDefault();
                if (firstCurrency != null)
                {
                    _lastUpdate = firstCurrency.LastUpdated.AddHours(_userTimeZoneOffset);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currency data");
                ShowNotification(NotificationSeverity.Error, "Error", "Failed to load currency data");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyFilters()
        {
            _filteredCurrencies = _currencies
                .Where(c =>
                    (string.IsNullOrWhiteSpace(_codeSearchText) || c.Code.Contains(_codeSearchText, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(_nameSearchText) || c.Name.Contains(_nameSearchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private async Task OnSearchChanged()
        {
            ApplyFilters();
            await InvokeAsync(StateHasChanged);
        }

        private async Task ClearFilters()
        {
            _codeSearchText = _nameSearchText = string.Empty;
            ApplyFilters();
            await InvokeAsync(StateHasChanged);
        }

        private string GetRateColumnTitle() => $"Rate ({_baseAmount} {_baseCurrency})";

        private decimal GetEquivalentAmount(Currency currency)
        {
            if (_baseCurrencyData == null || currency.Code == _baseCurrency)
                return _baseAmount;

            return _baseCurrency == "USD"
                ? _baseAmount * currency.Rate
                : _baseAmount * (currency.Rate / _baseCurrencyData.Rate);
        }

        private async Task OnBaseCurrencyChanged()
        {
            _baseCurrencyData = _allAvailableCurrencies.FirstOrDefault(c => c.Code == _baseCurrency);
            await InvokeAsync(StateHasChanged);
        }

        private async Task RefreshData()
        {
            _isRefreshing = true;
            StateHasChanged();

            try
            {
                var success = await CurrencyService.UpdateCurrencyRatesAsync();

                if (success)
                {
                    ShowNotification(NotificationSeverity.Success, "Success", "Exchange rates have been successfully updated");
                    await LoadAvailableCurrencies();
                    await LoadData();
                    StateHasChanged();
                }
                else
                {
                    ShowNotification(NotificationSeverity.Warning, "Warning", "Failed to update exchange rates");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rates");
                ShowNotification(NotificationSeverity.Error, "Error", $"An error occurred: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
                StateHasChanged();
            }
        }

        private async Task DownloadExcel()
        {
            _isDownloading = true;

            try
            {
                var fileContent = await CurrencyService.ExportToExcelAsync(_baseAmount, _baseCurrency, _userTimeZoneOffset);
                var fileName = $"CurrencyRates_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var base64 = Convert.ToBase64String(fileContent);
                await JSRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, base64);

                ShowNotification(NotificationSeverity.Success, "Success", "Excel file downloaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading Excel file");
                ShowNotification(NotificationSeverity.Error, "Error", $"Failed to download Excel file: {ex.Message}");
            }
            finally
            {
                _isDownloading = false;
            }
        }

        private void ShowNotification(NotificationSeverity severity, string summary, string detail)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = severity,
                Summary = summary,
                Detail = detail,
                Duration = NotificationDuration
            });
        }

        [Inject] private ILogger<Index> _logger { get; set; } = default!;
    }
}