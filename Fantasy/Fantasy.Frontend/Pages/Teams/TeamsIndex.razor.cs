using Fantasy.Frontend.Repositories;
using Fantasy.Frontend.Resources;
using Fantasy.Shared.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Fantasy.Frontend.Pages.Teams
{
    public partial class TeamsIndex
    {
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;

        private List<Team>? Teams { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var responseHppt = await Repository.GetAsync<List<Team>>("api/teams");
            Teams = responseHppt.Response!;
        }
    }
}