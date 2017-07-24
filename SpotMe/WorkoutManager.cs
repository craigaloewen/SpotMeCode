using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    static class WorkoutManager
    {
        static char listDelimiter = ',';
        static string filePathPrefix = "../../workoutData/";
        static string fileExtension = ".txt";

        public static bool SaveWorkout(Workout inputWorkout)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePathPrefix + inputWorkout.name + fileExtension))
                {

                    sw.WriteLine(inputWorkout.setRestTime);
                    sw.WriteLine(inputWorkout.exerciseRestTime);

                    // Store number of sets
                    sw.WriteLine(inputWorkout.numberOfSets);

                    foreach (Set someSet in inputWorkout.setList)
                    {
                        sw.WriteLine(someSet.exerciseName);
                        sw.WriteLine(someSet.numberOfReps);
                    }
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be written to:");
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public static Workout LoadWorkout(string workoutName)
        {

            Workout returnWorkout;

            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader(filePathPrefix + workoutName + fileExtension))
            {
                string line;
                int numberOfSets;
                int setRestTime;
                int exerciseRestTime;

                line = sr.ReadLine();
                setRestTime = Convert.ToInt32(line);

                line = sr.ReadLine();
                exerciseRestTime = Convert.ToInt32(line);

                line = sr.ReadLine();
                numberOfSets = Convert.ToInt32(line);

                returnWorkout = new Workout(workoutName, setRestTime, exerciseRestTime);

                for (int i = 0; i < numberOfSets; i++)
                {
                    string exerciseName = sr.ReadLine();
                    int numberOfReps = Convert.ToInt32(sr.ReadLine());
                    returnWorkout.AddSet(numberOfReps, exerciseName);
                }
            }
            return returnWorkout;
        }

        public static List<string> GetWorkoutNames()
        {
            List<string> returnList = new List<string>();

            string[] fileEntries = Directory.GetFiles(filePathPrefix);

            foreach (string someString in fileEntries)
            {
                string[] fileNameSplit = someString.Split('/');
                string[] extensionNameSplit = fileNameSplit.Last().Split('.');
                returnList.Add(extensionNameSplit.First());
            }

            return returnList;
        }

    }
}
