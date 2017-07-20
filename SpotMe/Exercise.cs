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
    class Exercise
    {
        private string exerciseName;
        public string name
        {
            get
            {
                return exerciseName;
            }
            set
            {
                exerciseName = value.ToUpper();
                foreach (Classifier c in classifierData)
                {
                    c.exerciseName = exerciseName;
                }
            }
        }
        public double[] contractedForm;
        public double[] extendedForm;
        public List<Classifier> classifierData;

        public Exercise() { }

        public Exercise(string inName, double[] inContractedForm, double[] inExtendedForm, List<Classifier> inClassifierList)
        {
            name = inName;
            contractedForm = inContractedForm;
            extendedForm = inExtendedForm;
            classifierData = inClassifierList;
        }

    }
}
