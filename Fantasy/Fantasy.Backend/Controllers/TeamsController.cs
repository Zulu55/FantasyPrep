using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Fantasy.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : GenericController<Team>
{
    private readonly ITeamsUnitOfWork _teamsUnitOfWork;

    public TeamsController(IGenericUnitOfWork<Team> unitOfWork, ITeamsUnitOfWork teamsUnitOfWork) : base(unitOfWork)
    {
        _teamsUnitOfWork = teamsUnitOfWork;
    }

    [HttpGet("combo/{countryId:int}")]
    public async Task<IActionResult> GetComboAsync(int countryId)
    {
        return Ok(await _teamsUnitOfWork.GetComboAsync(countryId));
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostAsync(TeamDTO teamDTO)
    {
        var action = await _teamsUnitOfWork.AddAsync(teamDTO);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPut("full")]
    public async Task<IActionResult> PutAsync(TeamDTO teamDTO)
    {
        var action = await _teamsUnitOfWork.UpdateAsync(teamDTO);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }
}