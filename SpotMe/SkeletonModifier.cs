using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Numerics;
using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.MachineLearning.VectorMachines;

namespace SpotMe
{
    public class bodyDouble
    {
        public enum joints
        {
            spineShoulder,
            leftShoulder,
            leftElbow,
            leftWrist,
            rightShoulder,
            rightElbow,
            rightWrist,
            spineBase,
            leftHip,
            leftKnee,
            leftAnkle,
            rightHip,
            rightKnee,
            rightAnkle
        };

        // Ordered to reflect the order that it is put into the classification data (do not change the order around!)
        public enum bones
        {
            leftBicep,
            leftForearm,
            leftThigh,
            leftShin,
            rightBicep,
            rightForearm,
            rightThigh,
            rightShin
        };

        public Dictionary<joints, Vector3> jointList = new Dictionary<joints, Vector3>();
    }
    static class SkeletonModifier
    {
        //------------------
        // PRIVATE FUNCTIONS
        //------------------

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
        private static Vector3 rotateVectorAroundZthenY(double angleYX, double angleXZ, Vector3 inputVector)
        {
            Matrix4x4 yRotMat = Matrix4x4.CreateRotationY((float)angleYX);
            Matrix4x4 zRotMat = Matrix4x4.CreateRotationZ(-(float)angleXZ);
            Vector3 outputVector = Vector3.Transform(inputVector, zRotMat);

            outputVector = Vector3.Transform(outputVector, yRotMat);

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

        private static double dotProductVectors(Vector3 vectorA, Vector3 vectorB)
        {
            double returnValue = (vectorA.X * vectorB.X + vectorA.Y * vectorB.Y + vectorA.Z * vectorB.Z);
            
            if (returnValue > 1)
            {
                returnValue = 1;
            }

            return returnValue;
        }

        private static double getAngleBetweenVectors(Vector3 vectorA, Vector3 vectorB)
        {
            return Math.Acos(dotProductVectors(vectorA,vectorB));
        }
        private static double[] getSkeletonAngles(double[] compareSkeleton, double[] acceptedSkeleton)
        {
            double[] vectorAngles = new double[compareSkeleton.Length / 3];

            Vector3 compareVector, acceptVector;

            for (int i = 0; i < vectorAngles.Length; i++)
            {
                compareVector = new Vector3((float)compareSkeleton[i * 3], (float)compareSkeleton[i * 3 + 1], (float)compareSkeleton[i * 3 + 2]);
                acceptVector = new Vector3((float)acceptedSkeleton[i * 3], (float)acceptedSkeleton[i * 3 + 1], (float)acceptedSkeleton[i * 3 + 2]);
                vectorAngles[i] = getAngleBetweenVectors(compareVector, acceptVector);
            }

            return vectorAngles;
        }

        // ----------------
        // PUBLIC FUNCTIONS
        // ----------------

        public static Vector3 getBoneCorrectionDirection(bodyDouble.bones inBone, double[] compareSkeleton, double[] acceptedSkeleton)
        {
            Vector3 returnVector, compareVector, acceptedVector;
            int boneIndex = (int)inBone;
            boneIndex *= 3; // To account for X Y and Z

            compareVector = new Vector3((float)compareSkeleton[boneIndex], (float)compareSkeleton[boneIndex + 1], (float)compareSkeleton[boneIndex + 2]);
            acceptedVector = new Vector3((float)acceptedSkeleton[boneIndex], (float)acceptedSkeleton[boneIndex + 1], (float)acceptedSkeleton[boneIndex + 2]);

            returnVector = acceptedVector - compareVector;
            returnVector = Vector3.Normalize(returnVector);

            return returnVector;
        }

        public static List<bodyDouble.bones> getProblemBones(double[] compareSkeleton, double[] acceptedSkeleton)
        {
            List<bodyDouble.bones> returnList = new List<bodyDouble.bones>();

            double[] boneAngles = getSkeletonAngles(compareSkeleton, acceptedSkeleton);

            for (int i = 0; i < boneAngles.Length; i++)
            {
                if (boneAngles[i] > 0.52) // 30 degs 
                {
                    returnList.Add((bodyDouble.bones)i);
                }
            }

            return returnList;
        }

        public static bodyDouble trainingDataTo3DSkeleton(double[] inputTrainingData)
        {
            bodyDouble returnBody = new bodyDouble();

            double angleYX, angleXZ;

            Vector3 leftBicepDirection = new Vector3((float)inputTrainingData[0], (float)inputTrainingData[1], (float)inputTrainingData[2]);
            getVector3Angles(out angleYX, out angleXZ, leftBicepDirection);
            Vector3 leftForearmDirection = new Vector3((float)inputTrainingData[3], (float)inputTrainingData[4], (float)inputTrainingData[5]);
            leftForearmDirection = rotateVectorAroundZthenY(-angleYX, -angleXZ, leftForearmDirection);

            Vector3 leftThighDirection = new Vector3((float)inputTrainingData[6], (float)inputTrainingData[7], (float)inputTrainingData[8]);
            getVector3Angles(out angleYX, out angleXZ, leftThighDirection);
            Vector3 leftShinDirection = new Vector3((float)inputTrainingData[9], (float)inputTrainingData[10], (float)inputTrainingData[11]);
            leftShinDirection = rotateVectorAroundZthenY(-angleYX, -angleXZ, leftShinDirection);

            Vector3 rightBicepDirection = new Vector3((float)inputTrainingData[12], (float)inputTrainingData[13], (float)inputTrainingData[14]);
            //rightBicepDirection = rotateVectorAroundZthenY(-3.14159, 0, rightBicepDirection);
            getVector3Angles(out angleYX, out angleXZ, rightBicepDirection);
            Vector3 rightForearmDirection = new Vector3((float)inputTrainingData[15], (float)inputTrainingData[16], (float)inputTrainingData[17]);
            rightForearmDirection = rotateVectorAroundZthenY(-angleYX, -angleXZ, rightForearmDirection);

            Vector3 rightThighDirection = new Vector3((float)inputTrainingData[18], (float)inputTrainingData[19], (float)inputTrainingData[20]);
            //rightThighDirection = rotateVectorAroundZthenY(-3.14159, 0, rightThighDirection);
            getVector3Angles(out angleYX, out angleXZ, rightThighDirection);
            Vector3 rightShinDirection= new Vector3((float)inputTrainingData[21], (float)inputTrainingData[22], (float)inputTrainingData[23]);
            rightShinDirection = rotateVectorAroundZthenY(-angleYX, -angleXZ, rightShinDirection);

            returnBody.jointList[bodyDouble.joints.spineShoulder] = new Vector3((float)-0.5, (float)0.5, 2);
            returnBody.jointList[bodyDouble.joints.spineBase] = returnBody.jointList[bodyDouble.joints.spineShoulder] + (new Vector3(0, (float)-0.5, 0));

            returnBody.jointList[bodyDouble.joints.leftShoulder] = returnBody.jointList[bodyDouble.joints.spineShoulder] + (new Vector3((float)-0.3, 0, 0));
            returnBody.jointList[bodyDouble.joints.leftElbow] = returnBody.jointList[bodyDouble.joints.leftShoulder] + (leftBicepDirection * (float)0.3);
            returnBody.jointList[bodyDouble.joints.leftWrist] = returnBody.jointList[bodyDouble.joints.leftElbow] + (leftForearmDirection * (float)0.3);

            returnBody.jointList[bodyDouble.joints.rightShoulder] = returnBody.jointList[bodyDouble.joints.spineShoulder] + (new Vector3((float)0.3, 0, 0));
            returnBody.jointList[bodyDouble.joints.rightElbow] = returnBody.jointList[bodyDouble.joints.rightShoulder] + (rightBicepDirection * (float)0.3);
            returnBody.jointList[bodyDouble.joints.rightWrist] = returnBody.jointList[bodyDouble.joints.rightElbow] + (rightForearmDirection * (float)0.3);

            returnBody.jointList[bodyDouble.joints.leftHip] = returnBody.jointList[bodyDouble.joints.spineBase] + (new Vector3((float)-0.3, 0, 0));
            returnBody.jointList[bodyDouble.joints.leftKnee] = returnBody.jointList[bodyDouble.joints.leftHip] + (leftThighDirection * (float)0.3);
            returnBody.jointList[bodyDouble.joints.leftAnkle] = returnBody.jointList[bodyDouble.joints.leftKnee] + (leftShinDirection * (float)0.3);

            returnBody.jointList[bodyDouble.joints.rightHip] = returnBody.jointList[bodyDouble.joints.spineBase] + (new Vector3((float)0.3, 0, 0));
            returnBody.jointList[bodyDouble.joints.rightKnee] = returnBody.jointList[bodyDouble.joints.rightHip] + (rightThighDirection * (float)0.3);
            returnBody.jointList[bodyDouble.joints.rightAnkle] = returnBody.jointList[bodyDouble.joints.rightKnee] + (rightShinDirection * (float)0.3);

            //Build the skeleton from here

            return returnBody;
        }
        public static double[] preprocessSkeleton(Body inBody)
        {
            // Possibly do not local coordinate the bicep vectors based upon the shoulders
            // And instead consider the shoulders as unit vectors in the pos and nev
            // x directions and see if that improves recognition

            Vector3 leftShoulder = vectorizeTwoJoints(JointType.ShoulderLeft, JointType.SpineShoulder, inBody);
            Vector3 rightShoulder = vectorizeTwoJoints(JointType.ShoulderRight, JointType.SpineShoulder, inBody);

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
            classificationDataList.Add(leftBicep); // Used to be rotated
            classificationDataList.Add(leftForearmRotated);
            classificationDataList.Add(leftThigh); // Used to be rotated
            classificationDataList.Add(leftShinRotated);

            classificationDataList.Add(rightBicep); // Used to be rotated
            classificationDataList.Add(rightForearmRotated);
            classificationDataList.Add(rightThigh); // Used to be rotated
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
            trainingDataTo3DSkeleton(preprocessSkeleton(inBody));
        }

    }
}
