using BlazorTestApp.Models;

namespace BlazorTestApp.Services;

//Интерфейс для операций с валютами
//Поддерживает чистую архитектуру и удобство тестирования

public interface ICurrencyService
{
    //Получить все валюты из базы данных
    Task<List<Currency>> GetAllCurrenciesAsync();

    //Получить актуальные курсы валют из API и обновить базу данных
    Task<bool> UpdateCurrencyRatesAsync();

    //Экспортировать данные о валютах в Excel-файл
    Task<byte[]> ExportToExcelAsync(decimal baseAmount = 1, string baseCurrencyCode = "USD", int timeZoneOffset = 0);

    //Получить валюту по её коду
    Task<Currency?> GetCurrencyByCodeAsync(string code);
}