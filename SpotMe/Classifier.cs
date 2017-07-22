using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    public enum SkeletonForm
    {
        Contracted,
        Extended
    }
    /// <summary>
    /// Stores all the information necessary to describe a classifier of an exercise's form
    /// </summary>
    public class Classifier
    {
        public int id;
        public string name;
        public string message;
        public SkeletonForm form;
        public List<TrainingData> formTrainingData;
        public Exercise parentExercise;
        
        public Classifier()
        {
            formTrainingData = new List<SpotMe.TrainingData>();
        }

        public Classifier(Exercise inputParentExercise, SkeletonForm inputForm, string inputName, string inputMessage)
        {
            formTrainingData = new List<SpotMe.TrainingData>();
            id = inputParentExercise.classifierData.Count;
            name = inputName;
            message = inputMessage;
            form = inputForm;
            parentExercise = inputParentExercise;
        }

        public bool AddTrainingData(double[] inputData)
        {
            TrainingData newData = new SpotMe.TrainingData(this, inputData);
            formTrainingData.Add(newData);
            return true;
        }

        public bool DeleteTrainingData(int trainingDataIndex)
        {
            if ( trainingDataIndex < formTrainingData.Count )
            {
                formTrainingData.RemoveAt(trainingDataIndex);
                reIndexTrainingDataStartingAt(trainingDataIndex);
                return true;
            } else
            {
                return false;
            }
        }

        // Whenever we add/delete the trainingDataList we should reIndex
        public void reIndexTrainingData()
        {
            for (int i = 0; i < formTrainingData.Count; ++i)
            {
                formTrainingData[i].id = i;
            }
        }

        private void reIndexTrainingDataStartingAt(int startingIndex)
        {
            if (startingIndex >= formTrainingData.Count)
            {
                return;
            }

            for (int i = startingIndex; i < formTrainingData.Count; ++i)
            {
                formTrainingData[i].id = i;
            }
        }
    }
}
