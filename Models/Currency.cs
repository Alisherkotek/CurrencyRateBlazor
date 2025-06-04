using System.ComponentModel.DataAnnotations;

namespace BlazorTestApp.Models;

/// <summary>
/// Основная модель валюты — представляет отдельную валюту с её курсом обмена
/// </summary>
public class Currency
{
    [Key] public int Id { get; set; }

    [Required] [MaxLength(3)] public string Code { get; set; } = string.Empty; // например, USD, EUR и т.д.

    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty; // полное название валюты

    public decimal Rate { get; set; } // курс обмена относительно базовой валюты (обычно USD)

    public DateTime LastUpdated { get; set; } // дата и время последнего обновления курса


}