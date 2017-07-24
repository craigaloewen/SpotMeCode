using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpotMe
{
    /// <summary>
    /// Interaction logic for ExerciseView.xaml
    /// </summary>
    public partial class ExerciseView : Page
    {

        SpotMeController mainController;

        Exercise currentExercise;

        public ExerciseView(Exercise inputExercise)
        {
            currentExercise = inputExercise;

            mainController = new SpotMeController();

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

            mainController.Init(inputExercise.name);

            Thread uiUpdateThread = new Thread(new ThreadStart(UpdateUIWorker)) { IsBackground = true };
            uiUpdateThread.Start();

            mainController.SwitchMode(SpotMeController.ControllerMode.Set);
        }

        private void BacktoExerciseList(object sender, RoutedEventArgs e)
        {
            ExerciseListView viewPage = new SpotMe.ExerciseListView();
            NavigationService.Navigate(viewPage);
        }

        private void UpdateUIWorker()
        {
            while (true)
            {
                UpdateUIDelegate uiUpdater = new UpdateUIDelegate(UpdateUI);
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, uiUpdater);
                if (mainController.kinectSensor == null)
                {
                    break;
                }
                Thread.Sleep(50);
            }
        }

        private delegate void UpdateUIDelegate();

        public void UpdateUI()
        {
            trainingDataLabel.Content = this.ScreenMessage;
            MovementBar.Value = mainController.machineLearningAlg.movementIndexValue;
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource BodyImageSource
        {
            get
            {
                return this.mainController.frameImageSource;
            }
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ColorImageSource
        {
            get
            {
                return this.mainController.colorBitmap;
            }
        }

        public string ScreenMessage
        {
            get
            {
                return mainController.outputMessage;
            }
        }


        private void ExerciseView_Loaded(object sender, RoutedEventArgs e)
        {
            mainController.Init(currentExercise.name);
        }

        private void ExerciseView_Unloaded(object sender, RoutedEventArgs e)
        {
            mainController.CleanUp();
        }

        private void SetModeFunction(object sender, RoutedEventArgs e)
        {
            mainController.SwitchMode(SpotMeController.ControllerMode.Set);
        }
        private void ContinuousModeFunction(object sender, RoutedEventArgs e)
        {
            mainController.SwitchMode(SpotMeController.ControllerMode.Continuous);
        }
    }
}
