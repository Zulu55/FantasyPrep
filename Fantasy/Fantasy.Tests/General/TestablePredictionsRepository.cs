using Fantasy.Backend.Data;
using Fantasy.Backend.Repositories.Implementations;
using Fantasy.Backend.Repositories.Interfaces;
using Fantasy.Shared.Entities;

namespace Fantasy.Tests.General
{
    public class TestablePredictionsRepository : PredictionsRepository
    {
        private readonly bool _canWatchResult;

        public TestablePredictionsRepository(DataContext context, IUsersRepository usersRepository, bool canWatchResult)
            : base(context, usersRepository)
        {
            _canWatchResult = canWatchResult;
        }

        public override bool CanWatch(Prediction prediction)
        {
            return _canWatchResult;
        }
    }
}