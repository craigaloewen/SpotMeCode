using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    public class Set
    {
        public int numberOfReps;

        public string exerciseName;

        public int setID;

        public Set(int inputNumberOfReps, string inputExerciseName, int inputID)
        {
            numberOfReps = inputNumberOfReps;
            exerciseName = inputExerciseName;
            setID = inputID;
        }


    }
}
