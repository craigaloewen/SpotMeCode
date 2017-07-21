using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    class TrainingData
    {
        public int id;
        public double[] formData;
        public Classifier parentClassifier;

        public TrainingData() { }

        public TrainingData(Classifier inputParentClassifier, double[] inFormData)
        {
            parentClassifier = inputParentClassifier;
            id = parentClassifier.formTrainingData.Count;
            formData = inFormData;
        }
    }
}
