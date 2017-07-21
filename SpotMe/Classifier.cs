﻿using System;
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

        public Classifier(Exercise parentExercise, SkeletonForm inputForm, string inputName, string inputMessage)
        {
            formTrainingData = new List<SpotMe.TrainingData>();
            id = parentExercise.classifierData.Count;
            name = inputName;
            message = inputMessage;
            form = inputForm;
            exerciseName = parentExercise.name;
        }

        public Classifier(int inId, string inName, string inMessage, SkeletonForm inForm, List<TrainingData> inTrainingData, string inExerciseName)
        {
            id = inId;
            name = inName;
            message = inMessage;
            form = inForm;
            formTrainingData = inTrainingData;
            exerciseName = inExerciseName;
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
                formTrainingData[i].classifierIndex = i;
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
                formTrainingData[i].classifierIndex = i;
            }
        }
    }
}
