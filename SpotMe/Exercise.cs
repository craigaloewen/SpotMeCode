using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    class Exercise
    {

        public string exerciseName;
        public double[] contractedForm;
        public double[] extendedForm;
        public List<Classifier> classifierList;

        public Exercise()
        {

        }

        public Exercise(string inName, double[] inContractedForm, double[] inExtendedForm, List<Classifier> inClassifierList)
        {
            exerciseName = inName;
            contractedForm = inContractedForm;
            extendedForm = inExtendedForm;
            classifierList = inClassifierList;
        }

    }
}
