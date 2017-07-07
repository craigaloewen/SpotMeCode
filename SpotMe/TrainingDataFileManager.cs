﻿using System;
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

    }
}