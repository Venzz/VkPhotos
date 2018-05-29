using System;
using System.Threading.Tasks;
using VkPhotos.Data;

namespace VkPhotos.Model
{
    public class Users
    {
        private IUserProvider Provider;
        private User CurrentUser;

        public Users(IUserProvider provider)
        {
            Provider = provider;
        }

        public User GetCurrentUser()
        {
            if (CurrentUser != null)
                return CurrentUser;
            CurrentUser = App.Settings.UserId.HasValue ? new User(App.Settings.UserId.Value, App.Settings.FirstName, App.Settings.LastName, App.Settings.AccessToken) : null;
            return CurrentUser;
        }

        public async Task<User> SignInAsync(UInt32 userId, String accessToken)
        {
            var user = await Provider.GetAsync(userId).ConfigureAwait(false);
            App.Settings.UserId = user.Id;
            App.Settings.FirstName = user.FirstName;
            App.Settings.LastName = user.LastName;
            App.Settings.AccessToken = accessToken;
            CurrentUser = new User(user, accessToken);
            return CurrentUser;
        }

        public void SignOut()
        {
            App.Settings.UserId = null;
            App.Settings.FirstName = null;
            App.Settings.LastName = null;
            App.Settings.AccessToken = null;
            CurrentUser = null;
        }
    }
}
