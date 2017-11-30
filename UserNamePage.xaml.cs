using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace Conesoft
{
    public partial class UserNamePage : PhoneApplicationPage
    {
        public UserNamePage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text) == false)
            {
                IsolatedStorageSettings.ApplicationSettings["Username"] = UserNameTextBox.Text;

                if (NavigationContext.QueryString.ContainsKey("NextPage"))
                {
                    var nextPage = NavigationContext.QueryString["NextPage"];
                    NavigationService.Navigate(new Uri("/" + nextPage, UriKind.Relative));
                }
                else
                {
                    NavigationService.GoBack();
                }
            }
        }
    }
}