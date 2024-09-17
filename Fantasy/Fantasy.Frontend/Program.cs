using CurrieTechnologies.Razor.SweetAlert2;
using Fantasy.Frontend;
using Fantasy.Frontend.AuthenticationProviders;
using Fantasy.Frontend.Helpers;
using Fantasy.Frontend.Repositories;
using Fantasy.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var uriBack = "https://fantasybackend.azurewebsites.net";
//var uriBack = "https://localhost:7232";

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(uriBack) });
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddLocalization();
builder.Services.AddSweetAlert2();
builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationProviderJWT>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationProviderJWT>(x => x.GetRequiredService<AuthenticationProviderJWT>());
builder.Services.AddScoped<ILoginService, AuthenticationProviderJWT>(x => x.GetRequiredService<AuthenticationProviderJWT>());
builder.Services.AddScoped<IClipboardService, ClipboardService>();

// Register language service and localStorage service for managing language preferences
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<LocalStorageService>();

// Build the application
var host = builder.Build();

// Retrieve the language service to set the initial language based on user preferences or browser language
var languageService = host.Services.GetRequiredService<LanguageService>();

// Initialize the language preference (from localStorage or browser)
await languageService.InitializeLanguageAsync(); // This will set the initial culture based on local storage or browser

// Run the application
await host.RunAsync();