using CLTI.Diagnosis.Client.Extensions;
using CLTI.Diagnosis.Client.Infrastructure.Http;
using CLTI.Diagnosis.Client.Infrastructure.State;
using Microsoft.AspNetCore.Components;

namespace CLTI.Diagnosis.Client.Features.Diagnosis.Services
{
    public class CltiCaseService
    {
        private readonly CltiApiClient _apiClient;
        private readonly NavigationManager _navigationManager;

        public CltiCaseService(CltiApiClient apiClient, NavigationManager navigationManager)
        {
            _apiClient = apiClient;
            _navigationManager = navigationManager;
        }

        public async Task<bool> SaveCaseAsync(StateService stateService, bool markAsDone = false)
        {
            try
            {
                var currentStep = GetCurrentStepRoute();
                stateService.LastVisitedStep = currentStep;
                stateService.CaseStatus = markAsDone ? "Done" : "Open";

                if (markAsDone)
                {
                    stateService.LastClosedStep = currentStep;
                }

                var dto = stateService.ToDto();
                var response = await _apiClient.SaveCaseAsync(dto);

                if (response.Success && response.Data != null)
                {
                    stateService.CaseId = response.Data.CaseId;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> LoadCaseAsync(int caseId, StateService stateService)
        {
            try
            {
                var response = await _apiClient.GetCaseAsync(caseId);

                if (response.Success && response.Data != null)
                {
                    stateService.UpdateFromDto(response.Data);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<CaseListItemDto>> GetAllCasesAsync()
        {
            try
            {
                var response = await _apiClient.GetAllCasesAsync();
                return response.Success && response.Data != null
                    ? response.Data
                    : new List<CaseListItemDto>();
            }
            catch (Exception)
            {
                return new List<CaseListItemDto>();
            }
        }

        public async Task<bool> DeleteCaseAsync(int caseId)
        {
            try
            {
                var response = await _apiClient.DeleteCaseAsync(caseId);
                return response.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetCurrentStepRoute()
        {
            var relativePath = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
            var pathWithoutQuery = relativePath.Split('?', '#')[0];
            return string.IsNullOrWhiteSpace(pathWithoutQuery) ? "/" : $"/{pathWithoutQuery.TrimStart('/')}";
        }
    }
}
