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
    

    /// <summary>
    /// Static class containing functions to modify Body (Skeleton data) into different forms (most notably training data)
    /// </summary>
    static class SkeletonModifier
    {
        //------------------
        // PRIVATE FUNCTIONS
        //------------------

        private static void GetVector3Angles(out double angleYX, out double angleXZ, Vector3 inputVector)
        {
            angleYX = Math.Atan2(inputVector.Z, inputVector.X);
            angleXZ = Math.Atan2(inputVector.Y, Math.Sqrt(inputVector.X * inputVector.X + inputVector.Z * inputVector.Z));
        }

        private static Vector3 RotateVectorAroundY(double angleYX, Vector3 inputVector)
        {
            Matrix4x4 yRotMat = Matrix4x4.CreateRotationY((float)angleYX);
            Vector3 outputVector = Vector3.Transform(inputVector, yRotMat);

            return outputVector;
        }

        private static Vector3 RotateVectorAroundYthenZ(double angleYX, double angleXZ, Vector3 inputVector)
        {
            Matrix4x4 yRotMat = Matrix4x4.CreateRotationY((float)angleYX);
            Matrix4x4 zRotMat = Matrix4x4.CreateRotationZ(-(float)angleXZ);
            Vector3 outputVector = Vector3.Transform(inputVector, yRotMat);

            outputVector = Vector3.Transform(outputVector, zRotMat);

            return outputVector;
        }
        private static Vector3 RotateVectorAroundZthenY(double angleYX, double angleXZ, Vector3 inputVector)
        {
            Matrix4x4 yRotMat = Matrix4x4.CreateRotationY((float)angleYX);
            Matrix4x4 zRotMat = Matrix4x4.CreateRotationZ(-(float)angleXZ);
            Vector3 outputVector = Vector3.Transform(inputVector, zRotMat);

            outputVector = Vector3.Transform(outputVector, yRotMat);

            return outputVector;
        }
        private static Vector3 VectorizeTwoJoints(JointType jointA, JointType jointB, Body inBody)
        {
            Vector3 outputVector = new Vector3()
            {
                X = inBody.Joints[jointA].Position.X - inBody.Joints[jointB].Position.X,
                Y = inBody.Joints[jointA].Position.Y - inBody.Joints[jointB].Position.Y,
                Z = inBody.Joints[jointA].Position.Z - inBody.Joints[jointB].Position.Z
            };
            outputVector = Vector3.Normalize(outputVector);
            return outputVector;
        }
        private static Vector3 LocalCoordVector(Vector3 vectorToLocalize, Vector3 baseVector)
        {
            double angleToXYPlane, angleToXZPlane;

            GetVector3Angles(out angleToXYPlane, out angleToXZPlane, baseVector);

            return RotateVectorAroundYthenZ(angleToXYPlane, angleToXZPlane, vectorToLocalize);
        }

        private static double DotProductVectors(Vector3 vectorA, Vector3 vectorB)
        {
            double returnValue = (vectorA.X * vectorB.X + vectorA.Y * vectorB.Y + vectorA.Z * vectorB.Z);
            
            if (returnValue > 1)
            {
                returnValue = 1;
            }

            return returnValue;
        }

        private static double GetAngleBetweenVectors(Vector3 vectorA, Vector3 vectorB)
        {
            return Math.Acos(DotProductVectors(vectorA,vectorB));
        }

        private static double[] GetSkeletonAngles(double[] compareSkeleton, double[] acceptedSkeleton)
        {
            double[] vectorAngles = new double[compareSkeleton.Length / 3];

            Vector3 compareVector, acceptVector;

            for (int i = 0; i < vectorAngles.Length; i++)
            {
                compareVector = new Vector3((float)compareSkeleton[i * 3], (float)compareSkeleton[i * 3 + 1], (float)compareSkeleton[i * 3 + 2]);
                acceptVector = new Vector3((float)acceptedSkeleton[i * 3], (float)acceptedSkeleton[i * 3 + 1], (float)acceptedSkeleton[i * 3 + 2]);
                vectorAngles[i] = GetAngleBetweenVectors(compareVector, acceptVector);
            }

            return vectorAngles;
        }

        private static double[] RotateSkeletonData(double[] skeletonData, double bodyAngle)
        {
            List<Vector3> skeletonListVector = SkeletonDataToVectorList(skeletonData);

            skeletonListVector[(int)bodyDouble.bones.leftBicep] = RotateVectorAroundY(-bodyAngle, skeletonListVector[(int)bodyDouble.bones.leftBicep]);
            skeletonListVector[(int)bodyDouble.bones.rightBicep] = RotateVectorAroundY(-bodyAngle, skeletonListVector[(int)bodyDouble.bones.rightBicep]);
            skeletonListVector[(int)bodyDouble.bones.leftThigh] = RotateVectorAroundY(-bodyAngle, skeletonListVector[(int)bodyDouble.bones.leftThigh]);
            skeletonListVector[(int)bodyDouble.bones.rightThigh] = RotateVectorAroundY(-bodyAngle, skeletonListVector[(int)bodyDouble.bones.rightThigh]);

            return VectorListToSkeletonData(skeletonListVector);
        }

        private static List<Vector3> SkeletonDataToVectorList(double[] skeletonData)
        {
            List<Vector3> returnList = new List<Vector3>();

            for (int i = 0; i < skeletonData.Length; i += 3)
            {
                Vector3 addVector = new Vector3()
                {
                    X = (float)skeletonData[i],
                    Y = (float)skeletonData[i + 1],
                    Z = (float)skeletonData[i + 2]
                };
                returnList.Add(addVector);
            }

            return returnList;
        }

        private static double[] VectorListToSkeletonData(List<Vector3> inputList)
        {
            double[] outputData = new double[inputList.Count * 3];

            for (int i = 0; i < outputData.Length; i += 3)
            {
                outputData[i] = inputList[i / 3].X;
                outputData[i + 1] = inputList[i / 3].Y;
                outputData[i + 2] = inputList[i / 3].Z;
            }

            return outputData;
        }

        // ----------------
        // PUBLIC FUNCTIONS
        // ----------------

        /// <summary>
        /// Returns an approximate angle in radians of how the body is rotated
        /// </summary>
        /// <param name="inBody"></param>
        /// <returns></returns>
        public static double GetBodyAngle(Body inBody)
        {
            double angleYX, angleXZ;

            Vector3 rightShoulder = VectorizeTwoJoints(JointType.ShoulderRight, JointType.SpineShoulder, inBody);

            GetVector3Angles(out angleYX, out angleXZ, rightShoulder);

            return angleYX;
        }

        /// <summary>
        /// Outputs a vector that points in the direction needed to change the vector of the bone of compareSkeleton to the bone of acceptedSKeleton
        /// </summary>
        /// <param name="inBone">The bone to do the comparison on each skeleton</param>
        /// <param name="compareSkeleton">The base point skeleton</param>
        /// <param name="acceptedSkeleton">The skeleton that the direction will point towards</param>
        /// <returns></returns>
        public static Vector3 GetBoneCorrectionDirection(bodyDouble.bones inBone, double[] compareSkeleton, double[] acceptedSkeleton)
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

        /// <summary>
        /// Compare two machine learning data skeletons and output a list of the bones that differ by a certain angle
        /// </summary>
        /// <param name="compareSkeleton">The skeleton you wish to compare to a base</param>
        /// <param name="acceptedSkeleton">The accepted skeleton or base skeleton</param>
        /// <returns></returns>
        public static List<bodyDouble.bones> GetProblemBones(double[] compareSkeleton, double[] acceptedSkeleton)
        {
            List<bodyDouble.bones> returnList = new List<bodyDouble.bones>();

            double[] boneAngles = GetSkeletonAngles(compareSkeleton, acceptedSkeleton);

            for (int i = 0; i < boneAngles.Length; i++)
            {
                if (boneAngles[i] > 0.52) // 30 degs 
                {
                    returnList.Add((bodyDouble.bones)i);
                }
            }

            return returnList;
        }

        /// <summary>
        /// Compare two machine learning data skeletons and output a list of the bones that differ by a certain angle including the effects of body rotation
        /// </summary>
        /// <param name="compareSkeleton">The skeleton you wish to compare to a base</param>
        /// <param name="acceptedSkeleton">The accepted skeleton or base skeleton</param>
        /// <returns></returns>
        public static List<bodyDouble.bones> GetProblemBonesWithBodyRotation(double[] compareSkeleton, double[] acceptedSkeleton, double bodyAngle)
        {
            List<bodyDouble.bones> returnList = new List<bodyDouble.bones>();

            double[] rotatedCompareSkeleton = RotateSkeletonData(compareSkeleton, bodyAngle);

            RotateSkeletonData(rotatedCompareSkeleton, bodyAngle);

            double[] boneAngles = GetSkeletonAngles(compareSkeleton, acceptedSkeleton);

            for (int i = 0; i < boneAngles.Length; i++)
            {
                if (boneAngles[i] > 0.52) // 30 degs 
                {
                    returnList.Add((bodyDouble.bones)i);
                }
            }

            return returnList;
        }

        /// <summary>
        /// Takes in a bone and machine learning data and generates the 3D positions of the two limb joints for the limb that the bone belongs to
        /// </summary>
        /// <param name="acceptedSkeletonData">Machine learning data</param>
        /// <param name="inBone">The bone for the limb to display</param>
        /// <param name="basePoint">The point where the limb starts</param>
        /// <param name="limbPoint1">An output of the 3D Position of the first joint of the limb</param>
        /// <param name="limbPoint2">An output of the 3D Position of the second joint of the limb</param>
        /// <returns></returns>
        public static bool GenerateLimbPositionsFromBone(double[] acceptedSkeletonData, bodyDouble.bones inBone, Vector3 basePoint, double bodyAngle, out Vector3 limbPoint1, out Vector3 limbPoint2)
        {
            Vector3 limbPoint1Direction;
            Vector3 limbPoint2Direction;

            double angleYX, angleXZ;

            switch (inBone)
            {
                case bodyDouble.bones.leftBicep:
                case bodyDouble.bones.leftForearm:
                    limbPoint1Direction = new Vector3((float)acceptedSkeletonData[0], (float)acceptedSkeletonData[1], (float)acceptedSkeletonData[2]);
                    limbPoint2Direction = new Vector3((float)acceptedSkeletonData[3], (float)acceptedSkeletonData[4], (float)acceptedSkeletonData[5]);
                    
                    break;
                case bodyDouble.bones.rightBicep:
                case bodyDouble.bones.rightForearm:
                    limbPoint1Direction = new Vector3((float)acceptedSkeletonData[12], (float)acceptedSkeletonData[13], (float)acceptedSkeletonData[14]);
                    limbPoint2Direction = new Vector3((float)acceptedSkeletonData[15], (float)acceptedSkeletonData[16], (float)acceptedSkeletonData[17]);

                    break;
                default:
                    limbPoint1 = new Vector3();
                    limbPoint2 = new Vector3();
                    return false;
            }

            limbPoint1Direction = RotateVectorAroundY(-bodyAngle, limbPoint1Direction);
            GetVector3Angles(out angleYX, out angleXZ, limbPoint1Direction);
            limbPoint2Direction = RotateVectorAroundZthenY(-angleYX, -angleXZ, limbPoint2Direction);

            limbPoint1 = basePoint + ( limbPoint1Direction * ( (float) 0.3 ) );
            limbPoint2 = limbPoint1 + ( limbPoint2Direction * ( (float) 0.3 ) );

            return true;
        }

        /// <summary>
        /// Takes in data from a machine learning algorithm and outputs an approximation of the 3D skeleton for that data
        /// </summary>
        /// <param name="inputTrainingData">Input machine learning algorithm data</param>
        /// <returns>Outputted 3D skeleton</returns>
        public static bodyDouble TrainingDataTo3DSkeleton(double[] inputTrainingData)
        {
            bodyDouble returnBody = new bodyDouble();

            double angleYX, angleXZ;

            Vector3 leftBicepDirection = new Vector3((float)inputTrainingData[0], (float)inputTrainingData[1], (float)inputTrainingData[2]);
            GetVector3Angles(out angleYX, out angleXZ, leftBicepDirection);
            Vector3 leftForearmDirection = new Vector3((float)inputTrainingData[3], (float)inputTrainingData[4], (float)inputTrainingData[5]);
            leftForearmDirection = RotateVectorAroundZthenY(-angleYX, -angleXZ, leftForearmDirection);

            Vector3 leftThighDirection = new Vector3((float)inputTrainingData[6], (float)inputTrainingData[7], (float)inputTrainingData[8]);
            GetVector3Angles(out angleYX, out angleXZ, leftThighDirection);
            Vector3 leftShinDirection = new Vector3((float)inputTrainingData[9], (float)inputTrainingData[10], (float)inputTrainingData[11]);
            leftShinDirection = RotateVectorAroundZthenY(-angleYX, -angleXZ, leftShinDirection);

            Vector3 rightBicepDirection = new Vector3((float)inputTrainingData[12], (float)inputTrainingData[13], (float)inputTrainingData[14]);
            //rightBicepDirection = rotateVectorAroundZthenY(-3.14159, 0, rightBicepDirection);
            GetVector3Angles(out angleYX, out angleXZ, rightBicepDirection);
            Vector3 rightForearmDirection = new Vector3((float)inputTrainingData[15], (float)inputTrainingData[16], (float)inputTrainingData[17]);
            rightForearmDirection = RotateVectorAroundZthenY(-angleYX, -angleXZ, rightForearmDirection);

            Vector3 rightThighDirection = new Vector3((float)inputTrainingData[18], (float)inputTrainingData[19], (float)inputTrainingData[20]);
            //rightThighDirection = rotateVectorAroundZthenY(-3.14159, 0, rightThighDirection);
            GetVector3Angles(out angleYX, out angleXZ, rightThighDirection);
            Vector3 rightShinDirection= new Vector3((float)inputTrainingData[21], (float)inputTrainingData[22], (float)inputTrainingData[23]);
            rightShinDirection = RotateVectorAroundZthenY(-angleYX, -angleXZ, rightShinDirection);

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

        /// <summary>
        /// Changes skeleton data into an array of doubles that can be interpreted by a machine learning algorithm
        /// </summary>
        /// <param name="inBody">Input body to process</param>
        /// <returns>Outputted machine learning algorithm data</returns>
        public static double[] PreprocessSkeleton(Body inBody)
        {
            // Possibly do not local coordinate the bicep vectors based upon the shoulders
            // And instead consider the shoulders as unit vectors in the pos and nev
            // x directions and see if that improves recognition

            // SHould do some kind of processing to determine if the person is rotated and reject that

            Vector3 leftShoulder = VectorizeTwoJoints(JointType.ShoulderLeft, JointType.SpineShoulder, inBody);
            Vector3 rightShoulder = VectorizeTwoJoints(JointType.ShoulderRight, JointType.SpineShoulder, inBody);

            Vector3 leftBicep = VectorizeTwoJoints(JointType.ElbowLeft, JointType.ShoulderLeft, inBody);
            Vector3 rightBicep = VectorizeTwoJoints(JointType.ElbowRight, JointType.ShoulderRight, inBody);

            Vector3 leftForearm = VectorizeTwoJoints(JointType.WristLeft, JointType.ElbowLeft, inBody);
            Vector3 rightForearm = VectorizeTwoJoints(JointType.WristRight, JointType.ElbowRight, inBody);

            Vector3 leftHip = VectorizeTwoJoints(JointType.HipLeft, JointType.SpineBase, inBody);
            Vector3 rightHip = VectorizeTwoJoints(JointType.HipRight, JointType.SpineBase, inBody);

            Vector3 leftThigh = VectorizeTwoJoints(JointType.KneeLeft, JointType.HipLeft, inBody);
            Vector3 rightThigh = VectorizeTwoJoints(JointType.KneeRight, JointType.HipRight, inBody);

            Vector3 leftShin = VectorizeTwoJoints(JointType.AnkleLeft, JointType.KneeLeft, inBody);
            Vector3 rightShin = VectorizeTwoJoints(JointType.AnkleRight, JointType.KneeRight, inBody);

            Vector3 leftBicepRotated = LocalCoordVector(leftBicep, leftShoulder);
            Vector3 rightBicepRotated = LocalCoordVector(rightBicep, rightShoulder);

            Vector3 leftForearmRotated = LocalCoordVector(leftForearm, leftBicep);
            Vector3 rightForearmRotated = LocalCoordVector(rightForearm, rightBicep);

            Vector3 leftThighRotated = LocalCoordVector(leftThigh, leftHip);
            Vector3 rightThighRotated = LocalCoordVector(rightThigh, rightHip);

            Vector3 leftShinRotated = LocalCoordVector(leftShin, leftThigh);
            Vector3 rightShinRotated = LocalCoordVector(rightShin, rightThigh);

            List<Vector3> classificationDataList = new List<Vector3>
            {
                leftBicep, // Used to be rotated
                leftForearmRotated,
                leftThigh, // Used to be rotated
                leftShinRotated,

                rightBicep, // Used to be rotated
                rightForearmRotated,
                rightThigh, // Used to be rotated
                rightShinRotated
            };
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

    }
}
