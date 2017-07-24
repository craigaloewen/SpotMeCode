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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;

namespace SpotMe
{
    /// <summary>
    /// Interaction logic for RecordTrainingDataWindow.xaml
    /// </summary>
    public partial class RecordTrainingDataWindow : Window
    {

        SpotMeController mainController;

        int delayMS;
        int numCaptures;
        int initialDelay;
        int classifierID;

        Exercise currentExercise;

        List<double[]> trainingDataList;

        public RecordTrainingDataWindow(Exercise inputExercise, int inputClassifierID, int inputInitialDelay, int inputDelayMS, int inputNumCaptures)
        {

            mainController = new SpotMeController();

            delayMS = inputDelayMS;

            numCaptures = inputNumCaptures;

            initialDelay = inputInitialDelay;

            classifierID = inputClassifierID;

            currentExercise = inputExercise;

            this.DataContext = this;

            InitializeComponent();

            trainingDataList = new List<double[]>();

            mainController.SwitchMode(SpotMeController.ControllerMode.Record);

            mainController.Init(inputExercise.name);
        }

        public ImageSource DisplayImageSource
        {
            get
            {
                return this.mainController.frameImageSource;
            }
        }

        private async void RecordTrainingDataWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RecordTrainingData();

            foreach (double[] trainingData in trainingDataList)
            {
                currentExercise.AddTrainingData(classifierID, trainingData);
            }

            ExerciseManager.SaveExercise(currentExercise);

            this.Close();
        }

        private void RecordTrainingDataWindow_Closing(object sender, EventArgs e)
        {
            mainController.CleanUp();
        }

        private void UpdateUI()
        {

        }

        private async Task RecordTrainingData()
        {
            int currentNumberToCapture = numCaptures;
            int initialDelayCounter = initialDelay;

            while (initialDelayCounter > 0)
            {
                TimeToNextPictureLabel.Text = initialDelayCounter.ToString();
                await Task.Delay(1);
                initialDelayCounter--;
            }

            for (int i = 0; i < numCaptures; i++)
            {
                PictureCountLabel.Text = currentNumberToCapture.ToString();
                await CountDownDelay();

                trainingDataList.Add(SkeletonModifier.PreprocessSkeleton(mainController.lastRecordedSkeleton));

                currentNumberToCapture--;
            }
        }

        private async Task CountDownDelay()
        {
            int currentDelayValue = delayMS;

            while (currentDelayValue > 0)
            {
                TimeToNextPictureLabel.Text = currentDelayValue.ToString();
                await Task.Delay(1);
                currentDelayValue--;
            }
        }





    }

    
}
