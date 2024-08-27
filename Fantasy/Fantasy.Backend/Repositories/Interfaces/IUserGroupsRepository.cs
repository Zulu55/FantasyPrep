using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;

namespace Fantasy.Backend.Repositories.Interfaces;

public interface IUserGroupsRepository
{
    Task<ActionResponse<UserGroup>> JoinAsync(JoinGroupDTO joinGroupDTO);

    Task<ActionResponse<UserGroup>> AddAsync(UserGroupDTO userGroupDTO);

    Task<ActionResponse<UserGroup>> UpdateAsync(UserGroupDTO userGroupDTO);

    Task<ActionResponse<UserGroup>> GetAsync(int id);

    Task<ActionResponse<IEnumerable<UserGroup>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<int>> GetTotalRecordsAsync(PaginationDTO pagination);
}