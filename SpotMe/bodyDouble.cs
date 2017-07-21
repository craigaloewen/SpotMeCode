using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SpotMe
{
    /// <summary>
    /// Emulates the 3D positions of a skeleton like a Body class, but is created from training data used for translating training data to a 3D skeleton
    /// </summary>
    public class bodyDouble
    {
        public enum joints
        {
            ShoulderLeft,
            ElbowLeft,
            WristLeft,
            HipLeft,
            KneeLeft,
            AnkleLeft,
            ShoulderRight,
            ElbowRight,
            WristRight,
            HipRight,
            KneeRight,
            AnkleRight,

            // All ones that aren't stored in the data
            SpineBase,
            SpineShoulder,
            Neck,
            Head
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
            rightShin,

            // All the ones that aren't included
            leftShoulder,
            rightShoulder,
            leftHip,
            rightHip,
            shoulderToBase,
            neck,
            head
        };

        public static Dictionary<bones, Tuple<joints, joints>> bonesToJoints = new Dictionary<bones, Tuple<joints, joints>>()
        {
            {bones.leftBicep, new Tuple<joints,joints>(joints.ShoulderLeft,joints.ElbowLeft) },
            {bones.leftForearm, new Tuple<joints,joints>(joints.ElbowLeft,joints.WristLeft) },
            {bones.leftThigh, new Tuple<joints,joints>(joints.HipLeft,joints.KneeLeft) },
            {bones.leftShin, new Tuple<joints,joints>(joints.KneeLeft,joints.AnkleLeft) },
            {bones.rightBicep, new Tuple<joints,joints>(joints.ShoulderRight,joints.ElbowRight) },
            {bones.rightForearm, new Tuple<joints,joints>(joints.ElbowRight,joints.WristRight) },
            {bones.rightThigh, new Tuple<joints,joints>(joints.HipRight,joints.KneeRight) },
            {bones.rightShin, new Tuple<joints,joints>(joints.KneeRight,joints.AnkleRight) },

            {bones.leftShoulder, new Tuple<joints,joints>(joints.SpineShoulder,joints.ShoulderLeft) },
            {bones.rightShoulder, new Tuple<joints,joints>(joints.SpineShoulder,joints.ShoulderRight) },
            {bones.leftHip, new Tuple<joints,joints>(joints.SpineBase,joints.HipLeft) },
            {bones.rightHip, new Tuple<joints,joints>(joints.SpineBase,joints.HipRight) },
            {bones.shoulderToBase, new Tuple<joints,joints>(joints.SpineShoulder,joints.SpineBase) },

            {bones.neck, new Tuple<joints,joints>(joints.SpineShoulder,joints.Neck) },
            {bones.head, new Tuple<joints,joints>(joints.Neck,joints.Head) }
        };

        public Dictionary<joints, Vector3> jointList = new Dictionary<joints, Vector3>();
    }

}
