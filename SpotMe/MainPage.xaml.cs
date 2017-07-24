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

namespace SpotMe
{
    /// <summary>
    /// Interaction logic for ExerciseList.xaml
    /// </summary>
    public partial class MainPage : Window
    {

        public MainPage()
        {
            InitializeComponent();

            MainFrame.Content = new ExerciseListView();
        }

        public void MainPage_Closing(object sender, EventArgs e)
        {
            MainFrame.Content = null;

            base.OnClosed(e);

            Application.Current.Shutdown();
        }


    }
}
