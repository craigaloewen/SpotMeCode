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
                    // Store number of classifiers
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

        public static Exercise LoadExercise(string ExerciseName)
        {

            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader(filePathPrefix + ExerciseName + fileExtension))
            {
                string line;
                int numberOfSets;

                line = sr.ReadLine();

                numberOfSets = Convert.ToInt32(line);

                for (int i = 0; i < numberOfSets; i++)
                {
                    string classifierName = sr.ReadLine();
                    string classifierMessage = sr.ReadLine();
                    SkeletonForm classifierForm = (SkeletonForm)Enum.Parse(typeof(SkeletonForm), sr.ReadLine());
                    outputExercise.AddClassifier(classifierForm, classifierName, classifierMessage);
                }

            }

            outputExercise.UpdateFormDefinitions();

            return outputExercise;
        }
    }
}
