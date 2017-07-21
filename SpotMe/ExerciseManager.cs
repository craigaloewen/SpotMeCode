using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SpotMe
{
    static class ExerciseManager
    {
        static char delimiter = ';';
        static char listDelimiter = ',';
        static string exerciseFileNamePreposition = "EXERCISE-";
        static string fileExtension = ".txt";

        /*
         *Exercise File Format:
         *  Name;<ContractedForm>3.49,5.67;<ExtendedForm>2.45,6.57;CLASS1,CLASS2
         */

        public static bool saveExercise(Exercise inExercise)
        {
            StringBuilder exerciseData = new StringBuilder();

            // Process exercise data
            string upperName = inExercise.name.ToUpper();

            if (upperName == "") return false;

            exerciseData.Append(upperName);
            exerciseData.Append(delimiter);

            if (inExercise.contractedForm.Length > 0)
            {
                exerciseData.Append(inExercise.contractedForm[0].ToString());

                for (int i = 1; i < inExercise.contractedForm.Length; ++i)
                {
                    exerciseData.Append(listDelimiter);
                    exerciseData.Append(inExercise.contractedForm[i].ToString());
                }              
            }
            exerciseData.Append(delimiter);

            if (inExercise.extendedForm.Length > 0)
            {
                exerciseData.Append(inExercise.extendedForm[0].ToString());

                for (int i = 1; i < inExercise.extendedForm.Length; ++i)
                {
                    exerciseData.Append(listDelimiter);
                    exerciseData.Append(inExercise.extendedForm[i].ToString());
                }
            }
            exerciseData.Append(delimiter);

            foreach (Classifier c in inExercise.classifierData)
            {

            }
            if (inExercise.classifierData.Count > 0)
            {
                exerciseData.Append(inExercise.classifierData[0].name.ToString());
                // saveClassifier(classifierData[0])

                for (int i = 1; i < inExercise.classifierData.Count; ++i)
                {
                    exerciseData.Append(listDelimiter);
                    exerciseData.Append(inExercise.classifierData[i].name.ToString());
                    // saveClassifier(classifierData[i])
                }
            }

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(exerciseFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);
                File.WriteAllText(@"\ExerciseData\" + filePath.ToString(), exerciseData.ToString());
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static Exercise loadExercise(string exerciseName)
        {
            string upperName = exerciseName.ToUpper();

            if (upperName == "") return null;

            StringBuilder filePath = new StringBuilder();
            filePath.Append(exerciseFileNamePreposition);
            filePath.Append(upperName);
            filePath.Append(fileExtension);

            try
            {
                string exerciseDataString = File.ReadAllText(@"\ExerciseData\" + filePath.ToString());

                string[] exerciseData = exerciseDataString.Trim().Split(delimiter);

                Exercise loadedExercise = new Exercise();

                // There should only be 4 components
                if (exerciseData.Length > 4) return null;

                if (upperName != exerciseData[0].ToUpper()) return null;

                loadedExercise.name = exerciseData[0];

                string[] contractedFormDataString = exerciseData[1].Split(listDelimiter);

                if (contractedFormDataString.Length > 0 && contractedFormDataString[0] != "")
                {
                    loadedExercise.contractedForm = new double[contractedFormDataString.Length];

                    for (int i = 0; i < loadedExercise.contractedForm.Length; ++i)
                    {
                        loadedExercise.contractedForm[i] = Double.Parse(contractedFormDataString[i]);
                    }
                }

                string[] extendededFormDataString = exerciseData[2].Split(listDelimiter);

                if (extendededFormDataString.Length > 0 && extendededFormDataString[0] != "")
                {
                    loadedExercise.extendedForm = new double[contractedFormDataString.Length];

                    for (int i = 0; i < loadedExercise.extendedForm.Length; ++i)
                    {
                        loadedExercise.extendedForm[i] = Double.Parse(extendededFormDataString[i]);
                    }
                }

                string[] classifierStringList = exerciseData[3].Split(listDelimiter);

                if (classifierStringList.Length > 0 && classifierStringList[0] != "")
                {
                    loadedExercise.classifierData = new List<Classifier>();

                    foreach (string c in classifierStringList)
                    {
                        string classifierNameUpper = c.ToUpper();
                        // Classifier loadedClassifier = loadClassifier(classifierNameUpper);
                        // if (loadedClassifier != null) loadedExercise.classifierData.Append(loadedClassifier);
                    }
                }

                return loadedExercise;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static bool deleteExercise(Exercise inExercise)
        {
            string upperName = inExercise.name.ToUpper();

            if (upperName == "") return false;

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(exerciseFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);
                File.Delete(@"\ExerciseData\" + filePath.ToString());

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool deleteExercise(string exerciseName)
        {
            string upperName = exerciseName.ToUpper();

            if (upperName == "") return false;

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(exerciseFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);
                File.Delete(@"\ExerciseData\" + filePath.ToString());

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
