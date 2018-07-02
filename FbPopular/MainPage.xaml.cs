using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FbPopular.Resources;
using Newtonsoft.Json.Linq;
using FbPopular.Models;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System.IO;
using GoogleAds;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO.IsolatedStorage;
using System.Windows.Resources;
using Windows.Storage.Streams;
using Microsoft.Xna.Framework.Media.PhoneExtensions;

namespace FbPopular
{
    public partial class MainPage : PhoneApplicationPage
    {
        ProgressIndicator progressIndicator;
        private WebClient PhotowebClient;
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            var bannerAd = MonetizeApp.SetAdBanner(AdFormats.Banner, "ca-app-pub-5683686090349772/5203127449");
            AdRequest adRequest = new AdRequest();
            adRequest.ForceTesting = true;     // Enable test ads
            AdPanel.Children.Add(bannerAd);
            bannerAd.LoadAd(adRequest);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            while (this.NavigationService.BackStack.Any())
            {
                this.NavigationService.RemoveBackEntry();
            }

            if (AppResources.ResourceManager.GetString(UppercaseFirst(settings["filter"].ToString()), AppResources.Culture) + " > " + settings["likes"].ToString() + " likes" != Filterslabel.Text)
            {
                GetData();
                if (ContentList != null && ContentList.Items.Count > 0)
                {
                    ContentList.ScrollIntoView(ContentList.Items.First());
                }
            }

            if (ContentList == null || ContentList.Items.Count == 0)
            {
                MessageText.Text = AppResources.GettingData;
                BitmapImage image = new BitmapImage { UriSource = new Uri("Resources/Images/hourglass.png", UriKind.Relative) };
                Messageimage.Source = image;
                GetData();
            }

            Filterslabel.Text = AppResources.ResourceManager.GetString(UppercaseFirst(settings["filter"].ToString()), AppResources.Culture) + " > " + settings["likes"].ToString() + " likes";
        }

