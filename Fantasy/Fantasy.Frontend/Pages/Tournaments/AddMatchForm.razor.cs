using CurrieTechnologies.Razor.SweetAlert2;
using Fantasy.Frontend.Repositories;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fantasy.Frontend.Pages.Tournaments;

public partial class AddMatchForm
{
    private EditContext editContext = null!;
    private Team selectedLocal = new();
    private Team selectedVisitor = new();
    private List<Team>? teams;
    private string? imageUrlLocal;
    private string? imageUrlVisitor;
    private string? isActiveMessage;

    [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
    [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;
    [Inject] private IRepository Repository { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [EditorRequired, Parameter] public MatchDTO MatchDTO { get; set; } = null!;
    [EditorRequired, Parameter] public EventCallback OnValidSubmit { get; set; }
    [EditorRequired, Parameter] public EventCallback ReturnAction { get; set; }

    private DateTime? selectedDate { get; set; } = DateTime.Now.Date;
    private TimeSpan? selectedTime { get; set; } = DateTime.Now.TimeOfDay;
    public bool FormPostedSuccessfully { get; set; } = false;

    protected override void OnInitialized()
    {
        editContext = new(MatchDTO);
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadMatchesAsync();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        isActiveMessage = MatchDTO.IsActive ? Localizer["MatchActive"] : Localizer["MatchInactive"];
        MatchDTO.Date = DateTime.Now;
    }

    private async Task LoadMatchesAsync()
    {
        var responseHttp = await Repository.GetAsync<List<TournamentTeam>>($"/api/tournamentTeams/combo/{MatchDTO.TournamentId}");
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(message, Severity.Error);
            return;
        }

        var tournamentTeams = responseHttp.Response;
        teams = tournamentTeams!.Select(t => t.Team).ToList();
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

    private async Task<IEnumerable<Team>> SearchTeam(string searchText, CancellationToken cancellationToken)
    {
        await Task.Delay(5);
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return teams!;
        }

        return teams!
            .Where(x => x.Name.Contains(searchText, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
    }

    private void LocalChanged(Team team)
    {
        selectedLocal = team;
        MatchDTO.LocalId = team.Id;
        imageUrlLocal = team.ImageFull;
    }

    private void VisitorChanged(Team team)
    {
        selectedVisitor = team;
        MatchDTO.VisitorId = team.Id;
        imageUrlVisitor = team.ImageFull;
    }

    private void OnDateChanged(DateTime? newDate)
    {
        selectedDate = newDate;
        UpdateMatchDate();
    }

    private void OnTimeChanged(TimeSpan? newTime)
    {
        selectedTime = newTime;
        UpdateMatchDate();
    }

    private void UpdateMatchDate()
    {
        if (selectedDate.HasValue && selectedTime.HasValue)
        {
            MatchDTO.Date = selectedDate.Value.Date + selectedTime.Value;
        }
    }

    private void SetTournamentOff()
    {
        MatchDTO.IsActive = false;
        isActiveMessage = Localizer["MatchInactive"];
    }

    private void SetTournamentOn()
    {
        MatchDTO.IsActive = true;
        isActiveMessage = Localizer["MatchActive"];
    }

    private async Task OnSubmitAsync()
    {
        if (ValidateForm())
        {
            await OnValidSubmit.InvokeAsync(null);
        }
    }

    private bool ValidateForm()
    {
        var hasErros = false;
        if (selectedLocal.Id == 0)
        {
            Snackbar.Add(Localizer["MustSelectLocalTeam"], Severity.Error);
            hasErros = true;
        }

        if (selectedVisitor.Id == 0)
        {
            Snackbar.Add(Localizer["MustSelectVisitorTeam"], Severity.Error);
            hasErros = true;
        }

        return !hasErros;
    }
}