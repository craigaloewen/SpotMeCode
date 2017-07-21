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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace SpotMe
{
    /// <summary>
    /// Interaction logic for ExerciseList.xaml
    /// </summary>
    public partial class ExerciseList : Page
    {

        List<string> exerciseNameList;

        int previewFrameWidth = 120;
        int previewFrameHeight = 120;

        private DrawingImage bodyFrameSource;

        private DrawingGroup bodyFrameDrawingGroup;

        public ExerciseList()
        {
            

            // Load exercise name data

            exerciseNameList = new List<string>();

            exerciseNameList.Add("Test 1");
            exerciseNameList.Add("Test 2");
            exerciseNameList.Add("Test 3");
            exerciseNameList.Add("Test 4");
            exerciseNameList.Add("Test 5");


            Exercise selectedExercise = new SpotMe.Exercise();
            selectedExercise.contractedForm = new double[24] { -1,0,0,1,0,0,-1,0,0,1,0,0,1,0,0,1,0,0,1,0,0,1,0,0 };
            bodyDouble inputBody = SkeletonModifier.TrainingDataTo3DSkeleton(selectedExercise.contractedForm);

            // Create the drawing group we'll use for drawing
            bodyFrameDrawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.bodyFrameSource = new DrawingImage(this.bodyFrameDrawingGroup);

            SkeletonDrawing drawingObject = new SkeletonDrawing(null);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            using (DrawingContext dc = bodyFrameDrawingGroup.Open())
            {
                Pen inputPen = new Pen(Brushes.Red, 1);
                Pen blackPen = new Pen(Brushes.Black, 1);

                dc.DrawRectangle(Brushes.Black, blackPen, new Rect(0, 0, previewFrameWidth, previewFrameHeight));
                drawingObject.DrawBodyDoubleProjection(dc, inputBody, inputPen, previewFrameWidth, previewFrameHeight);
            }

            InitializeComponent();
            listBox.ItemsSource = exerciseNameList;

        }



        public ImageSource BodyImageSource
        {
            get
            {
                return this.bodyFrameSource;
            }
        }

        private void StartExercise(object sender, RoutedEventArgs e)
        {
            Exercise selectedExercise = new SpotMe.Exercise();

            // Load selectedExercise
            string exerciseName;
            try
            {
                exerciseName = listBox.SelectedItem.ToString();

                selectedExercise.contractedForm = new double[24] { -1,0,0,1,0,0,-1,0,0,1,0,0,1,0,0,1,0,0,1,0,0,1,0,0 };

                ExerciseView viewPage = new SpotMe.ExerciseView(selectedExercise);
                NavigationService.Navigate(viewPage);
            }
            catch (Exception exception)
            {
                InstructionsText.Text = "Error: '" + exception.Message + "' occured \nPlease try again.";
            }
        }
    }
}
