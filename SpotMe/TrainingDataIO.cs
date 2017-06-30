using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;

namespace SpotMe
{
    class TrainingDataIO
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
            var lineCount = File.ReadLines(fileName).Count();
            double[][] outData = new double[lineCount - 1][];
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

            return outData;
        }
    }
}
