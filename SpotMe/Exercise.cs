using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    /// <summary>
    /// Stores all the data necessary to describe an exercise
    /// </summary>
    public class Exercise
    {
        public string name;
        public double[] contractedForm;
        public double[] extendedForm;
        public List<Classifier> classifierData;

        public Exercise()
        {
            classifierData = new List<Classifier>();
        }

        // This is the constructor that will be called based on inputted data
        public Exercise(string inName)
        {
            classifierData = new List<Classifier>();
            name = inName;
        }

        public bool AddClassifier(SkeletonForm inputForm, string inputName, string inputMessage)
        {
            Classifier newClassifier = new SpotMe.Classifier(this, inputForm, inputName, inputMessage);
            classifierData.Add(newClassifier);
            return true;
        }

        public bool DeleteClassifier(int classifierIndex)
        {
            if (classifierIndex < classifierData.Count)
            {
                classifierData.RemoveAt(classifierIndex);
                reIndexClassifiersStartingAt(classifierIndex);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddTrainingData(int classifierIndex, double[] inputData)
        {
            return classifierData[classifierIndex].AddTrainingData(inputData);
        }

        public bool DeleteTrainingData(int classifierIndex, int trainingDataIndex)
        {
            if (classifierIndex < classifierData.Count)
            {
                return classifierData[classifierIndex].DeleteTrainingData(trainingDataIndex);
            } else
            {
                return false;
            }
        }

        public bool UpdateFormDefinitions()
        {
            if (classifierData.Count < 2)
            {
                return false;
            }

            if (classifierData[0].formTrainingData.Count < 1 || classifierData[1].formTrainingData.Count < 1)
            {
                return false;
            }

            contractedForm = classifierData[0].formTrainingData[0].formData;
            extendedForm = classifierData[1].formTrainingData[0].formData;

            return true;
        }

        private void reIndexClassifiersStartingAt(int startingIndex)
        {
            if (startingIndex >= classifierData.Count)
            {
                return;
            }

            for (int i = startingIndex; i < classifierData.Count; ++i)
            {
                classifierData[i].id = i;
            }
        }

        public bool GetTrainingData(out double[][] inputs, out int[] outputs)
        {
            int trainingDataNum = 0;

            foreach (Classifier someClassifier in classifierData)
            {
                trainingDataNum += someClassifier.formTrainingData.Count;
            }

            inputs = new double[trainingDataNum][];
            outputs = new int[trainingDataNum];

            int trainingDataCount = 0;

            foreach (Classifier someClassifier in classifierData)
            {
                foreach (TrainingData someTrainingData in someClassifier.formTrainingData)
                {
                    inputs[trainingDataCount] = someTrainingData.formData;
                    outputs[trainingDataCount] = someTrainingData.id;
                    trainingDataCount++;
                }
            }

            return true;
        }

    }
}
