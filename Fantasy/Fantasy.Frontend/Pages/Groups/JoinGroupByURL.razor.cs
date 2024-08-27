using Fantasy.Frontend.Repositories;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fantasy.Frontend.Pages.Groups;

public partial class JoinGroupByURL
{
    private string? message;
    private Group? group;

    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IRepository Repository { get; set; } = null!;
    [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;

    [Parameter, SupplyParameterFromQuery] public string Code { get; set; } = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        var responseHttp = await Repository.GetAsync<Group>($"api/groups/code/{Code}");

        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Snackbar.Add(Localizer["ERR017"], Severity.Error);
                NavigationManager.NavigateTo("groups");
            }
            else
            {
                var messageError = await responseHttp.GetErrorMessageAsync();
                Snackbar.Add(messageError, Severity.Error);
                NavigationManager.NavigateTo("/");
            }
            return;
        }
        group = responseHttp.Response;
    }

    protected async Task JoinGroupAsync()
    {
        var responseHttp = await Repository.PostAsync($"/api/usergroups/join?code={Code}", new JoinGroupDTO { Code = Code });
        if (responseHttp.Error)
        {
            message = await responseHttp.GetErrorMessageAsync();
            NavigationManager.NavigateTo("/");
            Snackbar.Add(message, Severity.Error);
            return;
        }

        Snackbar.Add(Localizer["UserAddedToGroupOk"], Severity.Success);
        var closeOnEscapeKey = new DialogOptions() { CloseOnEscapeKey = true };
        NavigationManager.NavigateTo("/groups");
    }
}