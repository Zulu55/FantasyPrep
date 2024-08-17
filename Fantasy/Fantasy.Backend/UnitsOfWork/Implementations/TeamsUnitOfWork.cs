using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;

namespace Fantasy.Backend.UnitsOfWork.Implementations;

public class TeamsUnitOfWork : GenericUnitOfWork<Team>, ITeamsUnitOfWork
{
    private readonly ITeamsRepository _teamsRepository;

    public TeamsUnitOfWork(IGenericRepository<Team> repository, ITeamsRepository teamsRepository) : base(repository)
    {
        _teamsRepository = teamsRepository;
    }

    public async Task<ActionResponse<Team>> AddAsync(TeamDTO teamDTO) => await _teamsRepository.AddAsync(teamDTO);

    public async Task<IEnumerable<Team>> GetComboAsync(int countryId) => await _teamsRepository.GetComboAsync(countryId);

    public async Task<ActionResponse<Team>> UpdateAsync(TeamDTO teamDTO) => await _teamsRepository.UpdateAsync(teamDTO);
}