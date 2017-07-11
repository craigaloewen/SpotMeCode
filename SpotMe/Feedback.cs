﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace SpotMe
{
    class Feedback
    {
        private List<Body> repSkeletonList;

        public bool saveRepSkeleton(Body inBody)
        {
            repSkeletonList.Add(inBody);
            return true;
        }

        public IReadOnlyList<Body> getRepSkeletonList()
        {
            return repSkeletonList.AsReadOnly();
        }

        public bool clearRepSkeletonList()
        {
            repSkeletonList = new List<Body>();
            return true;
        }

    }
}