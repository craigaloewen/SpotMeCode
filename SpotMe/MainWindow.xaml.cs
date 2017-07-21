//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace SpotMe
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Numerics;
    using Accord.MachineLearning.VectorMachines.Learning;
    using Accord.Statistics.Kernels;
    using Accord.Math.Optimization.Losses;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
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
        private KinectSensor kinectSensor = null;

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
        private WriteableBitmap colorBitmap = null;

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
        SkeletonDrawing outputDrawing;

        /// <summary>
        /// The Machine Learning algorithm variable
        /// </summary>
        private SpotMeML spotMeMLAlg;

        // Variables to handle showing training data
        private List<bodyDouble> trainingBodyDoubles;
        private int trainingBodyDoublesIndex;

        // Some quick hacks to store skeleton data
        private int storeTrainingDataHACKNum = -1001;
        private double[][] trainingDataHACKStore = new double[5][];



        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
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

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // Create the drawing group we'll use for drawing
            this.bodyFrameDrawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.bodyFrameSource = new DrawingImage(this.bodyFrameDrawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // Initialize some of the training variables
            trainingBodyDoubles = new List<bodyDouble>();
            trainingBodyDoublesIndex = 0;

            // Initialize the ML aspect
            spotMeMLAlg = new SpotMe.SpotMeML();

            // Initializes the drawing component
            outputDrawing = new SkeletonDrawing(coordinateMapper);

            // initialize the components (controls) of the window
            this.InitializeComponent();

            //
            // Here is where you can add in some test data to test the file system!
            // This is all temporary so just use this space to test and then we will delete it later
            //

            Exercise testExercise = new Exercise("squat");

            testExercise.AddClassifier(SkeletonForm.Contracted, "Good Something Form", "Keep it up!");
            testExercise.AddClassifier(SkeletonForm.Extended, "Good Extended Form", "Great form! :)");
            testExercise.AddClassifier(SkeletonForm.Extended, "Good Contracted Form", "Awesome!");

            double[] inputData1 = new double[24] { 0.5,0,0,0,0,0,0,0,0,0,0.2,0,0,0,0,0,0,0,0,0,0,0.1,0,0};
            double[] inputData2 = new double[24] { 0.5,0,0,0,0,0,0.9,0,0,0,0.2,0,0,0,0,0,0.3,0,0,0,0,0.1,0,0};
            double[] inputData3 = new double[24] { 0.5,0,0,0,0,0,0.9,0,0,0,0.2,0,0,0,0,0,0.3,0,0,0,0,0.1,0,0};
            double[] inputData4 = new double[24] { 0.5,0,0,0,0,0,0.9,0,0,0,0.2,0,0,0,0,0,0.3,0,0,0,0,0.1,0,0};
            double[] inputData5 = new double[24] { 0.5, 0, 0, 0, 0, 0, 0.9, 0, 0, 0, 0.2, 0, 0, 0, 0, 0, 0.3, 0, 0, 0, 0, 0.1, 0, 0 };
            double[] inputData6 = new double[24] { 0.5, 0, 0, 0, 0, 0, 0.9, 0, 0, 0, 0.2, 0, 0, 0, 0, 0, 0.3, 0, 0, 0, 0, 0.1, 0, 0 };
            double[] inputData7 = new double[24] { 0.5, 0, 0, 0, 0, 0, 0.9, 0, 0, 0, 0.2, 0, 0, 0, 0, 0, 0.3, 0, 0, 0, 0, 0.1, 0, 0 };

            testExercise.AddTrainingData(0, inputData1);
            testExercise.AddTrainingData(1, inputData2);
            testExercise.AddTrainingData(1, inputData3);
            testExercise.AddTrainingData(2, inputData4);
            testExercise.AddTrainingData(2, inputData5);
            testExercise.AddTrainingData(2, inputData6);
            testExercise.AddTrainingData(2, inputData7);

            if (testExercise.UpdateFormDefinitions())
            {
                ExerciseManager.saveExerciseV2(testExercise);
                Exercise squatV2 = ExerciseManager.loadExerciseV2("SQUAT");
            }


        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource BodyImageSource
        {
            get
            {
                return this.bodyFrameSource;
            }
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ColorImageSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            spotMeMLAlg.init();

            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
            }

            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
            }

        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
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
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
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
                using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
                {
                    // Draw a transparent background to set the render size
                    //dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    // Draw the color image onto the frame
                    dc.DrawImage(ColorImageSource, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    //updateTrainingDataWithGivenDrawingContext(dc);

                    int penIndex = 0;
                    foreach (Body body in this.bodies)
                    {
                        Pen drawPen = outputDrawing.bodyColors[penIndex++];

                        if (body.IsTracked)
                        {

                            outputDrawing.DrawClippedEdges(body, dc);

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

                            outputDrawing.DrawBody(joints, jointPoints, dc, drawPen);

                            outputDrawing.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                            outputDrawing.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);

                            // Put your debugging code here to execute each time a skeleton is drawn
                            outputDrawing.DrawTrainingDataOuput(dc, SkeletonModifier.TrainingDataTo3DSkeleton(SkeletonModifier.PreprocessSkeleton(body)),drawPen);
                            outputDrawing.DrawFormCorrection(joints, jointPoints, body, spotMeMLAlg.goodForm, dc, drawPen);

                            // This is some hacked together code to store training data and test functions. It's not reliable for production use but fine for testing.
                            // Inputs a body to test if it has paused
                            spotMeMLAlg.hasBodyPaused(body);

                            // Outputs some debug information
                            trainingDataLabel.Content = spotMeMLAlg.getClassPrediction(body).ToString();
                            //trainingDataLabel.Content = storeTrainingDataHACKNum;
                            //trainingDataLabel.Content = Math.Round(spotMeMLAlg.movementIndexValue,3);

                            if ((storeTrainingDataHACKNum) >= -1000)
                            {
                                if ((storeTrainingDataHACKNum % 100 == 0) && (storeTrainingDataHACKNum >= 0))
                                {
                                    trainingDataHACKStore[storeTrainingDataHACKNum/100] = SkeletonModifier.PreprocessSkeleton(body);
                                }
                                
                                storeTrainingDataHACKNum++;
                                

                                if (storeTrainingDataHACKNum > 400)
                                {
                                    TrainingDataIO.saveTrainingData(trainingDataHACKStore, "testDataOutput.csv");
                                    storeTrainingDataHACKNum = -1001;
                                }
                            }
                            // End of the hacked together code
                            
                        }
                    }

                    // prevent drawing outside of our render area
                    this.bodyFrameDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
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

        

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }


        // -----------
        // Here is a bunch of debugging functions that I made to get stuff working in the interim. - Craig
        // ------------


        private void loadTrainingData(object sender, RoutedEventArgs e)
        {
            trainingBodyDoublesIndex = 0;
            trainingBodyDoubles = TrainingDataFileManager.loadBodyDoubleFromFileWithClassifierIgnored(fileNameBox.Text);

            updateTrainingData();
        }

        private void nextTrainingData(object sender, RoutedEventArgs e)
        {
            if (trainingBodyDoublesIndex < (trainingBodyDoubles.Count-1))
            {
                trainingBodyDoublesIndex++;
            }

            updateTrainingData();
        }

        private void prevTrainingData(object sender, RoutedEventArgs e)
        {
            if (trainingBodyDoublesIndex > 0)
            {
                trainingBodyDoublesIndex--;
            }

            updateTrainingData();
        }

        private void debugFunction(object sender, RoutedEventArgs e)
        {
            storeTrainingDataHACKNum = -200;
        }

        private void debugFunctionLoadTrainingData(object sender, RoutedEventArgs e)
        {
            Accord.Math.Random.Generator.Seed = 0;

            double[][] inputData = TrainingDataIO.readTrainingData("militaryPressData.csv");
            double[][] testInputs = TrainingDataIO.readTrainingData("bicepCurlData.csv");

            double[][] inputs = inputData;

            int[] outputs =
            {
                0,0,0,
                1,1,1,1
            };

            // Create the multi-class learning algorithm for the machine
            var teacher = new MulticlassSupportVectorLearning<Gaussian>()
            {
                // Configure the learning algorithm to use SMO to train the
                //  underlying SVMs in each of the binary class subproblems.
                Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
                {
                    // Estimate a suitable guess for the Gaussian kernel's parameters.
                    // This estimate can serve as a starting point for a grid search.
                    UseKernelEstimation = true
                }
            };

            // Configure parallel execution options
            teacher.ParallelOptions.MaxDegreeOfParallelism = 1;

            // Learn a machine
            var machine = teacher.Learn(inputs, outputs);


            var supportVectors = machine.Models[0][0].SupportVectors;



            trainingBodyDoublesIndex = 0;
            
            for (int i = 0; i < supportVectors.Length; i++)
            {
                trainingBodyDoubles.Add(SkeletonModifier.TrainingDataTo3DSkeleton(supportVectors[i]));
            }


        }

        private void updateTrainingData()
        {
            using (DrawingContext dc = this.bodyFrameDrawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                if (trainingBodyDoubles.Count < 1)
                {
                    trainingDataLabel.Content = "Failure to Open";
                }
                else
                {
                    trainingDataLabel.Content = ("Success " + (trainingBodyDoublesIndex+1) + " of " + trainingBodyDoubles.Count.ToString());
                    outputDrawing.DrawTrainingDataOuput(dc, trainingBodyDoubles[trainingBodyDoublesIndex], outputDrawing.bodyColors[0]);
                }
            }
        }

        private void updateTrainingDataWithGivenDrawingContext(DrawingContext dc)
        {
            // Draw a transparent background to set the render size
            dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

            if (trainingBodyDoubles.Count < 1)
            {
                trainingDataLabel.Content = "Failure to Open";
            }
            else
            {
                trainingDataLabel.Content = ("Success " + (trainingBodyDoublesIndex + 1) + " of " + trainingBodyDoubles.Count.ToString());
                outputDrawing.DrawTrainingDataOuput(dc, trainingBodyDoubles[trainingBodyDoublesIndex], outputDrawing.bodyColors[0]);
            }
        }
    }
}
