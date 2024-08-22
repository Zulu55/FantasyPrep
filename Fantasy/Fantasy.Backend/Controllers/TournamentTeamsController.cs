using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fantasy.Backend.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class TournamentTeamsController : GenericController<TournamentTeam>
{
    private readonly ITournamentTeamsUnitOfWork _tournamentTeamsUnitOfWork;

    public TournamentTeamsController(IGenericUnitOfWork<TournamentTeam> unitOfWork, ITournamentTeamsUnitOfWork tournamentTeamsUnitOfWork) : base(unitOfWork)
    {
        _tournamentTeamsUnitOfWork = tournamentTeamsUnitOfWork;
    }

    [HttpGet("combo/{tournamentId}")]
    public async Task<IActionResult> GetComboAsync(int tournamentId)
    {
        return Ok(await _tournamentTeamsUnitOfWork.GetComboAsync(tournamentId));
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostAsync(TournamentTeamDTO tournamentTeamDTO)
    {
        var action = await _tournamentTeamsUnitOfWork.AddAsync(tournamentTeamDTO);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpGet("paginated")]
    public override async Task<IActionResult> GetAsync(PaginationDTO pagination)
    {
        var response = await _tournamentTeamsUnitOfWork.GetAsync(pagination);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest();
    }

    [HttpGet("totalRecordsPaginated")]
    public async Task<IActionResult> GetTotalRecordsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _tournamentTeamsUnitOfWork.GetTotalRecordsAsync(pagination);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }
}