using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    class TrainingData
    {
        public int classifierID;
        public double[] formData;

        public TrainingData(int inClassId, double[] inFormData)
        {
            classifierID = inClassId;
            formData = inFormData;
        }
    }
}
