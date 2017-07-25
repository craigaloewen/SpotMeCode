using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpotMe
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
            LoadFolder("..\\..\\assets");
        }

        public void NavigateToCreateWorkoutPage(object sender, RoutedEventArgs e)
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("page2.xaml", UriKind.RelativeOrAbsolute));
        }

        public void NavigateToExerciseManagerPage(object sender, RoutedEventArgs e)
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("ExerciseManagerView.xaml", UriKind.RelativeOrAbsolute));
        }

        private WrapPanel[] ImageControls;
        private List<ImageSource> imagesList = new List<ImageSource>();
        private static string[] ValidExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }; //Provide extensions to load images in slider
        private static string[] TransitionEffects = new[] { "SlideUp", "SlideDown" };
        private string TransitionType, strImagePath = AppDomain.CurrentDomain.BaseDirectory + "\\SliderImages"; //Directory path of images
        private int CurrentCtrlIndex = 0;

        //Load images from folder
        private void LoadFolder(string folder)
        {
            try
            {
                c_errorText.Visibility = Visibility.Collapsed;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                if (!System.IO.Path.IsPathRooted(folder))
                    folder = System.IO.Path.Combine(Environment.CurrentDirectory, folder);
                if (!System.IO.Directory.Exists(folder))
                {
                    c_errorText.Text = "The specified folder does not exist: " + Environment.NewLine + folder;
                    c_errorText.Visibility = Visibility.Visible;
                    return;
                }
                Random r = new Random();
                var sources = from file in new System.IO.DirectoryInfo(folder).GetFiles().AsParallel()
                              where ValidExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase)
                              orderby file.Name
                              select CreateImageSource(file.FullName, true);
                imagesList.Clear();
                imagesList.AddRange(sources);
                sw.Stop();
                // Console.WriteLine("Total time to load {0} images: {1}ms", imagesList.Count, sw.ElapsedMilliseconds);
                int imageCnt = imagesList.Count;
                double originalCount = Convert.ToDouble(imageCnt / 3.0);
                int totalCount = Convert.ToInt32(imageCnt / 3);
                int finalCount;
                if (originalCount == totalCount)
                    finalCount = totalCount;
                else
                    finalCount = totalCount + 1;

                ImageControls = new WrapPanel[finalCount];
                for (int j = 0; j < finalCount; j++)
                {
                    WrapPanel myWrapPanel = new WrapPanel();
                    myWrapPanel.Name = "myImage" + j;
                    myWrapPanel.RenderTransformOrigin = new Point(0.5, 0.5);
                    myWrapPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    myWrapPanel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    myWrapPanel.Orientation = Orientation.Vertical;
                    myWrapPanel.RenderTransform = new ScaleTransform() { ScaleX = 1, ScaleY = 1, CenterX = 0, CenterY = 0 };
                    myWrapPanel.RenderTransform = new SkewTransform() { AngleX = 0, AngleY = 0, CenterX = 0, CenterY = 0 };
                    myWrapPanel.RenderTransform = new RotateTransform() { Angle = 0, CenterX = 0, CenterY = 0 };
                    myWrapPanel.RenderTransform = new TranslateTransform() { X = 0, Y = 0 };
                    myWrapPanel.Visibility = System.Windows.Visibility.Hidden;
                    mainCanvas.Children.Add(myWrapPanel);
                    ImageControls[j] = myWrapPanel;
                }
                ImageControls[0].Visibility = System.Windows.Visibility.Visible;
                int s = 0;
                int intCnt = 0;
                for (int i = 0; i < imagesList.Count; i++)
                {
                    Border brd = new Border();
                    brd.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    brd.BorderThickness = new Thickness(1);
                    brd.Margin = new Thickness(0, 10, 0, 10);
                    brd.Background = Brushes.Transparent;
                    brd.Padding = new Thickness(5);

                    Image img = new Image();
                    img.Name = "img" + i;
                    img.Source = new ImageSourceConverter().ConvertFromString(imagesList[i].ToString()) as ImageSource;
                    img.MouseUp += img_MouseUp;
                    brd.Child = img;

                    if (intCnt < 3)
                    {
                        ImageControls[s].Children.Add(brd);
                        intCnt++;
                    }
                    else
                    {
                        s++;
                        intCnt = 1;
                        ImageControls[s].Children.Add(brd);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //Slide images in Up or Down direction
        private void Play_Image_Slider(string strDirection)
        {
            try
            {
                if (ImageControls.Length == 1)
                    return;
                var oldCtrlIndex = CurrentCtrlIndex;

                if (strDirection == "UpSide")
                {
                    TransitionType = TransitionEffects[1].ToString();
                    if (CurrentCtrlIndex == 0)
                        CurrentCtrlIndex = (ImageControls.Length - 1);
                    else
                        CurrentCtrlIndex--;
                }
                else
                {
                    TransitionType = TransitionEffects[0].ToString();
                    if (CurrentCtrlIndex == (ImageControls.Length - 1))
                        CurrentCtrlIndex = 0;
                    else
                        CurrentCtrlIndex++;
                }

                WrapPanel oldImage = ImageControls[oldCtrlIndex];
                WrapPanel newImage = ImageControls[CurrentCtrlIndex];

                //For Animation of opacity......
                Storyboard DefaultPosition = (Resources["SlideAndFadeIn"] as Storyboard).Clone();
                DefaultPosition.Begin(newImage);

                //For Sliding....
                Storyboard hidePage = (Resources[string.Format("{0}Out", TransitionType.ToString())] as Storyboard).Clone();
                hidePage.Begin(oldImage);
                Storyboard showNewPage = Resources[string.Format("{0}In", TransitionType.ToString())] as Storyboard;
                showNewPage.Begin(newImage);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        //Create image source to load in slider
        private ImageSource CreateImageSource(string file, bool forcePreLoad)
        {
            if (forcePreLoad)
            {
                var src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(file, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                src.Freeze();
                return src;
            }
            else
            {
                var src = new BitmapImage(new Uri(file, UriKind.Absolute));
                src.Freeze();
                return src;
            }
        }

        //Images slide up
        private void imgUp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int tempIndex;
                if ((CurrentCtrlIndex - 1) == -1)
                    tempIndex = (ImageControls.Length - 1);
                else
                    tempIndex = (CurrentCtrlIndex - 1);
                if (ImageControls[tempIndex].Visibility == System.Windows.Visibility.Hidden)
                    ImageControls[tempIndex].Visibility = System.Windows.Visibility.Visible;

                Play_Image_Slider("UpSide");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //Images slide down
        private void imgDown_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int tempIndex;
                if (CurrentCtrlIndex == ImageControls.Length - 1)
                    tempIndex = 0;
                else
                    tempIndex = (CurrentCtrlIndex + 1);
                if (ImageControls[tempIndex].Visibility == System.Windows.Visibility.Hidden)
                    ImageControls[tempIndex].Visibility = System.Windows.Visibility.Visible;

                Play_Image_Slider("DownSide");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //Open popup
        private void img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string strImageSource = (sender as Image).Source.ToString();
                imgView.Source = new ImageSourceConverter().ConvertFromString(strImageSource) as ImageSource;
                pnlImageView.IsOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //Close popup
        private void btnClosePopup_Click(object sender, RoutedEventArgs e)
        {
            pnlImageView.IsOpen = false;
        }
    }
}
