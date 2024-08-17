using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;

namespace Fantasy.Backend.UnitsOfWork.Interfaces;

public interface ITeamsUnitOfWork
{
    Task<IEnumerable<Team>> GetComboAsync(int countryId);

    Task<ActionResponse<Team>> AddAsync(TeamDTO teamDTO);

    Task<ActionResponse<Team>> UpdateAsync(TeamDTO teamDTO);
}