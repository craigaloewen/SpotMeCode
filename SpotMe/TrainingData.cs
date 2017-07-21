using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    class TrainingData
    {
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
    }
}
