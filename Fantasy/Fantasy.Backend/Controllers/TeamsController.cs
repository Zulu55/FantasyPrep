using Fantasy.Backend.Data;
using Fantasy.Shared.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fantasy.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly DataContext _context;

    public TeamsController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        return Ok(await _context.Teams.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var country = await _context.Teams.FirstOrDefaultAsync(c => c.Id == id);
        if (country == null)
        {
            return NotFound();
        }

        return Ok(country);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(Team team)
    {
        _context.Add(team);
        await _context.SaveChangesAsync();
        return Ok(team);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var country = await _context.Teams.FirstOrDefaultAsync(c => c.Id == id);
        if (country == null)
        {
            return NotFound();
        }

        _context.Remove(country);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(Team team)
    {
        _context.Update(team);
        await _context.SaveChangesAsync();
        return Ok(team);
    }
}