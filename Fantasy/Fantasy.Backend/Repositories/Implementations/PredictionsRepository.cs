﻿using Fantasy.Backend.Data;
using Fantasy.Backend.Helpers;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fantasy.Backend.Repositories.Implementations;

public class PredictionsRepository : GenericRepository<Prediction>, IPredictionsRepository
{
    private readonly DataContext _context;
    private readonly IUsersRepository _usersRepository;

    public PredictionsRepository(DataContext context, IUsersRepository usersRepository) : base(context)
    {
        _context = context;
        _usersRepository = usersRepository;
    }

    public override async Task<ActionResponse<IEnumerable<Prediction>>> GetAsync(PaginationDTO pagination)
    {
        var queryable = _context.Predictions
            .Include(x => x.Match)
            .ThenInclude(x => x.Local)
            .Include(x => x.Match)
            .ThenInclude(x => x.Visitor)
            .AsQueryable();
        queryable = queryable.Where(x => x.GroupId == pagination.Id);
        queryable = queryable.Where(x => x.User.Email == pagination.Email);

        if (!string.IsNullOrWhiteSpace(pagination.Filter))
        {
            queryable = queryable.Where(x => x.Match.Local.Name.ToLower().Contains(pagination.Filter.ToLower()) ||
                                             x.Match.Visitor.Name.ToLower().Contains(pagination.Filter.ToLower()));
        }

        return new ActionResponse<IEnumerable<Prediction>>
        {
            WasSuccess = true,
            Result = await queryable
                .OrderBy(x => x.Match.Date)
                .Paginate(pagination)
                .ToListAsync()
        };
    }

    public override async Task<ActionResponse<Prediction>> GetAsync(int id)
    {
        var prediction = await _context.Predictions
            .Include(x => x.Match)
            .ThenInclude(x => x.Local)
            .Include(x => x.Match)
            .ThenInclude(x => x.Visitor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (prediction == null)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = "ERR001"
            };
        }

        return new ActionResponse<Prediction>
        {
            WasSuccess = true,
            Result = prediction
        };
    }

    public async Task<ActionResponse<Prediction>> AddAsync(PredictionDTO predictionDTO)
    {
        var user = await _usersRepository.GetUserAsync(Guid.Parse(predictionDTO.UserId));
        if (user == null)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = "ERR013"
            };
        }

        var group = await _context.Groups.FindAsync(predictionDTO.GroupId);
        if (group == null)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = "ERR014"
            };
        }

        var tournament = await _context.Tournaments.FindAsync(predictionDTO.TournamentId);
        if (tournament == null)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = "ERR009"
            };
        }

        var match = await _context.Matches.FindAsync(predictionDTO.MatchId);
        if (match == null)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = "ERR012"
            };
        }

        var prediction = new Prediction
        {
            GoalsLocal = predictionDTO.GoalsLocal,
            GoalsVisitor = predictionDTO.GoalsVisitor,
            Group = group,
            Tournament = tournament,
            Match = match,
            User = user,
        };

        _context.Add(prediction);
        try
        {
            await _context.SaveChangesAsync();
            return new ActionResponse<Prediction>
            {
                WasSuccess = true,
                Result = prediction
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = "ERR003"
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<int>> GetTotalRecordsAsync(PaginationDTO pagination)
    {
        var queryable = _context.Predictions.AsQueryable();
        queryable = queryable.Where(x => x.GroupId == pagination.Id);
        queryable = queryable.Where(x => x.User.Email == pagination.Email);

        if (!string.IsNullOrWhiteSpace(pagination.Filter))
        {
            queryable = queryable.Where(x => x.Match.Local.Name.ToLower().Contains(pagination.Filter.ToLower()) ||
                                             x.Match.Visitor.Name.ToLower().Contains(pagination.Filter.ToLower()));
        }

        double count = await queryable.CountAsync();
        return new ActionResponse<int>
        {
            WasSuccess = true,
            Result = (int)count
        };
    }

    public async Task<ActionResponse<Prediction>> UpdateAsync(PredictionDTO predictionDTO)
    {
        var currentPrediction = await _context.Predictions.FindAsync(predictionDTO.Id);
        if (currentPrediction == null)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = "ERR016"
            };
        }

        currentPrediction.GoalsLocal = predictionDTO.GoalsLocal;
        currentPrediction.GoalsVisitor = predictionDTO.GoalsVisitor;
        currentPrediction.Points = predictionDTO.Points;

        _context.Update(currentPrediction);
        try
        {
            await _context.SaveChangesAsync();
            return new ActionResponse<Prediction>
            {
                WasSuccess = true,
                Result = currentPrediction
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = "ERR003"
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Prediction>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }
}