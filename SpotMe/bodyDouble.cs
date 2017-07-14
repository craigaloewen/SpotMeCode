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

}
