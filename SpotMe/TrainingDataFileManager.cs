using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    static class TrainingDataFileManager
    {

        public static List<bodyDouble> loadBodyDoubleFromFile(string fileName)
        {
            List<bodyDouble> returnList = new List<bodyDouble>();

            double[][] fileOutput = TrainingDataIO.readTrainingData(fileName);

            // Return an empty list if there is no training data to be read
            if (fileOutput == null)
            {
                return returnList; 
            }

            for (int i = 0; i < fileOutput.Length; i++)
            {
                returnList.Add(SkeletonModifier.trainingDataTo3DSkeleton(fileOutput[i]));
            }

            return returnList;
        }

        public static List<bodyDouble> loadBodyDoubleFromFileWithClassifierIgnored(string fileName)
        {
            List<bodyDouble> returnList = new List<bodyDouble>();

            double[][] outputData;
            int[] outputClassifications;

            bool result = TrainingDataIO.readTrainingDataWithClassifiers(fileName,out outputData,out outputClassifications);

            // Return an empty list if there is no training data to be read
            if (outputData == null)
            {
                return returnList;
            }

            for (int i = 0; i < outputData.Length; i++)
            {
                returnList.Add(SkeletonModifier.trainingDataTo3DSkeleton(outputData[i]));
            }

            return returnList;
        }

        public static Exercise loadExerciseFromFile(string fileName)
        {
            Exercise returnExercise = new Exercise();

            double[][] outputData;
            int[] outputClassifications;

            bool result = TrainingDataIO.readTrainingDataWithClassifiers(fileName, out outputData, out outputClassifications);

            // Return an empty list if there is no training data to be read
            if (outputData == null)
            {
                return returnExercise;
            }

            return returnExercise;
        }

    }
}
