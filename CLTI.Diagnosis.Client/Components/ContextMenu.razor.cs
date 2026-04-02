using Microsoft.AspNetCore.Components;

namespace CLTI.Diagnosis.Client.Components
{
    public partial class ContextMenu
    {
        [Parameter] public EventCallback<int> OnViewCase { get; set; }
        [Parameter] public EventCallback<int> OnDeleteCase { get; set; }

        private bool Hidden { get; set; } = true;
        private string menuTopPx = "0px";
        private string menuLeftPx = "0px";

        private int targetCaseId;
        private string? targetName;

        public void Show(double x, double y, int caseId, string name)
        {
            menuLeftPx = $"{x}px";
            menuTopPx = $"{y}px";
            targetCaseId = caseId;
            targetName = name;
            Hidden = false;
            StateHasChanged();
        }

        public void Hide()
        {
            Hidden = true;
            StateHasChanged();
        }

        private async Task OnView()
        {
            if (OnViewCase.HasDelegate)
            {
                await OnViewCase.InvokeAsync(targetCaseId);
            }
            Hide();
        }

        private async Task OnDelete()
        {
            if (OnDeleteCase.HasDelegate)
            {
                await OnDeleteCase.InvokeAsync(targetCaseId);
            }
            Hide();
        }
    }
}
