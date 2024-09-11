using Fantasy.Frontend.Helpers;
using Fantasy.Shared.Resources;
using Microsoft.Extensions.Localization;
using System.Globalization;

public class LanguageService
{
    private readonly IStringLocalizer<Literals> _localizer;
    private readonly LocalStorageService _localStorageService;
    private const string LanguageKey = "preferredLanguage";

    public string CurrentLanguage { get; private set; }

    public LanguageService(IStringLocalizer<Literals> localizer, LocalStorageService localStorageService)
    {
        _localizer = localizer;
        _localStorageService = localStorageService;
        CurrentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }

    public async Task InitializeLanguageAsync()
    {
        var savedLanguage = await _localStorageService.GetItemAsync(LanguageKey);
        if (!string.IsNullOrEmpty(savedLanguage))
        {
            SetLanguage(savedLanguage);
        }
    }

    public async void SetLanguage(string languageCode)
    {
        var culture = new CultureInfo(languageCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CurrentLanguage = languageCode;

        await _localStorageService.SetItemAsync(LanguageKey, languageCode);
    }
}