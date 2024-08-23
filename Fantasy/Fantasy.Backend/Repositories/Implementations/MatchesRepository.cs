using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fantasy.Backend.Repositories.Implementations
{
    public class MatchesRepository : GenericRepository<Match>, IMatchesRepository
    {
        private readonly DataContext _context;

        public MatchesRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ActionResponse<Match>> AddAsync(MatchDTO matchDTO)
        {
            var tournament = await _context.Tournaments.FindAsync(matchDTO.TournamentId);
            if (tournament == null)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR009"
                };
            }

            var local = await _context.Teams.FindAsync(matchDTO.LocalId);
            if (local == null)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR010"
                };
            }

            var visitor = await _context.Teams.FindAsync(matchDTO.VisitorId);
            if (visitor == null)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR011"
                };
            }

            var match = new Match
            {
                IsActive = matchDTO.IsActive,
                Date = matchDTO.Date,
                Tournament = tournament,
                Local = local,
                Visitor = visitor,
            };

            _context.Add(match);
            try
            {
                await _context.SaveChangesAsync();
                return new ActionResponse<Match>
                {
                    WasSuccess = true,
                    Result = match
                };
            }
            catch (DbUpdateException)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR003"
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = exception.Message
                };
            }
        }

        public async Task<ActionResponse<int>> GetTotalRecordsAsync(PaginationDTO pagination)
        {
            var queryable = _context.Matches.AsQueryable();
            queryable = queryable.Where(x => x.TournamentId == pagination.Id);

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Local.Name.ToLower().Contains(pagination.Filter.ToLower()) ||
                                                 x.Visitor.Name.ToLower().Contains(pagination.Filter.ToLower()));
            }

            double count = await queryable.CountAsync();
            return new ActionResponse<int>
            {
                WasSuccess = true,
                Result = (int)count
            };
        }

        public override async Task<ActionResponse<IEnumerable<Match>>> GetAsync(PaginationDTO pagination)
        {
            var queryable = _context.Matches
                .Include(x => x.Tournament)
                .Include(x => x.Local)
                .Include(x => x.Visitor)
                .AsQueryable();
            queryable = queryable.Where(x => x.TournamentId == pagination.Id);

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Local.Name.ToLower().Contains(pagination.Filter.ToLower()) ||
                                                 x.Visitor.Name.ToLower().Contains(pagination.Filter.ToLower()));
            }

            return new ActionResponse<IEnumerable<Match>>
            {
                WasSuccess = true,
                Result = await queryable
                    .OrderBy(x => x.Date)
                    .Paginate(pagination)
                    .ToListAsync()
            };
        }

        public override async Task<ActionResponse<Match>> GetAsync(int id)
        {
            var team = await _context.Matches
                 .Include(x => x.Tournament)
                 .Include(x => x.Local)
                 .Include(x => x.Visitor)
                 .FirstOrDefaultAsync(c => c.Id == id);

            if (team == null)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR001"
                };
            }

            return new ActionResponse<Match>
            {
                WasSuccess = true,
                Result = team
            };
        }

        public async Task<ActionResponse<Match>> UpdateAsync(MatchDTO matchDTO)
        {
            var currentMatch = await _context.Matches.FindAsync(matchDTO.Id);
            if (currentMatch == null)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR012"
                };
            }

            var tournament = await _context.Tournaments.FindAsync(matchDTO.TournamentId);
            if (tournament == null)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR009"
                };
            }

            var local = await _context.Teams.FindAsync(matchDTO.LocalId);
            if (local == null)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR010"
                };
            }

            var visitor = await _context.Teams.FindAsync(matchDTO.VisitorId);
            if (visitor == null)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR011"
                };
            }

            currentMatch.Local = local;
            currentMatch.Visitor = visitor;
            currentMatch.GoalsVisitor = matchDTO.GoalsVisitor;
            currentMatch.GoalsLocal = matchDTO.GoalsLocal;
            currentMatch.Date = matchDTO.Date;
            currentMatch.IsActive = matchDTO.IsActive;

            _context.Update(currentMatch);
            try
            {
                await _context.SaveChangesAsync();
                return new ActionResponse<Match>
                {
                    WasSuccess = true,
                    Result = currentMatch
                };
            }
            catch (DbUpdateException)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = "ERR003"
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<Match>
                {
                    WasSuccess = false,
                    Message = exception.Message
                };
            }
        }
    }
}