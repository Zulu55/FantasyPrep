using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fantasy.Backend.Repositories.Implementations;

public class GroupsRepository : GenericRepository<Group>, IGroupsRepository
{
    private readonly DataContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly IUsersRepository _usersRepository;

    public GroupsRepository(DataContext context, IFileStorage fileStorage, IUsersRepository usersRepository) : base(context)
    {
        _context = context;
        _fileStorage = fileStorage;
        _usersRepository = usersRepository;
    }

    public override async Task<ActionResponse<IEnumerable<Group>>> GetAsync(PaginationDTO pagination)
    {
        var queryable = _context.Groups
            .Include(x => x.Members!)
            .ThenInclude(x => x.User)
            .Include(x => x.Tournament)
            .AsQueryable();
        queryable = queryable.Where(x => x.Members!.Any(x => x.User.Email == pagination.Email));

        if (!string.IsNullOrWhiteSpace(pagination.Filter))
        {
            queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
        }

        return new ActionResponse<IEnumerable<Group>>
        {
            WasSuccess = true,
            Result = await queryable
                .OrderBy(x => x.Name)
                .Paginate(pagination)
                .ToListAsync()
        };
    }

    public override async Task<ActionResponse<Group>> GetAsync(int id)
    {
        var group = await _context.Groups
            .Include(x => x.Members!)
            .ThenInclude(x => x.User)
            .Include(x => x.Tournament)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (group == null)
        {
            return new ActionResponse<Group>
            {
                WasSuccess = false,
                Message = "ERR001"
            };
        }

        return new ActionResponse<Group>
        {
            WasSuccess = true,
            Result = group
        };
    }

    public async Task<ActionResponse<Group>> AddAsync(GroupDTO groupDTO)
    {
        var admin = await _usersRepository.GetUserAsync(groupDTO.AdminId);
        if (admin == null)
        {
            return new ActionResponse<Group>
            {
                WasSuccess = false,
                Message = "ERR013"
            };
        }

        var tournament = await _context.Tournaments.FindAsync(groupDTO.TournamentId);
        if (tournament == null)
        {
            return new ActionResponse<Group>
            {
                WasSuccess = false,
                Message = "ERR009"
            };
        }

        var code = string.Empty;
        var exists = true;
        do
        {
            code = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            var currentGroup = await _context.Groups.FirstOrDefaultAsync(x => x.Code == code);
            exists = currentGroup != null;
        } while (exists);

        var group = new Group
        {
            Admin = admin,
            Tournament = tournament,
            Code = code,
            IsActive = true,
            Name = groupDTO.Name,
            Remarks = groupDTO.Remarks,
            Members = [
                new UserGroup { User = admin, IsActive = true },
            ]
        };

        if (!string.IsNullOrEmpty(groupDTO.Image))
        {
            var imageBase64 = Convert.FromBase64String(groupDTO.Image!);
            group.Image = await _fileStorage.SaveFileAsync(imageBase64, ".jpg", "groups");
        }

        _context.Add(group);
        try
        {
            await _context.SaveChangesAsync();
            return new ActionResponse<Group>
            {
                WasSuccess = true,
                Result = group
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<Group>
            {
                WasSuccess = false,
                Message = "ERR003"
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Group>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<int>> GetTotalRecordsAsync(PaginationDTO pagination)
    {
        var queryable = _context.Groups.AsQueryable();
        queryable = queryable.Where(x => x.Members!.Any(x => x.User.Email == pagination.Email));

        if (!string.IsNullOrWhiteSpace(pagination.Filter))
        {
            queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
        }

        double count = await queryable.CountAsync();
        return new ActionResponse<int>
        {
            WasSuccess = true,
            Result = (int)count
        };
    }

    public async Task<ActionResponse<Group>> UpdateAsync(GroupDTO groupDTO)
    {
        var currentGroup = await _context.Groups.FindAsync(groupDTO.Id);
        if (currentGroup == null)
        {
            return new ActionResponse<Group>
            {
                WasSuccess = false,
                Message = "ERR014"
            };
        }

        if (!string.IsNullOrEmpty(groupDTO.Image))
        {
            var imageBase64 = Convert.FromBase64String(groupDTO.Image!);
            currentGroup.Image = await _fileStorage.SaveFileAsync(imageBase64, ".jpg", "groups");
        }

        currentGroup.Name = groupDTO.Name;
        currentGroup.IsActive = groupDTO.IsActive;
        currentGroup.Remarks = groupDTO.Remarks;

        _context.Update(currentGroup);
        try
        {
            await _context.SaveChangesAsync();
            return new ActionResponse<Group>
            {
                WasSuccess = true,
                Result = currentGroup
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<Group>
            {
                WasSuccess = false,
                Message = "ERR003"
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Group>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<Group>> GetAsync(string code)
    {
        var group = await _context.Groups.FirstOrDefaultAsync(x => x.Code == code);

        if (group == null)
        {
            return new ActionResponse<Group>
            {
                WasSuccess = false,
                Message = "ERR001"
            };
        }

        return new ActionResponse<Group>
        {
            WasSuccess = true,
            Result = group
        };
    }

    public async Task CheckPredictionsForAllMatchesAsync(int id)
    {
        var group = await _context.Groups
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (group == null)
        {
            return;
        }

        var tournament = await _context.Tournaments
            .Include(x => x.Matches)
            .FirstOrDefaultAsync(x => x.Id == group.TournamentId);
        if (group == null)
        {
            return;
        }

        var wasChanges = false;
        foreach (var userGroup in group.Members!)
        {
            foreach (var match in tournament!.Matches!)
            {
                var prediction = await _context.Predictions.FirstOrDefaultAsync(x => x.GroupId == group.Id &&
                                                                                        x.Match.Id == match.Id &&
                                                                                        x.UserId == userGroup.UserId &&
                                                                                        x.TournamentId == tournament.Id);
                if (prediction == null)
                {
                    wasChanges = true;
                    _context.Predictions.Add(new Prediction
                    {
                        Group = group,
                        Match = match,
                        Tournament = tournament,
                        User = userGroup.User,
                    });
                }
            }
        }

        if (wasChanges)
        {
            await _context.SaveChangesAsync();
        }
    }
}