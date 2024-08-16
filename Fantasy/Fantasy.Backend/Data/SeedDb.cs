using Fantasy.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fantasy.Backend.Data;

public class SeedDb
{
    private readonly DataContext _context;

    public SeedDb(DataContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await CheckCountriesAsync();
        await CheckTeamsAsync();
    }

    private async Task CheckCountriesAsync()
    {
        if (!_context.Countries.Any())
        {
            var countriesStatesCitiesSQLScript = File.ReadAllText("Data\\Countries.sql");
            await _context.Database.ExecuteSqlRawAsync(countriesStatesCitiesSQLScript);
        }
    }

    private async Task CheckTeamsAsync()
    {
        if (!_context.Teams.Any())
        {
            var country = await _context.Countries.FirstOrDefaultAsync(x => x.Name == "Colombia");
            _context.Teams.Add(new Team { Name = "Colombia", Country = country! });
            _context.Teams.Add(new Team { Name = "Medellín", Country = country! });
            _context.Teams.Add(new Team { Name = "Nacional", Country = country! });
            _context.Teams.Add(new Team { Name = "Millonarios", Country = country! });
            _context.Teams.Add(new Team { Name = "Junior", Country = country! });
            await _context.SaveChangesAsync();
        }
    }
}