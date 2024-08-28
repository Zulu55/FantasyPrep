using CurrieTechnologies.Razor.SweetAlert2;
using Fantasy.Frontend.Repositories;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fantasy.Frontend.Pages.Groups;

public partial class PredictionForm
{
    private EditContext editContext = null!;
    private Match? match;

    [EditorRequired, Parameter] public PredictionDTO PredictionDTO { get; set; } = null!;
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
        editContext = new(PredictionDTO);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadMathAsync();
    }

    private async Task LoadMathAsync()
    {
        var responseHttp = await Repository.GetAsync<Match>($"api/Matches/{PredictionDTO.MatchId}");
        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var messageError = await responseHttp.GetErrorMessageAsync();
                Snackbar.Add(messageError, Severity.Error);
            }
            NavigationManager.NavigateTo($"groups/details/{PredictionDTO!.GroupId}");
            return;
        }
        match = responseHttp.Response;
    }

    private void OnInvalidSubmit(EditContext editContext)
    {
        var messages = editContext.GetValidationMessages();

        foreach (var message in messages)
        {
            Snackbar.Add(message, Severity.Error);
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
        if (PredictionDTO.GoalsLocal < 0)
        {
            PredictionDTO.GoalsLocal = 0;
        }
        if (PredictionDTO.GoalsVisitor < 0)
        {
            PredictionDTO.GoalsVisitor = 0;
        }
    }
}