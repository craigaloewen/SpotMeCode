﻿using System;
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

using Accord.Math;
using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.Math.Optimization.Losses;

namespace SpotMe
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        private void Button3_Function(object sender, RoutedEventArgs e)
        {

        }

        private void Button2_Function(object sender, RoutedEventArgs e)
        {

        }

        private void Button1_Function(object sender, RoutedEventArgs e)
        {
            Accord.Math.Random.Generator.Seed = 0;

            double[][] inputData = TrainingDataIO.readTrainingData("testData.csv");
            double[][] testInputs = TrainingDataIO.readTrainingData("bicepCurlData.csv");

            double[][] inputs = inputData.MemberwiseClone();

            int[] outputs =
            {
                0,0,0,
                1,1,1,1
            };

            // Create the multi-class learning algorithm for the machine
            var teacher = new MulticlassSupportVectorLearning<Gaussian>()
            {
                // Configure the learning algorithm to use SMO to train the
                //  underlying SVMs in each of the binary class subproblems.
                Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
                {
                    // Estimate a suitable guess for the Gaussian kernel's parameters.
                    // This estimate can serve as a starting point for a grid search.
                    UseKernelEstimation = true
                }
            };

            // Configure parallel execution options
            teacher.ParallelOptions.MaxDegreeOfParallelism = 1;

            // Learn a machine
            var machine = teacher.Learn(inputs, outputs);

            // Obtain class predictions for each sample
            int[] predicted = machine.Decide(inputs);

            // Get class scores for each sample
            double[] scores = machine.Score(inputs);

            // Compute classification error
            double error = new ZeroOneLoss(outputs).Loss(predicted);
        }
    }
}