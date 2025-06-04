using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace BlazorTestApp.Components.Layout;

public partial class MainLayout
{
    // внедряем сервисы
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected NotificationService NotificationService { get; set; } = default!;

    private bool sidebarExpanded = true; // боковая панель открыта по умолчанию

 
    //Переключает отображение боковой панели
    //Простой, но эффективный метод
    void SidebarToggleClick()
    {
        sidebarExpanded = !sidebarExpanded;
    }
}