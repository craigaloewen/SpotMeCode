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
    /// Interaction logic for ExerciseView.xaml
    /// </summary>
    public partial class ExerciseView : Page
    {
        Exercise mainExercise;

        public ExerciseView(Exercise inputExercise)
        {
            InitializeComponent();
            mainExercise = inputExercise;

        }

        private void BacktoExerciseList(object sender, RoutedEventArgs e)
        {
            ExerciseListView viewPage = new SpotMe.ExerciseListView();
            NavigationService.Navigate(viewPage);
        }
    }
}
