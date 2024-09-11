using CurrieTechnologies.Razor.SweetAlert2;
using Fantasy.Frontend.Repositories;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Fantasy.Shared.Entities;

namespace Fantasy.Frontend.Pages.Tournaments;

public partial class CloseForm
{
    private EditContext editContext = null!;
    private Match? match;

    [EditorRequired, Parameter] public MatchDTO MatchDTO { get; set; } = null!;
    [EditorRequired, Parameter] public EventCallback OnValidSubmit { get; set; }
    [EditorRequired, Parameter] public EventCallback ReturnAction { get; set; }

    public bool FormPostedSuccessfully { get; set; } = false;

    [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
    [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;
    [Inject] private IRepository Repository { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        editContext = new(MatchDTO);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadMathAsync();
    }

    private async Task LoadMathAsync()
    {
        var responseHttp = await Repository.GetAsync<Match>($"api/Matches/{MatchDTO.Id}");
        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var messageError = await responseHttp.GetErrorMessageAsync();
                Snackbar.Add(messageError, Severity.Error);
            }
            NavigationManager.NavigateTo($"/tournament/matches/{MatchDTO.TournamentId}");
            return;
        }
        match = responseHttp.Response;
    }

    private void OnInvalidSubmit(EditContext editContext)
    {
        var messages = editContext.GetValidationMessages();

        foreach (var message in messages)
        {
            Snackbar.Add(Localizer[message!], Severity.Error);
        }
    }

    private async Task OnBeforeInternalNavigation(LocationChangingContext context)
    {
        var formWasEdited = editContext.IsModified();

        if (!formWasEdited || FormPostedSuccessfully)
        {
            return;
        }

        var result = await SweetAlertService.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Confirmation"],
            Text = Localizer["LeaveAndLoseChanges"],
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            CancelButtonText = Localizer["Cancel"],
        });

        var confirm = !string.IsNullOrEmpty(result.Value);
        if (confirm)
        {
            return;
        }

        context.PreventNavigation();
    }

    private void ValidateInput()
    {
        if (MatchDTO.GoalsLocal < 0)
        {
            MatchDTO.GoalsLocal = 0;
        }
        if (MatchDTO.GoalsVisitor < 0)
        {
            MatchDTO.GoalsVisitor = 0;
        }
    }
}