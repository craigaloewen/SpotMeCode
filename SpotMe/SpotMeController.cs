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
        /// Current status text to display
        /// </summary>
        private string statusText = null;

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
            View
        };

        public ControllerMode currentMode;

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
        }

        ~SpotMeController()
        {
            CleanUp();
        }

        public bool Init()
        {
            outputMessage = "Initialized";
            currentMode = ControllerMode.Continuous;

            machineLearningAlg.init();

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
            machineLearningAlg.init(inName);
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

            if (dataReceived)
            {
                if (currentMode == ControllerMode.Continuous)
                {
                    ContinuousModeFrameArrived();
                } else
                {
                    int a = 5;
                }
            }
        }

        public bool switchMode(ControllerMode inputMode)
        {
            using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
            }
            currentMode = inputMode;
            return true;
        }

        private void ContinuousModeFrameArrived()
        {
            using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                //dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                // Draw the color image onto the frame
                dc.DrawImage(colorBitmap, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                int penIndex = 0;
                foreach (Body body in this.bodies)
                {
                    Pen drawPen = skeletonDrawingController.bodyColors[penIndex++];

                    if (body.IsTracked && currentExercise != null)
                    {

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

                // prevent drawing outside of our render area
                this.bodyFrameDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
            }
        }


    }
}
