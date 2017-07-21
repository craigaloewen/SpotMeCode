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
        static char delimiter = ';';
        static char listDelimiter = ',';
        static string exerciseFileNamePreposition = "EXERCISE-";
        static string classifierFileNamePreposition = "CLASSIFIER-";
        static string trainingDataFileNamePreposition = "TRAININGDATA-";
        static string fileExtension = ".txt";

        /*
         * Exercise File Format:
         *  Name;<ContractedForm>3.49,5.67;<ExtendedForm>2.45,6.57;CLASS1,CLASS2
         *  
         * Classifier File Format:
         *  Id;Name;Message;ExerciseName;Form;TrainingDataCount
         *  
         * Training Data File Format:
         *  ClassifierName;Id;<FormData>4.56,21.123
         */

        #region General IO
        public static List<Exercise> getExerciseList()
        {
            List<Exercise> exerciseList = new List<Exercise>();

            DirectoryInfo exerciseDirectory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\"));
            StringBuilder filesName = new StringBuilder();
            filesName.Append(exerciseFileNamePreposition);
            filesName.Append("*");
            filesName.Append(fileExtension);
            FileInfo[] exerciseFiles = exerciseDirectory.GetFiles(filesName.ToString());

            foreach (FileInfo f in exerciseFiles)
            {
                try
                {
                    string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + f.Name);
                    string exerciseDataString = File.ReadAllText(filePathInSystem);
                    string[] exerciseData = exerciseDataString.Trim().Split(delimiter);

                    Exercise ex = loadExercise(exerciseData[0]);
                    exerciseList.Add(ex);
                }
                catch(Exception e)
                {

                }
            }

            return exerciseList;
        }

        public static List<string> getExerciseNameList()
        {
            List<string> exerciseList = new List<string>();

            DirectoryInfo exerciseDirectory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\"));
            StringBuilder filesName = new StringBuilder();
            filesName.Append(exerciseFileNamePreposition);
            filesName.Append("*");
            filesName.Append(fileExtension);
            FileInfo[] exerciseFiles = exerciseDirectory.GetFiles(filesName.ToString());

            foreach (FileInfo f in exerciseFiles)
            {
                try
                {
                    string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + f.Name);
                    string exerciseDataString = File.ReadAllText(filePathInSystem);
                    string[] exerciseData = exerciseDataString.Trim().Split(delimiter);

                    exerciseList.Add(exerciseData[0]);
                }
                catch (Exception e)
                {

                }
            }

            return exerciseList;
        }

        // Completely deletes Exercise(Exercise, Classifier, TrainingData
        public static bool deleteExerciseSet(Exercise inExercise)
        {
            string upperName = inExercise.name.ToUpper();

            if (upperName == "") return false;

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(exerciseFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.Delete(filePathInSystem);

                foreach (Classifier c in inExercise.classifierData)
                {
                    deleteClassifierSet(c);
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        // Completely deletes Exercise(Exercise, Classifier, TrainingData
        public static bool deleteExerciseSet(string exerciseName)
        {
            Exercise toDeleteExercise = loadExercise(exerciseName);
            return deleteExerciseSet(toDeleteExercise);
        }
        #endregion

        #region Exercise Functions
        public static bool saveExercise(Exercise inExercise)
        {
            StringBuilder exerciseData = new StringBuilder();

            // Process exercise data
            string upperName = inExercise.name.ToUpper();

            if (upperName == "") return false;

            // Clear any previous versions
            deleteExercise(upperName);

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

            if (inExercise.classifierData.Count > 0)
            {
                exerciseData.Append(inExercise.classifierData[0].name.ToString());
                saveClassifier(inExercise.classifierData[0]);

                for (int i = 1; i < inExercise.classifierData.Count; ++i)
                {
                    exerciseData.Append(listDelimiter);
                    exerciseData.Append(inExercise.classifierData[i].name.ToString());
                    saveClassifier(inExercise.classifierData[i]);
                }
            }

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(exerciseFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                // The filePathInSystem is wrong, it is producing two / characters: so it is showing C:\\Users\\Documents... not C:\Users\Documents, etc.
                File.WriteAllText(filePathInSystem, exerciseData.ToString());
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
                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                string exerciseDataString = File.ReadAllText(filePathInSystem);

                string[] exerciseData = exerciseDataString.Trim().Split(delimiter);

                Exercise loadedExercise = new Exercise();

                // There should only be 4 components
                if (exerciseData.Length != 4) return null;

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
                        Classifier loadedClassifier = loadClassifier(classifierNameUpper);
                        if (loadedClassifier != null) loadedExercise.classifierData.Add(loadedClassifier);
                    }
                }

                return loadedExercise;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        // Deletes ONLY Exercise File for full delete see deleteExerciseSet
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

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.Delete(filePathInSystem);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        // Deletes ONLY Exercise File for full delete see deleteExerciseSet
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

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.Delete(filePathInSystem);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion

        #region Classifier Functions
        public static bool saveClassifier(Classifier inClassifier)
        {
            StringBuilder classifierData = new StringBuilder();

            // Process exercise data
            classifierData.Append(inClassifier.id);
            classifierData.Append(delimiter);

            string upperName = inClassifier.name.ToUpper();

            if (upperName == "") return false;

            // Delete any previous versions
            deleteClassifier(upperName);

            classifierData.Append(upperName);
            classifierData.Append(delimiter);

            classifierData.Append(inClassifier.message);
            classifierData.Append(delimiter);

            classifierData.Append(inClassifier.exerciseName);
            classifierData.Append(delimiter);

            if (inClassifier.form == SkeletonForm.Contracted)
            {
                classifierData.Append("CONTRACTED");
                classifierData.Append(delimiter);
            }
            else if (inClassifier.form == SkeletonForm.Extended)
            {
                classifierData.Append("EXTENDED");
                classifierData.Append(delimiter);
            }

            // TrainingData will be stored as CLASSIFIERNAME#.txt
            classifierData.Append(inClassifier.formTrainingData.Count);
            foreach(TrainingData t in inClassifier.formTrainingData)
            {
                saveTrainingData(t);
            }

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(classifierFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.WriteAllText(filePathInSystem, classifierData.ToString());
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static Classifier loadClassifier(string classifierName)
        {
            string upperName = classifierName.ToUpper();

            if (upperName == "") return null;

            StringBuilder filePath = new StringBuilder();
            filePath.Append(classifierFileNamePreposition);
            filePath.Append(upperName);
            filePath.Append(fileExtension);

            try
            {
                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                string classifierDataString = File.ReadAllText(filePathInSystem);

                string[] classifierData = classifierDataString.Trim().Split(delimiter);

                Classifier loadedClassifier = new Classifier();

                // There should only be 6 components
                if (classifierData.Length != 6) return null;

                loadedClassifier.id = Int32.Parse(classifierData[0]);

                if (upperName != classifierData[1].ToUpper()) return null;

                loadedClassifier.name = classifierData[1];

                loadedClassifier.message = classifierData[2];

                loadedClassifier.exerciseName = classifierData[3];

                if (classifierData[4] != "" && classifierData[4] == "CONTRACTED")
                {
                    loadedClassifier.form = SkeletonForm.Contracted;
                }
                else if (classifierData[4] != "" && classifierData[4] == "CONTRACTED")
                {
                    loadedClassifier.form = SkeletonForm.Extended;
                }

                loadedClassifier.formTrainingData = new List<TrainingData>();

                int trainingDataCount = Int32.Parse(classifierData[5]);

                for (int i = 0; i < trainingDataCount; ++i)
                {
                    TrainingData newEntry = loadTrainingData(upperName, i);
                    loadedClassifier.formTrainingData.Add(newEntry);
                }

                return loadedClassifier;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static bool deleteClassifier(Classifier inClassifier)
        {
            string upperName = inClassifier.name.ToUpper();

            if (upperName == "") return false;

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(classifierFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.Delete(filePathInSystem);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool deleteClassifier(string classifierName)
        {
            string upperName = classifierName.ToUpper();

            if (upperName == "") return false;

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(classifierFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.Delete(filePathInSystem);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        // Completely deletes Exercise(Exercise, Classifier, TrainingData
        private static bool deleteClassifierSet(Classifier inClassifier)
        {
            string upperName = inClassifier.name.ToUpper();

            if (upperName == "") return false;

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(classifierFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.Delete(filePathInSystem);

                cleanTrainingData(upperName);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion

        #region Training Data Functions
        public static bool saveTrainingData(TrainingData inTrainingData)
        {
            StringBuilder trainningDataData = new StringBuilder();

            // Process exercise data
            string upperName = inTrainingData.classifierName.ToUpper();

            if (upperName == "") return false;

            trainningDataData.Append(upperName);
            trainningDataData.Append(delimiter);

            trainningDataData.Append(inTrainingData.classifierIndex);
            trainningDataData.Append(delimiter);

            // Delete any previous versions
            deleteTrainingData(upperName + inTrainingData.classifierIndex);

            if (inTrainingData.formData.Length > 0)
            {
                trainningDataData.Append(inTrainingData.formData[0].ToString());

                for (int i = 1; i < inTrainingData.formData.Length; ++i)
                {
                    trainningDataData.Append(listDelimiter);
                    trainningDataData.Append(inTrainingData.formData[i].ToString());
                }
            }

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(trainingDataFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(inTrainingData.classifierIndex);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.WriteAllText(filePathInSystem, trainningDataData.ToString());
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static TrainingData loadTrainingData(string classifierName, int classifierIndex)
        {
            string upperName = classifierName.ToUpper();

            if (upperName == "") return null;

            StringBuilder filePath = new StringBuilder();
            filePath.Append(trainingDataFileNamePreposition);
            filePath.Append(upperName);
            filePath.Append(classifierIndex);
            filePath.Append(fileExtension);

            try
            {
                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                string trainingDataDataString = File.ReadAllText(filePathInSystem);

                string[] trainingDataData = trainingDataDataString.Trim().Split(delimiter);

                TrainingData loadedTrainingData = new TrainingData();

                // There should only be 3 components
                if (trainingDataData.Length != 6) return null;

                if (upperName != trainingDataData[0].ToUpper()) return null;

                loadedTrainingData.classifierName = trainingDataData[0];

                loadedTrainingData.classifierIndex = Int32.Parse(trainingDataData[1]);

                string[] formDataString = trainingDataData[2].Split(listDelimiter);

                if (formDataString.Length > 0 && formDataString[0] != "")
                {
                    loadedTrainingData.formData = new double[formDataString.Length];

                    for (int i = 0; i < loadedTrainingData.formData.Length; ++i)
                    {
                        loadedTrainingData.formData[i] = Double.Parse(formDataString[i]);
                    }
                }

                return loadedTrainingData;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static bool deleteTrainingData(TrainingData inTrainingData)
        {
            string upperName = inTrainingData.classifierName.ToUpper();

            if (upperName == "") return false;

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(trainingDataFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(inTrainingData.classifierIndex);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.Delete(filePathInSystem);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool deleteTrainingData(string classifierName, int classifierIndex)
        {
            string upperName = classifierName.ToUpper();

            if (upperName == "") return false;

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(trainingDataFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(classifierIndex);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.Delete(filePathInSystem);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        // Helper for cleanTrainingData
        private static bool deleteTrainingData(string trainingDataName)
        {
            string upperName = trainingDataName.ToUpper();

            if (upperName == "") return false;

            try
            {
                StringBuilder filePath = new StringBuilder();
                filePath.Append(trainingDataFileNamePreposition);
                filePath.Append(upperName);
                filePath.Append(fileExtension);

                string filePathInSystem = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\" + filePath.ToString());
                File.Delete(filePathInSystem);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        // Deletes ALL related TrainingData
        private static void cleanTrainingData(string classifierName)
        {
            DirectoryInfo exerciseDirectory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"exerciseData\"));
            StringBuilder filesName = new StringBuilder();
            filesName.Append(trainingDataFileNamePreposition);
            filesName.Append(classifierName);
            filesName.Append("*");
            filesName.Append(fileExtension);
            FileInfo[] trainingDataFiles = exerciseDirectory.GetFiles(filesName.ToString());

            foreach(FileInfo f in trainingDataFiles)
            {
                deleteTrainingData(f.Name);
            }
        }
        #endregion

        #region Craig Added Functions - To prettify later

        public static bool saveExerciseV2(Exercise inputExercise)
        {

            try
            {
                using (StreamWriter sw = new StreamWriter("../../exerciseData/" + inputExercise.name + ".txt"))
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
                                sw.Write(",");
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

        public static Exercise loadExerciseV2(string ExerciseName)
        {
            string filePathPrefix = "../../exerciseData/";
            string fileExtension = ".txt";
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
                    string[] lineDataValues = line.Split(',');
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

        #endregion


    }
}
