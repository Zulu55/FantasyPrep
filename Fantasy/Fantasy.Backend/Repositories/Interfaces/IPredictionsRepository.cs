﻿using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;

namespace Fantasy.Backend.Repositories.Interfaces;

public interface IPredictionsRepository
{
    Task<ActionResponse<IEnumerable<PositionDTO>>> GetPositionsAsync(PaginationDTO pagination);

    Task<ActionResponse<int>> GetTotalRecordsForPositionsAsync(PaginationDTO pagination);

    Task<ActionResponse<Prediction>> AddAsync(PredictionDTO predictionDTO);

    Task<ActionResponse<Prediction>> UpdateAsync(PredictionDTO predictionDTO);

    Task<ActionResponse<Prediction>> GetAsync(int id);

    Task<ActionResponse<IEnumerable<Prediction>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<int>> GetTotalRecordsAsync(PaginationDTO pagination);

    Task<ActionResponse<IEnumerable<Prediction>>> GetAllPredictionsAsync(PaginationDTO pagination);

    Task<ActionResponse<int>> GetTotalRecordsAllPredictionsAsync(PaginationDTO pagination);

    Task<ActionResponse<IEnumerable<Prediction>>> GetBalanceAsync(PaginationDTO pagination);

    Task<ActionResponse<int>> GetTotalRecordsBalanceAsync(PaginationDTO pagination);
}