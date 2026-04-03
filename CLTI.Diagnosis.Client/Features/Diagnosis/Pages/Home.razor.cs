using CLTI.Diagnosis.Client.Components;
using CLTI.Diagnosis.Client.Features.Diagnosis.Services;
using CLTI.Diagnosis.Client.Infrastructure.Http;
using CLTI.Diagnosis.Client.Infrastructure.State;
using Microsoft.AspNetCore.Components;

namespace CLTI.Diagnosis.Client.Features.Diagnosis.Pages
{
    public partial class Home : ComponentBase
    {
        private static readonly Dictionary<string, string> StepNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["/Algoritm/Pages/KPI-PPI"] = "КПІ/ППІ",
            ["/Algoritm/Pages/Wifi_W"] = "Оцінка критерію W",
            ["/Algoritm/Pages/Wifi_I"] = "Оцінка критерію I",
            ["/Algoritm/Pages/Wifi_fI"] = "Оцінка критерію fI",
            ["/Algoritm/Pages/WiFI_results"] = "Результати WIfI",
            ["/Algoritm/Pages/CRAB"] = "CRAB",
            ["/Algoritm/Pages/2YLE"] = "2YLE",
            ["/Algoritm/Pages/SurgicalRisk"] = "Хірургічний ризик",
            ["/Algoritm/Pages/GLASS_AnatomicalStage"] = "GLASS: анатомічна стадія",
            ["/Algoritm/Pages/GLASS_FemoroPoplitealSegment"] = "GLASS: стегново-підколінний сегмент",
            ["/Algoritm/Pages/GLASS_InfrapoplitealSegment"] = "GLASS: інфрапоплітеальний сегмент",
            ["/Algoritm/Pages/GLASS_FinalStage"] = "GLASS: фінальна стадія",
            ["/Algoritm/Pages/CLTI_SubmalleolarDisease"] = "Підкісточкова хвороба",
            ["/Algoritm/Pages/RevascularizationAssessment"] = "Показання до реваскуляризації",
            ["/Algoritm/Pages/RevascularizationMethod"] = "Метод реваскуляризації"
        };

        [Inject] public CLTI.Diagnosis.Client.Features.Diagnosis.Services.CltiCaseService CaseService { get; set; } = default!;
        [Inject] public StateService StateService { get; set; } = default!;
        [Inject] public NavigationManager NavigationManager { get; set; } = default!;

        public ContextMenu? contextMenuRef;
        private readonly List<CaseListItemDto> cases = new();
        private readonly Dictionary<int, CaseListItemDto> caseLookup = new();
        private bool isLoading = true;
        private string searchText = string.Empty;

        private IEnumerable<CaseListItemDto> OpenCases =>
            cases
                .Where(c => string.Equals(c.CaseStatus, "Open", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.ModifiedAt ?? c.CreatedAt);

        private IEnumerable<CaseListItemDto> DoneCases =>
            cases
                .Where(c => string.Equals(c.CaseStatus, "Done", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.ModifiedAt ?? c.CreatedAt);

        private IEnumerable<CaseListItemDto> OpenCasesFiltered => OpenCases.Where(MatchesSearch);
        private IEnumerable<CaseListItemDto> DoneCasesFiltered => DoneCases.Where(MatchesSearch);

        protected override async Task OnInitializedAsync()
        {
            await LoadCasesAsync();
        }

        private async Task LoadCasesAsync()
        {
            isLoading = true;
            StateHasChanged();

            var fetchedCases = await CaseService.GetAllCasesAsync();

            cases.Clear();
            caseLookup.Clear();

            foreach (var caseItem in fetchedCases)
            {
                if (caseItem == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(caseItem.PatientFullName))
                {
                    caseItem.PatientFullName = "Без імені";
                }

                if (string.IsNullOrWhiteSpace(caseItem.CaseStatus))
                {
                    caseItem.CaseStatus = "Open";
                }

                cases.Add(caseItem);
                caseLookup[caseItem.CaseId] = caseItem;
            }

            isLoading = false;
            StateHasChanged();
        }

        public void OpenContextMenu(double x, double y, CaseListItemDto caseItem)
        {
            contextMenuRef?.Show(x, y, caseItem.CaseId, caseItem.PatientFullName);
        }

        private bool MatchesSearch(CaseListItemDto caseItem)
        {
            if (caseItem == null)
            {
                return false;
            }

            var query = searchText?.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            return (caseItem.PatientFullName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                   || caseItem.CaseId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)
                   || (caseItem.LastVisitedStep?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (caseItem.LastClosedStep?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private string GetLastStepDisplay(CaseListItemDto caseItem)
        {
            if (caseItem == null)
            {
                return "Останній крок: не визначено";
            }

            var isDone = string.Equals(caseItem.CaseStatus, "Done", StringComparison.OrdinalIgnoreCase);
            var rawStep = isDone ? caseItem.LastClosedStep : caseItem.LastVisitedStep;
            if (string.IsNullOrWhiteSpace(rawStep))
            {
                return "Останній крок: не визначено";
            }

            var normalized = rawStep.StartsWith("/") ? rawStep : $"/{rawStep}";
            if (StepNames.TryGetValue(normalized, out var title))
            {
                return $"Останній крок: {title}";
            }

            var shortName = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? normalized;
            return $"Останній крок: {shortName}";
        }

        private async Task HandleOpenCase(int caseId)
        {
            var loaded = await CaseService.LoadCaseAsync(caseId, StateService);
            if (!loaded || !caseLookup.TryGetValue(caseId, out var caseItem))
            {
                return;
            }

            var destination = string.Equals(caseItem.CaseStatus, "Done", StringComparison.OrdinalIgnoreCase)
                ? caseItem.LastClosedStep
                : caseItem.LastVisitedStep;

            if (string.IsNullOrWhiteSpace(destination))
            {
                destination = "/Algoritm/Pages/KPI-PPI";
            }

            NavigationManager.NavigateTo(destination, forceLoad: false);
        }

        private async Task HandleDeleteCase(int caseId)
        {
            var deleted = await CaseService.DeleteCaseAsync(caseId);
            if (!deleted)
            {
                return;
            }

            await LoadCasesAsync();
        }
    }
}
