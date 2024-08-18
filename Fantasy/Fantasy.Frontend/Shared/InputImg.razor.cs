using Fantasy.Frontend.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace Fantasy.Frontend.Shared;

public partial class InputImg
{
    private string? imageBase64;

    [Inject] private IStringLocalizer<Literals> Localizer { get; set; } = null!;

    [Parameter] public string Label { get; set; }
    [Parameter] public string? ImageURL { get; set; }
    [Parameter] public EventCallback<string> ImageSelected { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (string.IsNullOrWhiteSpace(Label))
        {
            Label = Localizer["Image"];
        }
    }

    private async Task OnChange(InputFileChangeEventArgs e)
    {
        var imagenes = e.GetMultipleFiles();

        foreach (var imagen in imagenes)
        {
            var arrBytes = new byte[imagen.Size];
            await imagen.OpenReadStream().ReadAsync(arrBytes);
            imageBase64 = Convert.ToBase64String(arrBytes);
            ImageURL = null;
            await ImageSelected.InvokeAsync(imageBase64);
            StateHasChanged();
        }
    }
}