        private void Filters_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FiltersPage.xaml", UriKind.Relative));
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            GetData();
            if (ContentList.Items.Count > 0)
            {
                ContentList.ScrollIntoView(ContentList.Items.First());
            }
        }

        public async void GetData()
        {
            progressIndicator = new ProgressIndicator();
            progressIndicator.IsVisible = true;
            progressIndicator.Text = AppResources.GettingData;
            progressIndicator.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, progressIndicator);

            try
            {
                WebClient WBclient = new WebClient();
                string URL = "https://viralfacebookimages.p.mashape.com/dl";
                WBclient.Headers["X-Mashape-Key"] = "PLmxV8EPkumshYjX0M8mTRlQgPrKp1gf2Q5jsna4VzB1arGD2l";
                WBclient.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                WBclient.Headers["Accept"] = "application/json";
                string Parameters = "likesplus=" + settings["likes"].ToString() + "&trendcat=" + settings["filter"].ToString() + "&trendimageage=2";
                string result = await WBclient.UploadStringTaskAsync(new Uri(URL), Parameters);
                JArray data = JArray.Parse(result);
                SaveData(data);
            }

            catch 
            {
                Messageimage.Visibility = Visibility.Visible;
                BitmapImage image = new BitmapImage { UriSource = new Uri("Resources/Images/warning50.png", UriKind.Relative) };
                Messageimage.Source = image;
                MessageText.Visibility = Visibility.Visible;
                MessageText.Text = AppResources.SyncFailed;
            }

            progressIndicator.IsVisible = false;
            progressIndicator.IsIndeterminate = false;
        }

        public void SaveData(JArray data)
        {

            List<Photo> items = new List<Photo>();

            foreach (var item in data)
            {
                Photo photo = new Photo
                {
                    LikeCounts = int.Parse(item["likes_count"].ToString()),
                    Url = item["src_big"].ToString(),
                };
                items.Add(photo);
            }

            if (items.Any())
            {
                var percentagelimit = items.Max(a => a.LikeCounts);
                MessageText.Visibility = Visibility.Collapsed;

                try
                {
                    ObservableCollection<Photo> photos = new ObservableCollection<Photo>();
                    foreach (var item in items)
                    {
                        var LikePercentage = (item.LikeCounts / (double)percentagelimit) * 100;
                        if (LikePercentage > 100) { LikePercentage = 100; }
                        Photo photo = new Photo
                        {
                            LikeCounts = item.LikeCounts,
                            Url = item.Url,
                            Percentage = LikePercentage
                        };
                        photos.Add(photo);
                    }

                    Messageimage.Visibility = Visibility.Collapsed;
                    ContentList.ItemsSource = photos.OrderByDescending(a => a.LikeCounts);
                }

                catch
                {
                    Messageimage.Visibility = Visibility.Visible;
                    BitmapImage image = new BitmapImage { UriSource = new Uri("Resources/Images/warning.png", UriKind.Relative) };
                    Messageimage.Source = image;
                    MessageText.Visibility = Visibility.Visible;
                    MessageText.Text = AppResources.SyncFailed;
                }
            }
            else
            {
                BitmapImage image = new BitmapImage { UriSource = new Uri("Resources/Images/nodata.png", UriKind.Relative) };
                Messageimage.Source = image;
                Messageimage.Visibility = Visibility.Visible;
                MessageText.Visibility = Visibility.Visible;
                MessageText.Text = AppResources.NoDataFound;
            }
        }

        private void ShareButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var obj = this.ContentList.SelectedItem as Photo;
            GetSaveShareImage(obj.Url);
        }

        private async void DownloadButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var obj = this.ContentList.SelectedItem as Photo;

            progressIndicator = new ProgressIndicator();
            progressIndicator.IsVisible = true;
            progressIndicator.Text = AppResources.DownloadingPhoto;
            progressIndicator.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, progressIndicator);
            await System.Threading.Tasks.Task.Delay(2000);
            GetSaveImage(obj.Url);
            progressIndicator.Text = AppResources.PhotoDownloadedSuccessfully;
            await System.Threading.Tasks.Task.Delay(1000);
            progressIndicator.IsVisible = false;
            progressIndicator.IsIndeterminate = false;
        }

        private void RateAppButton_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        #region downloadphoto
        private void GetSaveImage(string downloadimageurl)
        {

            PhotowebClient = new WebClient();
            PhotowebClient.OpenReadCompleted += WebClientOpenReadCompleted;
            PhotowebClient.OpenReadAsync(new Uri(downloadimageurl, UriKind.Absolute));

            progressIndicator = new ProgressIndicator();
            progressIndicator.IsVisible = true;
            progressIndicator.Text = AppResources.DownloadingPhoto;
            progressIndicator.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, progressIndicator);
        }
        void WebClientOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            const string tempJpeg = "TempJPEG";
            var streamResourceInfo = new StreamResourceInfo(e.Result, null);

            var userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
            if (userStoreForApplication.FileExists(tempJpeg))
            {
                userStoreForApplication.DeleteFile(tempJpeg);
            }

            var isolatedStorageFileStream = userStoreForApplication.CreateFile(tempJpeg);

            var bitmapImage = new BitmapImage { CreateOptions = BitmapCreateOptions.None };
            bitmapImage.SetSource(streamResourceInfo.Stream);

            var writeableBitmap = new WriteableBitmap(bitmapImage);
            writeableBitmap.SaveJpeg(isolatedStorageFileStream, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, 0, 85);

            isolatedStorageFileStream.Close();
            isolatedStorageFileStream = userStoreForApplication.OpenFile(tempJpeg, FileMode.Open, FileAccess.Read);

            // Save the image to the camera roll or saved pictures album.
            var mediaLibrary = new MediaLibrary();

            // Save the image to the saved pictures album.
            mediaLibrary.SavePicture(string.Format("FbViral-SavedPicture{0}.jpg", DateTime.Now), isolatedStorageFileStream);

            isolatedStorageFileStream.Close();         
        }
        #endregion

        #region sharephoto
        private void GetSaveShareImage(string downloadimageurl)
        {
            PhotowebClient = new WebClient();
            PhotowebClient.OpenReadCompleted += WebClientOpenReadShareCompleted;
            PhotowebClient.OpenReadAsync(new Uri(downloadimageurl, UriKind.Absolute));

            progressIndicator = new ProgressIndicator();
            progressIndicator.IsVisible = true;
            progressIndicator.Text = AppResources.SharingPhoto;
            progressIndicator.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, progressIndicator);
        }
        void WebClientOpenReadShareCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            const string tempJpeg = "TempJPEG";
            var streamResourceInfo = new StreamResourceInfo(e.Result, null);

            var userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
            if (userStoreForApplication.FileExists(tempJpeg))
            {
                userStoreForApplication.DeleteFile(tempJpeg);
            }

            var isolatedStorageFileStream = userStoreForApplication.CreateFile(tempJpeg);

            var bitmapImage = new BitmapImage { CreateOptions = BitmapCreateOptions.None };
            bitmapImage.SetSource(streamResourceInfo.Stream);

            var writeableBitmap = new WriteableBitmap(bitmapImage);
            writeableBitmap.SaveJpeg(isolatedStorageFileStream, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, 0, 85);

            isolatedStorageFileStream.Close();
            isolatedStorageFileStream = userStoreForApplication.OpenFile(tempJpeg, FileMode.Open, FileAccess.Read);

            // Save the image to the camera roll or saved pictures album.
            var mediaLibrary = new MediaLibrary();

            // Save the image to the saved pictures album.
            var picture = mediaLibrary.SavePicture(string.Format("FbViral-SavedPicture{0}.jpg", DateTime.Now), isolatedStorageFileStream);

            isolatedStorageFileStream.Close();

            var shareMediaTask = new ShareMediaTask();
            shareMediaTask = new ShareMediaTask();
            shareMediaTask.FilePath = picture.GetPath(); // requires using Microsoft.Xna.Framework.Media.PhoneExtensions;
            shareMediaTask.Show();

            progressIndicator.Text = AppResources.PhotoDownloadedSuccessfully;
            progressIndicator.IsVisible = false;
            progressIndicator.IsIndeterminate = false;
        }
        #endregion

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
            ApplicationBarMenuItem appBarMenuItem1 = new ApplicationBarMenuItem(AppResources.Filters);
            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem(AppResources.Refresh);
            //ApplicationBarMenuItem appBarMenuItem3 = new ApplicationBarMenuItem(AppResources.About);
            ApplicationBarMenuItem appBarMenuItem4 = new ApplicationBarMenuItem(AppResources.RateApp);

            appBarMenuItem1.Click += new EventHandler(Filters_Click);
            appBarMenuItem2.Click += new EventHandler(Refresh_Click);
            //appBarMenuItem3.Click += new EventHandler(button1_Click);
            appBarMenuItem4.Click += new EventHandler(RateAppButton_Click);

            ApplicationBar.MenuItems.Add(appBarMenuItem1);
            ApplicationBar.MenuItems.Add(appBarMenuItem2);
            //ApplicationBar.MenuItems.Add(appBarMenuItem3);
            ApplicationBar.MenuItems.Add(appBarMenuItem4);
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}