using Fantasy.Frontend.Repositories;
using Fantasy.Frontend.Shared;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fantasy.Frontend.Pages.Tournaments;

public partial class CloseMatch
{
    private MatchDTO? matchDTO;
    private CloseForm? closeForm;
    private Match? match;

    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IRepository Repository { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    [Parameter] public int Id { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await Repository.GetAsync<Match>($"api/Matches/{Id}");

        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var messageError = await responseHttp.GetErrorMessageAsync();
                Snackbar.Add(messageError, Severity.Error);
            }
            NavigationManager.NavigateTo($"/tournament/matches/{matchDTO!.TournamentId}");
        }
        else
        {
            match = responseHttp.Response;
            matchDTO = new MatchDTO()
            {
                GoalsLocal = match!.GoalsLocal,
                GoalsVisitor = match!.GoalsVisitor,
                Id = match!.Id,
                TournamentId = match!.TournamentId,
                Date = match!.Date,
                IsActive = match!.IsActive,
                LocalId = match!.LocalId,
                VisitorId = match!.VisitorId,
                DoublePoints = match!.DoublePoints,
            };
        }
    }

    private async Task EditAsync()
    {
        if (matchDTO!.GoalsLocal == null || matchDTO.GoalsLocal < 0)
        {
            Snackbar.Add(Localizer["GoalsLocalError"], Severity.Error);
            return;
        }

        if (matchDTO!.GoalsVisitor == null || matchDTO.GoalsVisitor < 0)
        {
            Snackbar.Add(Localizer["GoalsVisitorError"], Severity.Error);
            return;
        }

        var parameters = new DialogParameters
        {
            { "Message", string.Format(Localizer["CloseMatchConfirmMessage"], match!.Local.Name, match.Visitor.Name) }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall, CloseOnEscapeKey = true };
        var dialog = DialogService.Show<ConfirmDialog>(Localizer["Confirmation"], parameters, options);
        var result = await dialog.Result;
        if (result!.Canceled)
        {
            return;
        }

        var responseHttp = await Repository.PutAsync("api/Matches/full", matchDTO);

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
        closeForm!.FormPostedSuccessfully = true;
        NavigationManager.NavigateTo($"/tournament/matches/{matchDTO!.TournamentId}");
    }
}