using Fantasy.Frontend.Repositories;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fantasy.Frontend.Pages.Groups;

[Authorize(Roles = "Admin, User")]
public partial class Predictions
{
    private List<Prediction>? predictions;
    private MudTable<Prediction> table = new();
    private readonly int[] pageSizeOptions = { 10, 25, 50, int.MaxValue };
    private int totalRecords = 0;
    private bool loading;
    private const string baseUrlMatch = "api/predictions";
    private string infoFormat = "{first_item}-{last_item} de {all_items}";
    private bool userEnabledForGroup;
    private string username = string.Empty;

    [Parameter] public int GroupId { get; set; }

    [Inject] private IRepository Repository { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
        await LoadUserNameAsync();
        await CheckUserEnabledAsync();
    }

    private async Task CheckUserEnabledAsync()
    {
        var responseHttp = await Repository.GetAsync<UserGroup>($"api/userGroups/{GroupId}/{username}");

        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var messageError = await responseHttp.GetErrorMessageAsync();
                Snackbar.Add(messageError, Severity.Error);
            }
        }
        var userGroup = responseHttp.Response;
        userEnabledForGroup = userGroup!.IsActive;
    }

    private async Task LoadUserNameAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            username = user.Identity.Name!;
        }
    }

    private async Task LoadAsync()
    {
        await LoadTotalRecords();
    }

    private async Task<bool> LoadTotalRecords()
    {
        loading = true;

        var url = $"{baseUrlMatch}/totalRecordsPaginated/?id={GroupId}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await Repository.GetAsync<int>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(Localizer[message], Severity.Error);
            return false;
        }
        totalRecords = responseHttp.Response;
        loading = false;
        return true;
    }

    private async Task<TableData<Prediction>> LoadListAsync(TableState state, CancellationToken cancellationToken)
    {
        int page = state.Page + 1;
        int pageSize = state.PageSize;
        var url = $"{baseUrlMatch}/paginated?id={GroupId}&page={page}&recordsnumber={pageSize}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }

        var responseHttp = await Repository.GetAsync<List<Prediction>>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(Localizer[message], Severity.Error);
            return new TableData<Prediction> { Items = [], TotalItems = 0 };
        }
        if (responseHttp.Response == null)
        {
            return new TableData<Prediction> { Items = [], TotalItems = 0 };
        }
        return new TableData<Prediction>
        {
            Items = responseHttp.Response,
            TotalItems = totalRecords
        };
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await LoadAsync();
        await table.ReloadServerData();
    }

    private void ReturnAction()
    {
        NavigationManager.NavigateTo("/groups");
    }

    private async Task EditPredictionAsync(int id)
    {
        var options = new DialogOptions() { CloseOnEscapeKey = true, CloseButton = true };
        var parameters = new DialogParameters
        {
            { "Id", id }
        };
        var dialog = DialogService.Show<PredictionEdit>($"{Localizer["Edit"]} {Localizer["Prediction"]}", parameters, options);

        var result = await dialog.Result;
        if (result!.Canceled)
        {
            await LoadAsync();
            await table.ReloadServerData();
        }
    }

    private async Task WatchPredictionsAsync(Prediction prediction)
    {
        var options = new DialogOptions()
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };
        var parameters = new DialogParameters
        {
            { "GroupId", prediction.GroupId },
            { "MatchId", prediction.MatchId }
        };
        var dialog = DialogService.Show<WatchPredictions>($"{Localizer["Watch"]} {Localizer["Predictions"]}", parameters, options);

        await dialog.Result;
    }

    private bool CanWatch(Prediction prediction)
    {
        if (prediction.Points != null)
        {
            return true;
        }

        var difference = DateTime.Now - prediction.Match.Date;
        var minutes = difference.TotalMinutes;
        return minutes >= 10;
    }
}