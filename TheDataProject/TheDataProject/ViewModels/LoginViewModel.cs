using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDataProject.Models;

namespace TheDataProject.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public User User { get; set; }
        public Command LoginCommand { get; set; }
        public Command ChangePasswordCommand { get; set; }

        public LoginViewModel()
        {
            Title = "Building";
            User = new User();
            LoginCommand = new Command<User>(async (User user) => await ExecuteLoginCommand(user));
            ChangePasswordCommand = new Command<User>(async (User user) => await ExecuteChangePasswordCommand(user));
        }

        async Task ExecuteLoginCommand(User user)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
               await DataStore.LoginUser(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteChangePasswordCommand(User user)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                await DataStore.ChangePassword(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
