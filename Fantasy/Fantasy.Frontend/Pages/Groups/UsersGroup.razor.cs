using Fantasy.Frontend.Repositories;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fantasy.Frontend.Pages.Groups;

public partial class UsersGroup
{
    private List<UserGroup>? userGroups;
    private MudTable<UserGroup> table = new();
    private readonly int[] pageSizeOptions = { 10, 25, 50, int.MaxValue };
    private int totalRecords = 0;
    private bool loading;
    private const string baseUrl = "api/userGroups";
    private string infoFormat = "{first_item}-{last_item} de {all_items}";

    [Parameter] public int GroupId { get; set; }

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

        var url = $"{baseUrl}/totalRecordsPaginated/?id={GroupId}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await Repository.GetAsync<int>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(Localizer[message!], Severity.Error);
            return false;
        }
        totalRecords = responseHttp.Response;
        loading = false;
        return true;
    }

    private async Task<TableData<UserGroup>> LoadListAsync(TableState state, CancellationToken cancellationToken)
    {
        int page = state.Page + 1;
        int pageSize = state.PageSize;
        var url = $"{baseUrl}/paginated/?id={GroupId}&page={page}&recordsnumber={pageSize}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }

        var responseHttp = await Repository.GetAsync<List<UserGroup>>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(Localizer[message!], Severity.Error);
            return new TableData<UserGroup> { Items = [], TotalItems = 0 };
        }
        if (responseHttp.Response == null)
        {
            return new TableData<UserGroup> { Items = [], TotalItems = 0 };
        }
        return new TableData<UserGroup>
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
        NavigationManager.NavigateTo($"/groups");
    }

    private async Task ActivateUserGroupAsync(UserGroup userGroup)
    {
        userGroup.IsActive = true;
        await UpdateUserGroupAsync(userGroup);
    }

    private async Task DectivateUserGroupAsync(UserGroup userGroup)
    {
        userGroup.IsActive = false;
        await UpdateUserGroupAsync(userGroup);
    }

    private async Task UpdateUserGroupAsync(UserGroup userGroup)
    {
        var userGroupDTO = new UserGroupDTO
        {
            IsActive = userGroup.IsActive,
            Id = userGroup.Id,
            GroupId = userGroup.Id,
            UserId = userGroup.UserId,
        };
        var responseHttp = await Repository.PutAsync($"{baseUrl}/full", userGroupDTO);

        if (responseHttp.Error)
        {
            var messageError = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(messageError, Severity.Error);
            return;
        }

        Snackbar.Add(Localizer["RecordSavedOk"], Severity.Success);
    }
}