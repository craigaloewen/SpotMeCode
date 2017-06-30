using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Numerics;
using Accord.MachineLearning;

namespace SpotMe
{
    static class SkeletonModifier
    {

        private static int testNum = 0;
        private static double[][] testArray = new double[3][];

        private static KMeans kmeansAlg;
        private static KMeansClusterCollection clusters;
        private static void getVector3Angles(out double angleYX, out double angleXZ, Vector3 inputVector)
        {
            angleYX = Math.Atan2(inputVector.Z, inputVector.X);
            angleXZ = Math.Atan2(inputVector.Y, Math.Sqrt(inputVector.X * inputVector.X + inputVector.Z * inputVector.Z));
        }
        private static Vector3 rotateVectorAroundYthenZ(double angleYX, double angleXZ, Vector3 inputVector)
        {
            Matrix4x4 yRotMat = Matrix4x4.CreateRotationY((float)angleYX);
            Matrix4x4 zRotMat = Matrix4x4.CreateRotationZ(-(float)angleXZ);
            Vector3 outputVector = Vector3.Transform(inputVector, yRotMat);

            outputVector = Vector3.Transform(outputVector, zRotMat);

            return outputVector;
        }
        private static Vector3 vectorizeTwoJoints(JointType jointA, JointType jointB, Body inBody)
        {
            Vector3 outputVector = new Vector3();
            outputVector.X = inBody.Joints[jointA].Position.X - inBody.Joints[jointB].Position.X;
            outputVector.Y = inBody.Joints[jointA].Position.Y - inBody.Joints[jointB].Position.Y;
            outputVector.Z = inBody.Joints[jointA].Position.Z - inBody.Joints[jointB].Position.Z;
            outputVector = Vector3.Normalize(outputVector);
            return outputVector;
        }
        private static Vector3 localCoordVector(Vector3 vectorToLocalize, Vector3 baseVector)
        {
            double angleToXYPlane, angleToXZPlane;
            getVector3Angles(out angleToXYPlane, out angleToXZPlane, baseVector);

            return rotateVectorAroundYthenZ(angleToXYPlane, angleToXZPlane, vectorToLocalize);
        }
        public static double[][] trainingDataTo3DSkeleton()
        {
            Vector3 spineShoulder = new Vector3(0, 0, 2);
            Vector3 shoulderLeft = spineShoulder + (new Vector3((float)-0.3, 0, 0));

            //Build the skeleton from here

            return null;
        }
        public static void firstRun()
        {
            kmeansAlg = new KMeans(3);
        }
        public static double[] preprocessSkeleton(Body inBody)
        {
            Console.WriteLine("Test");
            Vector3 leftShoulder = vectorizeTwoJoints(JointType.ShoulderLeft, JointType.SpineShoulder, inBody);
            Vector3 rightShoulder = vectorizeTwoJoints(JointType.ShoulderLeft, JointType.SpineShoulder, inBody);

            Vector3 leftBicep = vectorizeTwoJoints(JointType.ElbowLeft, JointType.ShoulderLeft, inBody);
            Vector3 rightBicep = vectorizeTwoJoints(JointType.ElbowRight, JointType.ShoulderRight, inBody);

            Vector3 leftForearm = vectorizeTwoJoints(JointType.WristLeft, JointType.ElbowLeft, inBody);
            Vector3 rightForearm = vectorizeTwoJoints(JointType.WristRight, JointType.ElbowRight, inBody);

            Vector3 leftHip = vectorizeTwoJoints(JointType.HipLeft, JointType.SpineBase, inBody);
            Vector3 rightHip = vectorizeTwoJoints(JointType.HipRight, JointType.SpineBase, inBody);

            Vector3 leftThigh = vectorizeTwoJoints(JointType.KneeLeft, JointType.HipLeft, inBody);
            Vector3 rightThigh = vectorizeTwoJoints(JointType.KneeRight, JointType.HipRight, inBody);

            Vector3 leftShin = vectorizeTwoJoints(JointType.AnkleLeft, JointType.KneeLeft, inBody);
            Vector3 rightShin = vectorizeTwoJoints(JointType.AnkleRight, JointType.KneeRight, inBody);

            Vector3 leftBicepRotated = localCoordVector(leftBicep, leftShoulder);
            Vector3 rightBicepRotated = localCoordVector(rightBicep, rightShoulder);

            Vector3 leftForearmRotated = localCoordVector(leftForearm, leftBicep);
            Vector3 rightForearmRotated = localCoordVector(rightForearm, rightBicep);

            Vector3 leftThighRotated = localCoordVector(leftThigh, leftHip);
            Vector3 rightThighRotated = localCoordVector(rightThigh, rightHip);

            Vector3 leftShinRotated = localCoordVector(leftShin, leftThigh);
            Vector3 rightShinRotated = localCoordVector(rightShin, rightThigh);

            List<Vector3> classificationDataList = new List<Vector3>();
            classificationDataList.Add(leftBicepRotated);
            classificationDataList.Add(leftForearmRotated);
            classificationDataList.Add(leftThighRotated);
            classificationDataList.Add(leftShinRotated);

            classificationDataList.Add(rightBicepRotated);
            classificationDataList.Add(rightForearmRotated);
            classificationDataList.Add(rightThighRotated);
            classificationDataList.Add(rightShinRotated);

            double[] classificationDataArray = new double[24];
            int arrayCounter = 0;
            foreach (Vector3 visitorVector in classificationDataList)
            {
                classificationDataArray[arrayCounter++] = visitorVector.X;
                classificationDataArray[arrayCounter++] = visitorVector.Y;
                classificationDataArray[arrayCounter++] = visitorVector.Z;
            }

            return classificationDataArray;
        }

        public static void debugMethod(Body inBody)
        {
            trainingDataTo3DSkeleton();
        }

    }
}
