using Fantasy.Frontend.Repositories;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fantasy.Frontend.Pages.Groups;

public partial class PredictionEdit
{
    private PredictionDTO? predictionDTO;
    private PredictionForm? predictionForm;

    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IRepository Repository { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;

    [Parameter] public int Id { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await Repository.GetAsync<Prediction>($"api/Predictions/{Id}");

        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var messageError = await responseHttp.GetErrorMessageAsync();
                Snackbar.Add(messageError, Severity.Error);
            }
            NavigationManager.NavigateTo($"groups/details/{predictionDTO!.GroupId}");
        }
        else
        {
            var prediction = responseHttp.Response;
            predictionDTO = new PredictionDTO()
            {
                GoalsLocal = prediction!.GoalsLocal,
                GoalsVisitor = prediction!.GoalsVisitor,
                GroupId = prediction!.GroupId,
                Id = prediction!.Id,
                MatchId = prediction!.MatchId,
                Points = prediction!.Points,
                TournamentId = prediction!.TournamentId,
                UserId = prediction!.UserId,
            };
        }
    }

    private async Task EditAsync()
    {
        var responseHttp = await Repository.PutAsync("api/Predictions/full", predictionDTO);

        if (responseHttp.Error)
        {
            var mensajeError = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(Localizer[mensajeError!], Severity.Error);
            return;
        }

        Return();
        Snackbar.Add(Localizer["RecordSavedOk"], Severity.Success);
    }

    private void Return()
    {
        predictionForm!.FormPostedSuccessfully = true;
        NavigationManager.NavigateTo($"groups/details/{predictionDTO!.GroupId}/false");
    }
}