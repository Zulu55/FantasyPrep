﻿using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fantasy.Backend.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class UserGroupsController : GenericController<UserGroup>
{
    private readonly IUserGroupsUnitOfWork _userGroupsUnitOfWork;

    public UserGroupsController(IGenericUnitOfWork<UserGroup> unitOfWork, IUserGroupsUnitOfWork userGroupsUnitOfWork) : base(unitOfWork)
    {
        _userGroupsUnitOfWork = userGroupsUnitOfWork;
    }

    [HttpGet("paginated")]
    public override async Task<IActionResult> GetAsync(PaginationDTO pagination)
    {
        var response = await _userGroupsUnitOfWork.GetAsync(pagination);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest();
    }

    [HttpGet("totalRecordsPaginated")]
    public async Task<IActionResult> GetTotalRecordsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _userGroupsUnitOfWork.GetTotalRecordsAsync(pagination);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [HttpGet("{id}")]
    public override async Task<IActionResult> GetAsync(int id)
    {
        var response = await _userGroupsUnitOfWork.GetAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpGet("{groupId}/{email}")]
    public async Task<IActionResult> GetAsync(int groupId, string email)
    {
        var response = await _userGroupsUnitOfWork.GetAsync(groupId, email);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPost("join")]
    public async Task<IActionResult> PostAsync(JoinGroupDTO joinGroupDTO)
    {
        joinGroupDTO.UserName = User.Identity!.Name!;
        var action = await _userGroupsUnitOfWork.JoinAsync(joinGroupDTO);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostAsync(UserGroupDTO userGroupDTO)
    {
        var action = await _userGroupsUnitOfWork.AddAsync(userGroupDTO);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPut("full")]
    public async Task<IActionResult> PutAsync(UserGroupDTO userGroupDTO)
    {
        var action = await _userGroupsUnitOfWork.UpdateAsync(userGroupDTO);
        if (action.WasSuccess)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }
}