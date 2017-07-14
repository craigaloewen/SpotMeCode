using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning.VectorMachines;
using Microsoft.Kinect;

namespace SpotMe
{
    /// <summary>
    /// The brain behind the Machine Learning of Spot Me, used to determine exercise classification and exercise decision making
    /// </summary>
    class SpotMeML
    {

        private MulticlassSupportVectorMachine<Gaussian> machine;
        private double[] lastKnownInput = null;

        private const double predictionProbabilityCeiling = 1.3;

        // A bool to act as a Schmidtt Trigger for movement detection
        public bool hasReportedMovement = false;

        public double movementIndexValue = 0;
        private const double movementIndexRetentionRate = ( 1 - 0.2 ); // The - 0.2 is to put it in a 'decay rate' format which is easier to conceptualize
        private const double lowerMovementLimit = 0.1;
        private const double upperMovementLimit = 0.2;

        // DEBUG DATA
        public double[] goodForm = null;

        public void init()
        {
            Accord.Math.Random.Generator.Seed = 0;
            lastKnownInput = null;

            double[][] inputs;
            int[] outputs;

            bool result = TrainingDataIO.readTrainingDataWithClassifiers("mp.csv", out inputs, out outputs);

            // DEBUG DATA
            goodForm = inputs[0];
            // ----


            // Create the multi-class learning algorithm for the machine
            var teacher = new MulticlassSupportVectorLearning<Gaussian>()
            {
                // Configure the learning algorithm to use SMO to train the
                //  underlying SVMs in each of the binary class subproblems.
                Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
                {
                    
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

        public int getClassPrediction(Body inBody)
        {

            double[] inputData = SkeletonModifier.preprocessSkeleton(inBody);

            int prediction = machine.Decide(inputData);
            double probability = machine.Score(inputData);

            if (probability < predictionProbabilityCeiling)
            {
                return -1;
            } else
            {
                return prediction;
            }
        }

        private double getMovementSquaredDiff(double[] inputA, double[] inputB)
        {
            double returnValue = 0;

            for (int i = 0; i < inputA.Length; i++)
            {
                double inputDiff = inputA[i] - inputB[i];
                returnValue += ( inputDiff * inputDiff );
            }

            return returnValue;
        }

        public bool hasBodyPaused(Body inBody)
        {
            double[] currentInput = SkeletonModifier.preprocessSkeleton(inBody);

            // Test if this is the first run
            if (lastKnownInput == null)
            {
                lastKnownInput = currentInput;
                return false;
            }

            // Get number difference between inputs
            double movementSquaredDiff = getMovementSquaredDiff(currentInput,lastKnownInput);
            
            movementIndexValue += movementSquaredDiff;
            //Decay the value
            movementIndexValue *= movementIndexRetentionRate;

            lastKnownInput = currentInput;

            if (movementIndexValue < lowerMovementLimit && !hasReportedMovement)
            {
                hasReportedMovement = true;
                System.Diagnostics.Debug.Print("Movement Stop Detected");
                return true;
            }

            if (movementIndexValue > upperMovementLimit && hasReportedMovement)
            {
                hasReportedMovement = false;
            }

            return false;
        }
    }
}
