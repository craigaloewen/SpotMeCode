using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace SpotMe
{
    static class ExerciseManager
    {
        static char listDelimiter = ',';
        static string filePathPrefix = "../../exerciseData/";
        static string fileExtension = ".txt";

        public static bool SaveExercise(Exercise inputExercise)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePathPrefix + inputExercise.name + fileExtension))
                {
                    // Store number of classifiers
                    sw.WriteLine(inputExercise.classifierData.Count);

                    foreach (Classifier someClassifier in inputExercise.classifierData)
                    {
                        sw.WriteLine(someClassifier.name);
                        sw.WriteLine(someClassifier.message);
                        sw.WriteLine(someClassifier.form);
                    }

                    foreach(Classifier someClassifier in inputExercise.classifierData)
                    {
                        foreach(TrainingData someTrainingData in someClassifier.formTrainingData)
                        {
                            sw.Write(someTrainingData.parentClassifier.id);
                            for (int i = 0; i < someTrainingData.formData.Length; i++)
                            {
                                sw.Write(listDelimiter);
                                sw.Write(someTrainingData.formData[i]);
                            }
                            sw.WriteLine();
                        }
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
            
            Exercise outputExercise = new SpotMe.Exercise();

            outputExercise.name = ExerciseName;

            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader(filePathPrefix + ExerciseName + fileExtension))
            {
                string line;
                int numOfClassifications;

                line = sr.ReadLine();

                numOfClassifications = Convert.ToInt32(line);

                for (int i = 0; i < numOfClassifications; i++)
                {
                    string classifierName = sr.ReadLine();
                    string classifierMessage = sr.ReadLine();
                    SkeletonForm classifierForm = (SkeletonForm)Enum.Parse(typeof(SkeletonForm), sr.ReadLine());
                    outputExercise.AddClassifier(classifierForm, classifierName, classifierMessage);
                }

                while ((line = sr.ReadLine()) != null)
                {
                    string[] lineDataValues = line.Split(listDelimiter);
                    int dataClassifierIndex = Convert.ToInt32(lineDataValues[0]);

                    List<double> trainingDataValues = new List<double>();

                    for (int i = 1; i < lineDataValues.Length; i++)
                    {
                        trainingDataValues.Add(Convert.ToDouble(lineDataValues[i]));
                    }

                    double[] trainingDataArray = trainingDataValues.ToArray();

                    outputExercise.AddTrainingData(dataClassifierIndex, trainingDataArray);
                }

            }

            outputExercise.UpdateFormDefinitions();

            return outputExercise;
        }

        public static List<string> GetExerciseNames()
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
