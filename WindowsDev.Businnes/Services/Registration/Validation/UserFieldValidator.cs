using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase.Interfaces;

namespace WindowsDev.Business.Services.Registration.Validation
{
    public class UserFieldValidator
    {
        private readonly IDbManager _dbManager;

        public UserFieldValidator(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public Task<bool> IsLoginAvailableAsync(string login)
        {
            var debounced = Debounce(async () =>
            {
                using var dbContext = _dbManager.Create();

                var exists = await dbContext.UsersInfo
                .AnyAsync(l => l.Login  == login);

                return !exists;
            });

            return debounced();
        }

        public Task<bool> IsUsernameAvailableAsync(string username)
        {
           var debounced = Debounce(async () =>
            {
                using var dbContext = _dbManager.Create();

                var exists = await dbContext.UsersInfo
                    .AnyAsync(u => u.Username == username);

                return !exists;
            });

            return debounced();
        }

        private Func<Task<bool>> Debounce(Func<Task<bool>> action)
        {
            CancellationTokenSource Cts = new CancellationTokenSource();

            return async () =>
            {
                Cts.Cancel();
                Cts = new CancellationTokenSource();

               await Task.Delay(500, Cts.Token);

               return await action();
            };
        }
    }
}


