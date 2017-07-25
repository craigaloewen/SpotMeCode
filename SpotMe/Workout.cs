using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    public class Workout
    {
        
        public int setRestTime { get; private set; }

        public int exerciseRestTime { get; private set; }

        public string name { get; private set; }

        public List<Set> setList { get; private set; }

        public int numberOfSets { get; private set; }

        /// <summary>
        /// Create a new workout
        /// </summary>
        /// <param name="inputName">Name of workout</param>
        /// <param name="inputSetRestTime">Rest time between sets in seconds</param>
        /// <param name="inputExerciseRestTime">Rest time between exercises in seconds</param>
        public Workout(string inputName, int inputSetRestTime, int inputExerciseRestTime) 
        {
            name = inputName;
            setRestTime = inputSetRestTime;
            exerciseRestTime = inputExerciseRestTime;

            numberOfSets = 0;

            setList = new List<Set>();
        }

        public bool AddSet(int numberOfReps, string exerciseName)
        {
            Set newSet = new Set(numberOfReps, exerciseName, numberOfSets);
            setList.Add(newSet);
            numberOfSets++;
            return true;
        }

        public bool RemoveSet(int inputIndex)
        {
            if (inputIndex < numberOfSets && inputIndex >= 0)
            {
                setList.RemoveAt(inputIndex);
                numberOfSets--;
                return true;
            } else
            {
                return false;
            }
        }

        public List<string> GetSetNames()
        {
            List<string> returnList = new List<string>();

            foreach (Set set in setList)
            {
                returnList.Add(set.exerciseName);
            }

            return returnList;
        }
    }
}
