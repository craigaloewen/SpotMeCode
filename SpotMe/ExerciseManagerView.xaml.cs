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

namespace SpotMe
{
    /// <summary>
    /// Interaction logic for ExerciseManagerView.xaml
    /// </summary>
    public partial class ExerciseManagerView : Page
    {
        public ExerciseManagerView()
        {
            InitializeComponent();

            ExerciseListBox.ItemsSource = ExerciseManager.GetExerciseNames();
        }

        public void OpenExercise (object sender, RoutedEventArgs e)
        {
            string exerciseName = ExerciseListBox.SelectedValue.ToString();
            
            if (exerciseName != "")
            {
                ClearForms();
                Exercise openedExercise = ExerciseManager.LoadExercise(exerciseName);

                ExerciseNameTxtBox.Text = openedExercise.name;
                ClassifierListBox.ItemsSource = openedExercise.GetClassifierNames();
            }
        }

        public void DeleteExercise(object sender, RoutedEventArgs e)
        {
            string exerciseName = ExerciseListBox.SelectedValue.ToString();

            if (exerciseName != "")
            {
                ExerciseManager.DeleteExercise(exerciseName);

                // Reload Exercise List
                ExerciseListBox.ItemsSource = ExerciseManager.GetExerciseNames();
                ClearForms();
            }
        }

        public void ClearForms(object sender, RoutedEventArgs e)
        {
            ClearForms();
        }

        public void ClearForms()
        {
            ClearExerciseForms();
            ClearClassifierForms();
        }

        public void ClearExerciseForms()
        {
            ExerciseNameTxtBox.Clear();
            ClassifierListBox.ItemsSource = null;
        }

        public void ClearClassifierForms()
        {
            ClassifierNameTxtBox.Clear();
            ClassifierMessageTxtBox.Clear();
        }

        public void SaveExercise(object sender, RoutedEventArgs e)
        {
            string exerciseName = ExerciseNameTxtBox.Text;

            if (exerciseName != "")
            {
                Exercise newExercise;
                try
                {
                    newExercise = ExerciseManager.LoadExercise(exerciseName);
                }
                catch (Exception ex)
                {
                    // Exercise file not found
                    newExercise = new Exercise(exerciseName);
                }
                
                ExerciseManager.SaveExercise(newExercise);

                // Reload Exercise List
                ExerciseListBox.ItemsSource = ExerciseManager.GetExerciseNames();
                ClearForms();
            }
        }

        public void OpenClasifier(object sender, RoutedEventArgs e)
        {
            string classifierName = ClassifierListBox.SelectedValue.ToString();
            string exerciseName = ExerciseNameTxtBox.Text;

            if (classifierName != "" && exerciseName != "")
            {
                ClearClassifierForms();
                Exercise openedExercise = ExerciseManager.LoadExercise(exerciseName);
                Classifier openedClassifier = openedExercise.GetClassifierByName(classifierName);

                ClassifierNameTxtBox.Text = openedClassifier.name;

                if (openedClassifier.form == SkeletonForm.Contracted)
                {
                    ClassifierContractedFormBtn.IsChecked = true;
                }
                else
                {
                    ClassifierExtendedFormBtn.IsChecked = true;
                }

                ClassifierMessageTxtBox.Text = openedClassifier.message;
            }
        }

        public void SaveClassifier(object sender, RoutedEventArgs e)
        {
            string classifierName = ClassifierNameTxtBox.Text;
            string exerciseName = ExerciseNameTxtBox.Text;

            if (classifierName != "" && exerciseName != "")
            {
                Exercise openedExercise = ExerciseManager.LoadExercise(exerciseName);
                SkeletonForm newClassifierForm = SkeletonForm.Contracted;

                if (ClassifierExtendedFormBtn.IsChecked == true)
                {
                    newClassifierForm = SkeletonForm.Extended;
                }

                openedExercise.AddClassifier(newClassifierForm, classifierName, ClassifierMessageTxtBox.Text);
                ExerciseManager.SaveExercise(openedExercise);

                // Reload Classifier List
                ClassifierListBox.ItemsSource = openedExercise.GetClassifierNames();
                ClearClassifierForms();
            }
        }

        public void DeleteClassifier(object sender, RoutedEventArgs e)
        {
            string classifierName = ClassifierNameTxtBox.Text;
            string exerciseName = ExerciseNameTxtBox.Text;

            if (classifierName != "" && exerciseName != "")
            {
                Exercise openedExercise = ExerciseManager.LoadExercise(exerciseName);
                Classifier openedClassifier = openedExercise.GetClassifierByName(classifierName);

                openedExercise.DeleteClassifier(openedClassifier.id);
                ExerciseManager.SaveExercise(openedExercise);

                // Reload Classifier List
                ClassifierListBox.ItemsSource = openedExercise.GetClassifierNames();
                ClearClassifierForms();
            }
        }

        private void BackBtnClick(object sender, RoutedEventArgs e)
        {
            ExerciseListView viewPage = new ExerciseListView();
            NavigationService.Navigate(viewPage);
        }

        private void AddTrainingDataBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int inputDelay = Convert.ToInt32(CaptureDelayBox.Text);
                int inputNumCaptures = Convert.ToInt32(NumberOfCapturesBox.Text);
                int inputInitialDelay = Convert.ToInt32(InitialDelayBox.Text);

                string classifierName = ClassifierNameTxtBox.Text;
                string exerciseName = ExerciseNameTxtBox.Text;

                Exercise openedExercise = new Exercise();
                Classifier openedClassifier = new Classifier();

                if (classifierName != "" && exerciseName != "")
                {
                    openedExercise = ExerciseManager.LoadExercise(exerciseName);
                    openedClassifier = openedExercise.GetClassifierByName(classifierName);
                } else
                {
                    Exception newException = new Exception("Exercise or classifier not selected");
                    throw newException;
                }

                Window captureWindow = new RecordTrainingDataWindow(openedExercise,openedClassifier.id, inputInitialDelay, inputDelay,inputNumCaptures);
                captureWindow.Show();
            }
            catch (Exception exception)
            {
                TrainingDataMessage.Visibility = Visibility.Visible;
                TrainingDataMessage.Text = exception.Message;
            }
        }
    }
}
