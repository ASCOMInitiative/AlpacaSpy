using ASCOM.Alpaca;

namespace AlpacaSpy.Data
{
    internal class UserService : IUserService
    {
        public Task<bool> Authenticate(string username, string password) =>
            Task.FromResult(false);

        public bool UseAuth => false;
    }
}
