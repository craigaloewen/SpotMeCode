using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;

namespace SpotMe
{
    /// <summary>
    /// A temporary class used to input and output training data. Will become obselete
    /// </summary>
    static class TrainingDataIO
    {
        public static bool saveTrainingData(double[][] inData, string fileName)
        {

            if (inData == null)
            {
                return false;
            }

            if (inData[0] == null)
            {
                return false;
            }

            using (var sw = new StreamWriter(fileName))
            {
                var writer = new CsvWriter(sw);

                for (int i = 0; i < inData[0].Length; i++)
                {
                    writer.WriteField(i);
                }
                writer.NextRecord();

                for (int i = 0; i < inData.Length; i++)
                {
                    for (int j = 0; j < inData[i].Length; j++)
                    {
                        writer.WriteField(inData[i][j]);
                    }
                    writer.NextRecord();
                }
            }

            return true;
        }

        public static double[][] readTrainingData(string fileName)
        {

            double[][] outData = null;

            try
            {
                var lineCount = File.ReadLines(fileName).Count();
                outData = new double[lineCount - 1][];
                lineCount = 0;

                using (var sr = new StreamReader(fileName))
                {
                    var reader = new CsvReader(sr);

                    while (reader.Read())
                    {
                        double[] tempData = new double[reader.FieldHeaders.Length];

                        for (int i = 0; i < tempData.Length; i++)
                        {
                            tempData[i] = reader.GetField<double>(i);
                        }
                        outData[lineCount++] = tempData;
                    }
                }
            }
            catch
            {
                return outData;
            }

            return outData;
        }

        public static bool saveTrainingDataWithClassifiers(double[][] inData, int[] classifierData, string fileName)
        {

            if (inData == null)
            {
                return false;
            }

            if (inData[0] == null)
            {
                return false;
            }

            using (var sw = new StreamWriter(fileName))
            {
                var writer = new CsvWriter(sw);

                writer.WriteField("Classifier");
                for (int i = 0; i < inData[0].Length; i++)
                {
                    writer.WriteField(i);
                }
                writer.NextRecord();

                for (int i = 0; i < inData.Length; i++)
                {
                    writer.WriteField(classifierData[i]);
                    for (int j = 0; j < inData[i].Length; j++)
                    {
                        writer.WriteField(inData[i][j]);
                    }
                    writer.NextRecord();
                }
            }

            return true;
        }

        public static bool readTrainingDataWithClassifiers(string fileName, out double[][] skeletonData, out int[] classificationData)
        {

            skeletonData = null;
            classificationData = null;

            try
            {
                var lineCount = File.ReadLines(fileName).Count();
                skeletonData = new double[lineCount - 1][];
                classificationData = new int[lineCount - 1];
                lineCount = 0;

                using (var sr = new StreamReader(fileName))
                {
                    var reader = new CsvReader(sr);

                    while (reader.Read())
                    {
                        double[] tempData = new double[reader.FieldHeaders.Length-1];

                        classificationData[lineCount] = reader.GetField<int>(0);

                        for (int i = 1; i < reader.FieldHeaders.Length; i++)
                        {
                            tempData[i-1] = reader.GetField<double>(i);
                        }
                        skeletonData[lineCount++] = tempData;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
