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
        private int classifierId;
        public int id
        {
            get
            {
                return classifierId;
            }
            set
            {
                classifierId = value;
                foreach (TrainingData t in formTrainingData)
                {
                    t.classifierID = classifierId;
                }
            }
        }
        public string name;
        public string message;
        public SkeletonForm form;
        public List<TrainingData> formTrainingData;
        public string exerciseName;

        public Classifier(int inId, string inName, string inMessage, SkeletonForm inForm, List<TrainingData> inTrainingData, string inExerciseName)
        {
            id = inId;
            name = inName;
            message = inMessage;
            form = inForm;
            formTrainingData = inTrainingData;
            exerciseName = inExerciseName;
        }
    }
}
