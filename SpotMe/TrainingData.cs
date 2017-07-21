using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    class TrainingData
    {
        // Can this string be replaced with the index of the parent classifier? Or a reference to the parent?
        // That would cut down storage space significantly and a string isn't congruent with the file structure in Exercise.cs or Classifier.cs
        public string classifierName;
        public int classifierIndex;
        public double[] formData;

        public TrainingData() { }

        public TrainingData(string inClassifierName, int inClassIndex, double[] inFormData)
        {
            classifierName = inClassifierName;
            classifierIndex = inClassIndex;
            formData = inFormData;
        }

        public TrainingData(Classifier parentClassifier, double[] inFormData)
        {
            classifierName = parentClassifier.name;
            classifierIndex = parentClassifier.formTrainingData.Count;
            formData = inFormData;
        }
    }
}
