using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    enum SkeletonForm
    {
        Contracted,
        Extended
    }
    /// <summary>
    /// Stores all the information necessary to describe a classifier of an exercise's form
    /// </summary>
    class Classifier
    {
        public int id;
        private string classifierName;
        public string name
        {
            get
            {
                return classifierName;
            }
            set
            {
                classifierName = value.ToUpper();
                foreach (TrainingData t in formTrainingData)
                {
                    t.classifierName = classifierName;
                }
            }
        }
        public string message;
        public string exerciseName;
        public SkeletonForm form;
        public List<TrainingData> formTrainingData;  
        
        public Classifier() { }

        public Classifier(int inId, string inName, string inMessage, SkeletonForm inForm, List<TrainingData> inTrainingData, string inExerciseName)
        {
            id = inId;
            name = inName;
            message = inMessage;
            form = inForm;
            formTrainingData = inTrainingData;
            exerciseName = inExerciseName;
        }

        // Whenever we add/delete the trainingDataList we should reIndex
        public void reIndexTrainingData()
        {
            for (int i = 0; i < formTrainingData.Count; ++i)
            {
                formTrainingData[i].classifierIndex = i;
            }
        }
    }
}
