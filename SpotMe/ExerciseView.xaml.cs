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

        Workout currentWorkout;

        int currentRep;

        int currentSet;

        bool inReviewState;

        private DrawingImage reviewFrameSource;
        private DrawingGroup reviewFrameDrawingGroup;

        private SkeletonDrawing drawingObject;

        public ExerciseView(Workout inputWorkout)
        {

            currentWorkout = inputWorkout;

            currentRep = 0;
            currentSet = 0;

            inReviewState = false;

            currentExercise = ExerciseManager.LoadExercise(inputWorkout.setList.First().exerciseName);

            mainController = new SpotMeController();

            mainController.RepComplete += new RepCompleteEventHandler(RepComplete);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // Create the drawing group we'll use for drawing
            reviewFrameDrawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            reviewFrameSource = new DrawingImage(this.reviewFrameDrawingGroup);

            drawingObject = new SkeletonDrawing(null);

            // initialize the components (controls) of the window
            this.InitializeComponent();

            mainController.Init(currentExercise.name);

            Thread uiUpdateThread = new Thread(new ThreadStart(UpdateUIWorker)) { IsBackground = true };
            uiUpdateThread.Start();

            mainController.SwitchMode(SpotMeController.ControllerMode.Set);

            WorkoutProgressBar.Maximum = currentWorkout.setList[currentSet].numberOfReps;

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
            trainingDataLabel.Text = this.ScreenMessage;
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

        public ImageSource ReviewSkeletonImageSource
        {
            get
            {
                return reviewFrameSource;
            }
        }

        public string ScreenMessage
        {
            get
            {
                return mainController.outputMessage;
            }
        }

        private void RepComplete(object sender, EventArgs e)
        {
            if (!inReviewState)
            {
                currentRep++;
                if (currentRep == currentWorkout.setList[currentSet].numberOfReps)
                {
                    SetComplete();
                }
                WorkoutProgressBar.Value = currentRep;
            }
        }

        private async void SetComplete()
        {
            currentRep = 0;
            WorkoutProgressBar.Value = 0;
            if (currentSet == (currentWorkout.numberOfSets - 1))
            {
                WorkoutComplete();
            } else
            {
                EnableSetReviewOverlay();
                await CountDownDelay(currentWorkout.setRestTime);
                DisableSetReviewOverlay();
                mainController.ClearSkeletonList();
                currentSet++;
                WorkoutProgressBar.Maximum = currentWorkout.setList[currentSet].numberOfReps;
            }
        }

        private void EnableSetReviewOverlay()
        {
            CountdownToNextSet.Visibility = Visibility.Visible;
            SetReviewBox.Visibility = Visibility.Visible;
            FeedbackBox.Visibility = Visibility.Hidden;
            inReviewState = true;

            NextExerciseLabel.Text = currentWorkout.setList[currentSet + 1].exerciseName;
            DrawReviewSkeleton();
            CalculateAccuracy();
            mainController.SwitchMode(SpotMeController.ControllerMode.Continuous);
        }

        private void DisableSetReviewOverlay()
        {
            CountdownToNextSet.Visibility = Visibility.Hidden;
            SetReviewBox.Visibility = Visibility.Hidden;
            FeedbackBox.Visibility = Visibility.Visible;
            inReviewState = false;
            mainController.SwitchMode(SpotMeController.ControllerMode.Set);
        }

        private void WorkoutComplete()
        {
            currentSet = 0;
            inReviewState = true;
            FeedbackBox.Visibility = Visibility.Hidden;
            DoneWorkout.Visibility = Visibility.Visible;
            mainController.SwitchMode(SpotMeController.ControllerMode.Continuous);
            WorkoutProgressBar.Visibility = Visibility.Hidden;
        }

        private void CalculateAccuracy()
        {
            int numberOfGoodForm = mainController.storedRepClassIndex.Count(x => (x == 1 || x == 0));
            double accuracyPrecentage = numberOfGoodForm * 100 / mainController.totalRecordedSkeletons;
            FormAccuracyBlock.Text = Math.Round(accuracyPrecentage).ToString() + " %";
        }

        private void DrawReviewSkeleton()
        {
            using (DrawingContext dc = reviewFrameDrawingGroup.Open())
            {
                Pen inputPen = new Pen(Brushes.Red, 1);
                Pen blackPen = new Pen(Brushes.Black, 1);

                bodyDouble inputBody = mainController.storedSkeletons[mainController.skeletonViewIndex];

                drawingObject.DrawBodyDoubleProjection(dc, inputBody, inputPen, 120, 120);
            }
        }

        private async Task CountDownDelay(int timer)
        {
            int currentDelayValue = timer;

            while (currentDelayValue > 0)
            {
                if (currentDelayValue % 5 == 0)
                {
                    mainController.LoadNextSkeleton();
                    SkeletonIndexLabel.Text = (mainController.skeletonViewIndex + 1).ToString() + " of " + mainController.totalRecordedSkeletons.ToString();
                    ReviewOutputMessageLabel.Text = mainController.getOutputMessageFromClassIndex(mainController.storedRepClassIndex[mainController.skeletonViewIndex]);
                    DrawReviewSkeleton();
                }
                CountdownTimer.Text = currentDelayValue.ToString();
                await Task.Delay(1000);
                currentDelayValue--;
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

        private void ViewModeFunction(object sender, RoutedEventArgs e)
        {
            mainController.SwitchMode(SpotMeController.ControllerMode.View);
        }

        private void LoadNextExerciseBtn(object sender, RoutedEventArgs e)
        {
            mainController.LoadNextSkeleton();
        }

        private void LoadPrevExerciseBtn(object sender, RoutedEventArgs e)
        {
            mainController.LoadPrevSkeleton();
        }
    }
}
