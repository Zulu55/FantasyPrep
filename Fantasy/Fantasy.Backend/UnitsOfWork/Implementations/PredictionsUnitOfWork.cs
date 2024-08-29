using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Backend.UnitsOfWork.Interfaces;
using Fantasy.Shared.DTOs;
using Fantasy.Shared.Entities;
using Fantasy.Shared.Responses;

namespace Fantasy.Backend.UnitsOfWork.Implementations;

public class PredictionsUnitOfWork : GenericUnitOfWork<Prediction>, IPredictionsUnitOfWork
{
    private readonly IPredictionsRepository _predictionsRepository;

    public PredictionsUnitOfWork(IGenericRepository<Prediction> repository, IPredictionsRepository predictionsRepository) : base(repository)
    {
        _predictionsRepository = predictionsRepository;
    }

    public override async Task<ActionResponse<IEnumerable<Prediction>>> GetAsync(PaginationDTO pagination) => await _predictionsRepository.GetAsync(pagination);

    public override async Task<ActionResponse<Prediction>> GetAsync(int id) => await _predictionsRepository.GetAsync(id);

    public async Task<ActionResponse<Prediction>> AddAsync(PredictionDTO AddAsync) => await _predictionsRepository.AddAsync(AddAsync);

    public async Task<ActionResponse<int>> GetTotalRecordsAsync(PaginationDTO paginationDTO) => await _predictionsRepository.GetTotalRecordsAsync(paginationDTO);

    public async Task<ActionResponse<Prediction>> UpdateAsync(PredictionDTO predictionDTO) => await _predictionsRepository.UpdateAsync(predictionDTO);

    public async Task<ActionResponse<IEnumerable<PositionDTO>>> GetPositionsAsync(PaginationDTO pagination) => await _predictionsRepository.GetPositionsAsync(pagination);

    public async Task<ActionResponse<int>> GetTotalRecordsForPositionsAsync(PaginationDTO pagination) => await _predictionsRepository.GetTotalRecordsForPositionsAsync(pagination);

    public async Task<ActionResponse<IEnumerable<Prediction>>> GetAllPredictionsAsync(PaginationDTO pagination) => await _predictionsRepository.GetAllPredictionsAsync(pagination);

    public async Task<ActionResponse<int>> GetTotalRecordsAllPredictionsAsync(PaginationDTO pagination) => await _predictionsRepository.GetTotalRecordsAllPredictionsAsync(pagination);

    public async Task<ActionResponse<IEnumerable<Prediction>>> GetBalanceAsync(PaginationDTO pagination) => await _predictionsRepository.GetBalanceAsync(pagination);

    public async Task<ActionResponse<int>> GetTotalRecordsBalanceAsync(PaginationDTO pagination) => await _predictionsRepository.GetTotalRecordsBalanceAsync(pagination);
}