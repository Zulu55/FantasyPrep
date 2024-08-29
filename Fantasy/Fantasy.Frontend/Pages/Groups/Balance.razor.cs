using Fantasy.Frontend.Repositories;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fantasy.Frontend.Pages.Groups;

public partial class Balance
{
    private List<Prediction>? predictions;
    private MudTable<Prediction> table = new();
    private readonly int[] pageSizeOptions = { 10, 25, 50, int.MaxValue };
    private int totalRecords = 0;
    private bool loading;
    private const string baseUrl = "api/predictions";
    private string infoFormat = "{first_item}-{last_item} de {all_items}";
    private Match? match;

    [Parameter] public int GroupId { get; set; }
    [Parameter] public string Email { get; set; } = null!;

    [Inject] private IRepository Repository { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;

    [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        await LoadTotalRecords();
    }

    private async Task<bool> LoadTotalRecords()
    {
        loading = true;

        var url = $"{baseUrl}/totalRecordsBalance/?id={GroupId}&email={Email}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await Repository.GetAsync<int>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(message, Severity.Error);
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
        var url = $"{baseUrl}/paginatedBalance/?id={GroupId}&email={Email}&page={page}&recordsnumber={pageSize}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }

        var responseHttp = await Repository.GetAsync<List<Prediction>>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(message, Severity.Error);
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
        NavigationManager.NavigateTo($"/groups/details/{GroupId}");
    }
}