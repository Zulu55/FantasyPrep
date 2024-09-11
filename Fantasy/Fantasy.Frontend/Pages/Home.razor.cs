using Fantasy.Frontend.Helpers;
using Fantasy.Frontend.Pages.Groups;
using Fantasy.Frontend.Repositories;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fantasy.Frontend.Pages;

public partial class Home
{
    private const string baseUrl = "api/groups";
    private List<Group>? Groups { get; set; }

    [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;
    [Inject] private IRepository Repository { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IClipboardService ClipboardService { get; set; } = null!;
    [Inject] private IStringLocalizer<Parameters> Parameters { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private LanguageService LanguageService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private string selectedLanguage = "es"; // Default to Spanish

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadGroupsAsync();
        selectedLanguage = LanguageService.CurrentLanguage;
    }

    private void ChangeLanguage(string language)
    {
        LanguageService.SetLanguage(language);
        NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
    }

    private async Task LoadGroupsAsync()
    {
        var url = $"{baseUrl}/all";
        var responseHttp = await Repository.GetAsync<List<Group>>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            Snackbar.Add(Localizer[message!], Severity.Error);
            return;
        }
        Groups = responseHttp.Response;
    }

    private async Task CopyInvitationAsync(Group group)
    {
        var joinURL = $"{Parameters["URLFront"]}/groups/join/?code={group!.Code}";
        await ClipboardService.CopyToClipboardAsync(joinURL);
        var text = string.Format(Localizer["InvitationURLCopied"], group!.Name);
        Snackbar.Add(text, Severity.Success);
    }

    private async Task GroupDetailsAsync(Group group)
    {
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
                { "GroupId", group.Id },
                { "IsAnonymouns", true }
            };
            var dialog = DialogService.Show<GroupDetails>(@Localizer["GroupDetails"], parameters, options);
            await dialog.Result;
        }
    }
}