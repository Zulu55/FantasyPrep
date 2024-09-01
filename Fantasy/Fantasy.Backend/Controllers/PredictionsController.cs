using Fantasy.Backend.UnitsOfWork.Implementations;
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
public class PredictionsController : GenericController<Prediction>
{
    private readonly IPredictionsUnitOfWork _predictionsUnitOfWork;

    public PredictionsController(IGenericUnitOfWork<Prediction> unitOfWork, IPredictionsUnitOfWork predictionsUnitOfWork) : base(unitOfWork)
    {
        _predictionsUnitOfWork = predictionsUnitOfWork;
    }

    [HttpGet("paginatedBalance")]
    public async Task<IActionResult> GetBalanceAsync([FromQuery] PaginationDTO pagination)
    {
        var response = await _predictionsUnitOfWork.GetBalanceAsync(pagination);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest();
    }

    [HttpGet("totalRecordsBalance")]
    public async Task<IActionResult> GetTotalRecordsBalanceAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _predictionsUnitOfWork.GetTotalRecordsBalanceAsync(pagination);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [HttpGet("paginatedAllPredictions")]
    public async Task<IActionResult> GetAllPredictionsAsync([FromQuery] PaginationDTO pagination)
    {
        var response = await _predictionsUnitOfWork.GetAllPredictionsAsync(pagination);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest();
    }

    [HttpGet("totalRecordsPaginatedAllPredictions")]
    public async Task<IActionResult> GetTotalRecordsAllPredictionsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _predictionsUnitOfWork.GetTotalRecordsAllPredictionsAsync(pagination);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [AllowAnonymous]
    [HttpGet("positions")]
    public async Task<IActionResult> GetPositionsAsync([FromQuery] PaginationDTO pagination)
    {
        var response = await _predictionsUnitOfWork.GetPositionsAsync(pagination);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest();
    }

    [AllowAnonymous]
    [HttpGet("totalRecordsForPositionsPaginated")]
    public async Task<IActionResult> GetTotalRecordsForPositionsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _predictionsUnitOfWork.GetTotalRecordsForPositionsAsync(pagination);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [HttpGet("paginated")]
    public override async Task<IActionResult> GetAsync(PaginationDTO pagination)
    {
        pagination.Email = User.Identity!.Name;
        var response = await _predictionsUnitOfWork.GetAsync(pagination);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest();
    }

    [HttpGet("totalRecordsPaginated")]
    public async Task<IActionResult> GetTotalRecordsAsync([FromQuery] PaginationDTO pagination)
    {
        pagination.Email = User.Identity!.Name;
        var action = await _predictionsUnitOfWork.GetTotalRecordsAsync(pagination);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [HttpGet("{id}")]
    public override async Task<IActionResult> GetAsync(int id)
    {
        var response = await _predictionsUnitOfWork.GetAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostAsync(PredictionDTO predictionDTO)
    {
        var action = await _predictionsUnitOfWork.AddAsync(predictionDTO);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }

    [HttpPut("full")]
    public async Task<IActionResult> PutAsync(PredictionDTO predictionDTO)
    {
        var action = await _predictionsUnitOfWork.UpdateAsync(predictionDTO);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }
}