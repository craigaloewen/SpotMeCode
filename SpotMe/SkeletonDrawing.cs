using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media;

namespace SpotMe
{
    class SkeletonDrawing
    {

        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double HandSize = 30;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as closed
        /// </summary>
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as opened
        /// </summary>
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        /// </summary>
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// definition of bones
        /// </summary>
        private List<Tuple<JointType, JointType>> bones;

        /// <summary>
        /// List of colors for each body tracked
        /// </summary>
        public List<Pen> bodyColors;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        public SkeletonDrawing(CoordinateMapper inputCoordinateMapper)
        {
            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            // populate body colors, one for each BodyIndex
            this.bodyColors = new List<Pen>();

            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));

            coordinateMapper = inputCoordinateMapper;

        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        public void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }

        public void DrawTrainingDataOuput(DrawingContext drawingContext, bodyDouble inBodyDouble, Pen drawingPen)
        {
            Dictionary<bodyDouble.joints, Point> jointPoints = new Dictionary<bodyDouble.joints, Point>();

            foreach (KeyValuePair<bodyDouble.joints, System.Numerics.Vector3> someVectorPair in inBodyDouble.jointList)
            {
                CameraSpacePoint jointCamSpacePoint = new CameraSpacePoint();

                jointCamSpacePoint.X = someVectorPair.Value.X;
                jointCamSpacePoint.Y = someVectorPair.Value.Y;
                jointCamSpacePoint.Z = someVectorPair.Value.Z;

                ColorSpacePoint colorSpacePoint = this.coordinateMapper.MapCameraPointToColorSpace(jointCamSpacePoint);
                jointPoints[someVectorPair.Key] = new Point(colorSpacePoint.X, colorSpacePoint.Y);
            }

            foreach (bodyDouble.joints jointType in jointPoints.Keys)
            {
                drawingContext.DrawEllipse(this.trackedJointBrush, null, jointPoints[jointType], JointThickness, JointThickness);
            }

            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.spineShoulder], jointPoints[bodyDouble.joints.leftShoulder]);
            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.leftShoulder], jointPoints[bodyDouble.joints.leftElbow]);
            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.leftElbow], jointPoints[bodyDouble.joints.leftWrist]);

            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.spineShoulder], jointPoints[bodyDouble.joints.rightShoulder]);
            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.rightShoulder], jointPoints[bodyDouble.joints.rightElbow]);
            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.rightElbow], jointPoints[bodyDouble.joints.rightWrist]);

            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.spineBase], jointPoints[bodyDouble.joints.leftHip]);
            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.leftHip], jointPoints[bodyDouble.joints.leftKnee]);
            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.leftKnee], jointPoints[bodyDouble.joints.leftAnkle]);

            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.spineBase], jointPoints[bodyDouble.joints.rightHip]);
            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.rightHip], jointPoints[bodyDouble.joints.rightKnee]);
            drawingContext.DrawLine(drawingPen, jointPoints[bodyDouble.joints.rightKnee], jointPoints[bodyDouble.joints.rightAnkle]);
        }

        public void DrawFormCorrection(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, Body inBody, double[] acceptedForm, DrawingContext drawingContext, Pen drawingPen)
        {

            double[] inputData = SkeletonModifier.preprocessSkeleton(inBody);
            List<bodyDouble.bones> results = SkeletonModifier.getProblemBones(inputData, acceptedForm);

            // Scrub the data so it doesn't draw twice. This isn't the best solution but leaving it in here in case this feature doesn't pan out

            if (results.Contains(bodyDouble.bones.leftBicep) && results.Contains(bodyDouble.bones.leftForearm))
            {
                results.Remove(bodyDouble.bones.leftForearm);
            }

            if (results.Contains(bodyDouble.bones.rightBicep) && results.Contains(bodyDouble.bones.rightForearm))
            {
                results.Remove(bodyDouble.bones.rightForearm);
            }

            foreach (bodyDouble.bones problemBone in results)
            {
                DrawCorrectedLimb(joints, jointPoints, problemBone, acceptedForm, drawingContext, drawingPen);
            }
        }

        public void DrawCorrectedLimb(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, bodyDouble.bones inBone, double[] acceptedForm, DrawingContext drawingContext, Pen drawingPen)
        {

            Vector3 baseJoint = new Vector3();
            Vector3 limbJoint1;
            Vector3 limbJoint2;

            CameraSpacePoint jointCamSpacePoint;
            ColorSpacePoint colorSpacePoint;
            Point limbJoint1Point;
            Point limbJoint2Point;

            JointType baseJointType;
            JointType currentPositionJoint;

            switch (inBone)
            {
                case bodyDouble.bones.leftBicep:
                case bodyDouble.bones.leftForearm:

                    baseJointType = JointType.ShoulderLeft;
                    currentPositionJoint = JointType.WristLeft;


                    break;
                case bodyDouble.bones.rightBicep:
                case bodyDouble.bones.rightForearm:

                    baseJointType = JointType.ShoulderRight;
                    currentPositionJoint = JointType.WristRight;

                    break;
                default:
                    return;
            }

            // Base Joint
            baseJoint.X = joints[baseJointType].Position.X;
            baseJoint.Y = joints[baseJointType].Position.Y;
            baseJoint.Z = joints[baseJointType].Position.Z;

            SkeletonModifier.generateLimbPositionsFromBone(acceptedForm, inBone, baseJoint, out limbJoint1, out limbJoint2);

            // Joint 1
            jointCamSpacePoint = new CameraSpacePoint();

            jointCamSpacePoint.X = limbJoint1.X;
            jointCamSpacePoint.Y = limbJoint1.Y;
            jointCamSpacePoint.Z = limbJoint1.Z;

            colorSpacePoint = this.coordinateMapper.MapCameraPointToColorSpace(jointCamSpacePoint);
            limbJoint1Point = new Point(colorSpacePoint.X, colorSpacePoint.Y);

            // Joint 2
            jointCamSpacePoint.X = limbJoint2.X;
            jointCamSpacePoint.Y = limbJoint2.Y;
            jointCamSpacePoint.Z = limbJoint2.Z;

            colorSpacePoint = this.coordinateMapper.MapCameraPointToColorSpace(jointCamSpacePoint);
            limbJoint2Point = new Point(colorSpacePoint.X, colorSpacePoint.Y);

            // Draw joints
            drawingContext.DrawLine(this.bodyColors[0], jointPoints[baseJointType], limbJoint1Point);
            drawingContext.DrawLine(this.bodyColors[0], limbJoint1Point, limbJoint2Point);

            // Draw line back to good form
            drawingContext.DrawLine(this.bodyColors[1], jointPoints[currentPositionJoint], limbJoint2Point);

        }

        /// <summary>
        /// Draws one bone of a body (joint to joint)
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="jointType0">first joint of bone to draw</param>
        /// <param name="jointType1">second joint of bone to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        public void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /// <summary>
        /// Draws a hand symbol if the hand is tracked: red circle = closed, green circle = opened; blue circle = lasso
        /// </summary>
        /// <param name="handState">state of the hand</param>
        /// <param name="handPosition">position of the hand</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        public void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;
            }
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        public void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }


    }
}
