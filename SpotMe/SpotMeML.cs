using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning.VectorMachines;

namespace SpotMe
{
    class SpotMeML
    {

        MulticlassSupportVectorMachine<Gaussian> machine;

        public void init()
        {
            Accord.Math.Random.Generator.Seed = 0;

            double[][] inputs;
            int[] outputs;

            bool result = TrainingDataIO.readTrainingDataWithClassifiers("mp.csv", out inputs, out outputs);

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

            // Make the machine learn
            machine = teacher.Learn(inputs, outputs);

            // Create the multi-class learning algorithm for the machine
            var calibration = new MulticlassSupportVectorLearning<Gaussian>()
            {
                Model = machine, // We will start with an existing machine

                // Configure the learning algorithm to use Platt's calibration
                Learner = (param) => new ProbabilisticOutputCalibration<Gaussian>()
                {
                    Model = param.Model // Start with an existing machine
                }
            };


            // Configure parallel execution options
            calibration.ParallelOptions.MaxDegreeOfParallelism = 1;

            // Learn a machine
            calibration.Learn(inputs, outputs);

            // Obtain class predictions for each sample
            int[] predicted = machine.Decide(inputs);

            // Get class scores for each sample
            double[] scores = machine.Score(inputs);

        }

        public int getClassPrediction(double[] inputData)
        {
            int prediction = machine.Decide(inputData);
            double probability = machine.Score(inputData);

            if (probability < 0.5)
            {
                return -1;
            } else
            {
                return prediction;
            }
        }

    }
}
