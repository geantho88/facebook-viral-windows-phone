using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using FbPopular.Resources;

namespace FbPopular
{
    public partial class FiltersPage : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public FiltersPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LikesTextBox.Text = settings["likes"].ToString();

            switch (settings["filter"].ToString())
            {
                case "fun":
                    FunFilterButton.Background = ColorConverter.GetColorFromHexa("#FFE74C3C");
                    FunFilterButton.Foreground = new SolidColorBrush(Colors.White);
                    break;
                case "sports":
                    SportsFiltesButton.Background = ColorConverter.GetColorFromHexa("#FFE74C3C");
                    SportsFiltesButton.Foreground = new SolidColorBrush(Colors.White);
                    break;
                case "movies":
                    MoviesFilterButton.Background = ColorConverter.GetColorFromHexa("#FFE74C3C");
                    MoviesFilterButton.Foreground = new SolidColorBrush(Colors.White);
                    break;
                case "animals":
                    AnimalsFilterButton.Background = ColorConverter.GetColorFromHexa("#FFE74C3C");
                    AnimalsFilterButton.Foreground = new SolidColorBrush(Colors.White);
                    break;
            }
        }

        private void FunFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SportsFiltesButton.Background = null;
            MoviesFilterButton.Background = null;
            AnimalsFilterButton.Background = null;
            SportsFiltesButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            MoviesFilterButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            AnimalsFilterButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            settings["filter"] = "fun"; 
            FunFilterButton.Background = ColorConverter.GetColorFromHexa("#FFE74C3C");
            FunFilterButton.Foreground = new SolidColorBrush(Colors.White);
            FunFilterButton.UpdateLayout();
        }

        private void SportsFiltesButton_Click(object sender, RoutedEventArgs e)
        {
            MoviesFilterButton.Background = null;
            AnimalsFilterButton.Background = null;
            FunFilterButton.Background = null;
            MoviesFilterButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            AnimalsFilterButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            FunFilterButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            settings["filter"] = "sports";
            SportsFiltesButton.Background = ColorConverter.GetColorFromHexa("#FFE74C3C");
            SportsFiltesButton.Foreground = new SolidColorBrush(Colors.White);
            SportsFiltesButton.UpdateLayout();
        }

        private void MoviesFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SportsFiltesButton.Background = null;
            AnimalsFilterButton.Background = null;
            FunFilterButton.Background = null;
            SportsFiltesButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            AnimalsFilterButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            FunFilterButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            settings["filter"] = "movies";
            MoviesFilterButton.Background = ColorConverter.GetColorFromHexa("#FFE74C3C");
            MoviesFilterButton.Foreground = new SolidColorBrush(Colors.White);
            MoviesFilterButton.UpdateLayout();
        }

        private void AnimalsFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SportsFiltesButton.Background = null;
            MoviesFilterButton.Background = null;
            FunFilterButton.Background = null;
            SportsFiltesButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            MoviesFilterButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            FunFilterButton.Foreground = ColorConverter.GetColorFromHexa("#FF9E9E9E");
            settings["filter"] = "animals";
            AnimalsFilterButton.Background = ColorConverter.GetColorFromHexa("#FFE74C3C");
            AnimalsFilterButton.Foreground = new SolidColorBrush(Colors.White);
            AnimalsFilterButton.UpdateLayout();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            settings["likes"] = LikesTextBox.Text;
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBarIconButton savebutton = new ApplicationBarIconButton();
            savebutton.IconUri = new Uri("/Assets/AppBar1/check.png", UriKind.Relative);
            savebutton.Text = AppResources.Save;
            savebutton.Click += new EventHandler(SaveButton_Click);
            ApplicationBar.Buttons.Add(savebutton);     
        }
    }
}