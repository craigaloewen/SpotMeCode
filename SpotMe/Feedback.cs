using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace SpotMe
{
    /// <summary>
    /// A class to store feedback data (such as specific skeletons for each rep)
    /// </summary>
    class Feedback
    {
        private List<Body> repSkeletonList;

        public bool SaveRepSkeleton(Body inBody)
        {
            repSkeletonList.Add(inBody);
            return true;
        }

        public IReadOnlyList<Body> GetRepSkeletonList()
        {
            return repSkeletonList.AsReadOnly();
        }

        public bool ClearRepSkeletonList()
        {
            repSkeletonList = new List<Body>();
            return true;
        }

    }
}
