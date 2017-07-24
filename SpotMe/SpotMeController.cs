using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpotMe
{
    class SpotMeController
    {
        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage bodyFrameSource;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage colorFrameSource;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        public KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        public WriteableBitmap colorBitmap = null;

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup bodyFrameDrawingGroup;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// The class used to draw all of the skeletons
        /// </summary>
        public SkeletonDrawing skeletonDrawingController;

        /// <summary>
        /// The Machine Learning algorithm variable
        /// </summary>
        public SpotMeML machineLearningAlg;

        private Exercise currentExercise;

        public ImageSource frameImageSource
        {
            get
            {
                return bodyFrameSource;
            }
        }

        public string outputMessage
        {
            get;
            private set;
        }

        public enum ControllerMode
        {
            Continuous,
            Set,
            View,
            Record
        };

        public Body lastRecordedSkeleton;

        public List<bodyDouble> storedSkeletons;
        public int skeletonViewIndex { get; private set; }

        public ControllerMode currentMode;

        // This is very wasteful, consider adding another integer that is only changed when the list is changed
        public int totalRecordedSkeletons { get; private set; }

        private ulong prevTrackingID;


        public SpotMeController()
        {
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;

            // get size of joint space
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            // open the sensor
            this.kinectSensor.Open();

            // Create the drawing group we'll use for drawing
            this.bodyFrameDrawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.bodyFrameSource = new DrawingImage(this.bodyFrameDrawingGroup);

            // --- End of Kinect set up

            outputMessage = "Startup";

            // Initialize the ML aspect
            machineLearningAlg = new SpotMe.SpotMeML();

            // Initializes the drawing component
            skeletonDrawingController = new SkeletonDrawing(coordinateMapper);

            storedSkeletons = new List<bodyDouble>();
            skeletonViewIndex = 0;
            totalRecordedSkeletons = 0;

            prevTrackingID = 0;

            // Start off in continuous mode
            SwitchMode(ControllerMode.Continuous);

        }

        public bool Init(string inputExercise)
        {
            outputMessage = "Initialized";

            LoadExercise(inputExercise);

            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
            }

            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
            }

            return true;
        }

        public bool CleanUp()
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
            return true;
        }

        public bool LoadExercise(string inName)
        {
            currentExercise = ExerciseManager.LoadExercise(inName);
            if (currentMode != ControllerMode.Record)
            {
                machineLearningAlg.init(inName);

            }
            return true;
        }

        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {

            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            Body body;

            if (prevTrackingID == 0)
            {
                body = (from s in bodies
                        where s.IsTracked == true
                        select s).FirstOrDefault();

                if (body != null)
                {
                    prevTrackingID = body.TrackingId;
                }
            } else
            {
                body = (from s in bodies
                        where s.TrackingId == prevTrackingID
                        select s).FirstOrDefault();

                if (body == null)
                {
                    prevTrackingID = 0;
                }
            }

            if (dataReceived)
            {
                if (currentMode == ControllerMode.Continuous)
                {
                    ContinuousModeFrameArrived(body);
                } else if (currentMode == ControllerMode.Set)
                {
                    SetModeFrameArrived(body);
                } else if (currentMode == ControllerMode.Record)
                {
                    RecordModeFrameArrived(body);
                }
            }
        }

        public bool SwitchMode(ControllerMode inputMode)
        {
            /*
            using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
            }
            */

            using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
            {
                dc.DrawImage(colorBitmap, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
            }
            currentMode = inputMode;

            if (currentMode == ControllerMode.View)
            {
                LoadSkeletonView(0);
            }

            return true;
        }

        public void LoadNextSkeleton()
        {
            if ((skeletonViewIndex + 1) < totalRecordedSkeletons)
            {
                LoadSkeletonView(++skeletonViewIndex);
            }
        }

        public void LoadPrevSkeleton()
        {
            if (skeletonViewIndex > 0)
            {
                LoadSkeletonView(--skeletonViewIndex);
            }
        }

        public void ClearSkeletonList()
        {
            skeletonViewIndex = 0;
            totalRecordedSkeletons = 0;
            storedSkeletons.Clear();
        }

        private void LoadSkeletonView(int indexNum)
        {
            skeletonViewIndex = indexNum;
            LoadSkeletonView();
        }

        private void LoadSkeletonView()
        {
            if (totalRecordedSkeletons < 1)
            {
                return;
            }

            if (skeletonViewIndex >= totalRecordedSkeletons)
            {
                return;
            }

            using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                Pen drawPen = skeletonDrawingController.bodyColors[0];

                bodyDouble body = storedSkeletons[skeletonViewIndex];

                skeletonDrawingController.DrawTrainingDataOuput(dc,body,drawPen);
            }
        }

        private void ContinuousModeFrameArrived(Body body)
        {
            using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                //dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                // Draw the color image onto the frame
                dc.DrawImage(colorBitmap, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                int penIndex = 0;
                Pen drawPen = skeletonDrawingController.bodyColors[penIndex++];

                if (body != null && currentExercise != null)
                {

                    lastRecordedSkeleton = body;

                    skeletonDrawingController.DrawClippedEdges(body, dc);

                    IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                    // convert the joint points to depth (display) space
                    Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                    foreach (JointType jointType in joints.Keys)
                    {
                        // sometimes the depth(Z) of an inferred joint may show as negative
                        // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                        CameraSpacePoint position = joints[jointType].Position;
                        if (position.Z < 0)
                        {
                            position.Z = InferredZPositionClamp;
                        }

                        ColorSpacePoint colorSpacePoint = this.coordinateMapper.MapCameraPointToColorSpace(position);
                        jointPoints[jointType] = new Point(colorSpacePoint.X, colorSpacePoint.Y);
                    }

                    skeletonDrawingController.DrawBody(joints, jointPoints, dc, drawPen);

                    skeletonDrawingController.DrawTrainingDataOuput(dc, SkeletonModifier.TrainingDataTo3DSkeleton(SkeletonModifier.PreprocessSkeleton(body)), drawPen);

                    double[] bodyPreProcessedData = SkeletonModifier.PreprocessSkeleton(body);

                    int predictionResult = machineLearningAlg.getClassPrediction(bodyPreProcessedData);

                    machineLearningAlg.hasBodyPaused(bodyPreProcessedData);

                    if (predictionResult < 0)
                    {
                        outputMessage = "Undetermined";

                        // This seems excessive...
                        double differenceToContracted = SkeletonModifier.getSkeletonDifferenceSum(bodyPreProcessedData, currentExercise.contractedForm);
                        double differenceToExtended = SkeletonModifier.getSkeletonDifferenceSum(bodyPreProcessedData, currentExercise.extendedForm);

                        if (differenceToContracted > differenceToExtended)
                        {
                            skeletonDrawingController.DrawFormCorrection(joints, jointPoints, body, currentExercise.extendedForm, dc, drawPen);
                        }
                        else
                        {
                            skeletonDrawingController.DrawFormCorrection(joints, jointPoints, body, currentExercise.contractedForm, dc, drawPen);
                        }
                        // End of Excessive portion
                    }
                    else
                    {
                        outputMessage = currentExercise.classifierData[predictionResult].name;
                        if (predictionResult > 1)
                        {
                            skeletonDrawingController.DrawFormCorrection(joints, jointPoints, body, currentExercise.getAcceptedForm(currentExercise.classifierData[predictionResult].form), dc, drawPen);
                        }
                    }

                    // prevent drawing outside of our render area
                    this.bodyFrameDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        private void SetModeFrameArrived(Body body)
        {
            int penIndex = 0;
            if (body != null && currentExercise != null)
            {
                double[] bodyPreProcessedData = SkeletonModifier.PreprocessSkeleton(body);

                int predictionResult = machineLearningAlg.getClassPrediction(bodyPreProcessedData);

                if (machineLearningAlg.hasBodyPaused(bodyPreProcessedData))
                {
                    lastRecordedSkeleton = body;

                    storedSkeletons.Add(SkeletonModifier.TrainingDataTo3DSkeleton(bodyPreProcessedData));
                    totalRecordedSkeletons++;

                    using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
                    {
                        Pen drawPen = skeletonDrawingController.bodyColors[penIndex++];
                        dc.DrawImage(colorBitmap, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                        // convert the joint points to depth (display) space
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                        foreach (JointType jointType in joints.Keys)
                        {
                            // sometimes the depth(Z) of an inferred joint may show as negative
                            // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                            CameraSpacePoint position = joints[jointType].Position;
                            if (position.Z < 0)
                            {
                                position.Z = InferredZPositionClamp;
                            }

                            ColorSpacePoint colorSpacePoint = this.coordinateMapper.MapCameraPointToColorSpace(position);
                            jointPoints[jointType] = new Point(colorSpacePoint.X, colorSpacePoint.Y);
                        }
                        skeletonDrawingController.DrawBody(joints, jointPoints, dc, drawPen);
                        this.bodyFrameDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                        if (predictionResult < 0)
                        {
                            outputMessage = "Undetermined";

                            // This seems excessive...
                            double differenceToContracted = SkeletonModifier.getSkeletonDifferenceSum(bodyPreProcessedData, currentExercise.contractedForm);
                            double differenceToExtended = SkeletonModifier.getSkeletonDifferenceSum(bodyPreProcessedData, currentExercise.extendedForm);

                            if (differenceToContracted > differenceToExtended)
                            {
                                skeletonDrawingController.DrawFormCorrection(joints, jointPoints, body, currentExercise.extendedForm, dc, drawPen);
                            }
                            else
                            {
                                skeletonDrawingController.DrawFormCorrection(joints, jointPoints, body, currentExercise.contractedForm, dc, drawPen);
                            }
                            // End of Excessive portion
                        }
                        else
                        {
                            outputMessage = currentExercise.classifierData[predictionResult].name;
                            if (predictionResult > 1)
                            {
                                skeletonDrawingController.DrawFormCorrection(joints, jointPoints, body, currentExercise.getAcceptedForm(currentExercise.classifierData[predictionResult].form), dc, drawPen);
                            }
                        }
                    }
                }
            }
        }

        private void RecordModeFrameArrived(Body body)
        {
            using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                //dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                // Draw the color image onto the frame
                dc.DrawImage(colorBitmap, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                int penIndex = 0;
                Pen drawPen = skeletonDrawingController.bodyColors[penIndex++];

                if (body != null && currentExercise != null)
                {

                    lastRecordedSkeleton = body;

                    skeletonDrawingController.DrawClippedEdges(body, dc);

                    IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                    // convert the joint points to depth (display) space
                    Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                    foreach (JointType jointType in joints.Keys)
                    {
                        // sometimes the depth(Z) of an inferred joint may show as negative
                        // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                        CameraSpacePoint position = joints[jointType].Position;
                        if (position.Z < 0)
                        {
                            position.Z = InferredZPositionClamp;
                        }

                        ColorSpacePoint colorSpacePoint = this.coordinateMapper.MapCameraPointToColorSpace(position);
                        jointPoints[jointType] = new Point(colorSpacePoint.X, colorSpacePoint.Y);
                    }

                    skeletonDrawingController.DrawBody(joints, jointPoints, dc, drawPen);
                }

                // prevent drawing outside of our render area
                this.bodyFrameDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
            }


        }
    }
}
