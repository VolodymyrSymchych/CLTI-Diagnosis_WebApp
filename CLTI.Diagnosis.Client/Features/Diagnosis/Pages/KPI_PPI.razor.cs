using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CLTI.Diagnosis.Client.Features.Diagnosis.Pages
{
    public partial class KPI_PPI : IDisposable
    {
        public string kpiValueString = "";
        private string ppiValue = "";
        private Action? onStateChanged;

        [Inject]
        public CLTI.Diagnosis.Client.Features.Diagnosis.Services.CltiCaseService? CaseService { get; set; }
        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        protected override void OnInitialized()
        {
            onStateChanged = () => InvokeAsync(StateHasChanged);
            StateService.OnChange += onStateChanged;
        }

        private bool HasKpiValue() => StateService.KpiValue > 0;

        private void HandlePatientNameChanged(string value)
        {
            StateService.PatientFullName = string.IsNullOrWhiteSpace(value) ? "Без імені" : value.Trim();
            StateService.NotifyStateChanged();
        }

        private void HandleKpiInputChanged(string value)
        {
            kpiValueString = value;
            ProcessKpiValue();
        }

        private void ProcessKpiValue()
        {
            if (double.TryParse(kpiValueString.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double value))
            {
                StateService.UpdateKpiValue(value);
            }
            else
            {
                StateService.UpdateKpiValue(0);
            }
        }

        private void HandlePpiInputChanged(string value)
        {
            ppiValue = value;
            ProcessPpiValue();
        }

        private void ProcessPpiValue()
        {
            if (double.TryParse(ppiValue.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double value))
            {
                StateService.UpdatePpiValue(value);
            }
            else
            {
                StateService.UpdatePpiValue(0);
            }
        }

        private async void Continue()
        {
            if (CaseService != null)
            {
                await CaseService.SaveCaseAsync(StateService);
            }
            await InvokeAsync(StateHasChanged);
            NavigationManager.NavigateTo("/Algoritm/Pages/Wifi_W", forceLoad: false);
            StateService.ShowWifiSection = true;
        }

        private async void Exit()
        {
            if (CaseService != null)
            {
                var saved = await CaseService.SaveCaseAsync(StateService);
                if (!saved)
                {
                    await JSRuntime.InvokeVoidAsync("alert", "Не вдалося зберегти кейс. Перевірте авторизацію та підключення до серверу.");
                    return;
                }
            }

            await InvokeAsync(StateHasChanged);
            NavigationManager.NavigateTo("/", forceLoad: false);
        }

        public void Dispose()
        {
            if (onStateChanged is not null)
                StateService.OnChange -= onStateChanged;
        }
    }
}
