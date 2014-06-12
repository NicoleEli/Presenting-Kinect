using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Kinetique
{
    public class testMouseInput : MouseInput
    {
        public void MouseUp(float x, float y, int handID)
        {
        }
        public void MouseDown(float x, float y, int handID)
        {

        }
        public void MouseUpdate(float x, float y, int handID)
        {
            Console.WriteLine("(" + x + " , " + y + " , " + handID + ")");
        }

    }

    class testCalibrationListener : CalibrationListener
    {
        public void starting()
        { Console.Write("bottom left... ");  }
        public void bottomLeftDone()
        { Console.Write("done\ntop left... "); }
        public void topLeftDone()
        { Console.Write("done\ntop right... "); }
        public void topRightDone()
        { Console.Write("done\nbottom right... "); }
        public void allDone()
        { Console.Write("done");
        Console.WriteLine("ready for input");
        }
    }

    class KinectMainController
    {
        //for testing purposes
        static void Main(string[] args)
        {
            KinectMainController main = new KinectMainController(new Mouse_Simulator());
            //KinectMainController main = new KinectMainController(new testMouseInput());
            //main.calibrateSensor(new testCalibrationListener());
            while(("calibrate" == Console.ReadLine()))
                main.calibrateSensor(new testCalibrationListener());
        }
        MouseInput listener;

        //controlled by this main class's calibrate method
        public static Boolean doMouseInput = true;
        //controlled by the mouse input object
        public static Boolean doGestureRecognition = true;
        //controlled by the calibrate method
        public static Boolean isCalibrated = false;
        //defines plane and screen
        public static myCoord topLeft, topRight, bottomLeft;

        private Calibrator c;
        private TouchInputRecogniser h;

        public static  GestureListener gestureListener;

        static KinectSensor _sensor;
        public KinectMainController(MouseInput listener)
        {
            this.listener = listener;
            SetupKinectManually();      //enable the kinect
            Console.WriteLine("Ready");

            //hand -> mouse input object
            h = new TouchInputRecogniser(_sensor, listener);

            //Calibration object
            c = new Calibrator(_sensor);

            //Gesture recognition object
            GestureRecogniser g = new GestureRecogniser(_sensor);

            //Gesture listener object
            gestureListener = new DefaultListener();
        }

        public void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }
                }
            }
        }

        //method that enable the kinect, everything is from the online demo
        private static void SetupKinectManually()
        {
            //smoothing parameters as shown in the online demo
            //I considered that if I use smoothing it would help the calibration process,
            //as it is easier to detect when the hand is not moving this way

            var parameters = new TransformSmoothParameters
            {
                Smoothing = .96f,
                Correction = 0.2f,
                Prediction = 0.2f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.05f
            };

            if (KinectSensor.KinectSensors.Count > 0)
            {
                //use first Kinect
                _sensor = KinectSensor.KinectSensors[0];

                if (_sensor.Status == KinectStatus.Connected)
                {
                    _sensor.DepthStream.Enable();
                    _sensor.SkeletonStream.Enable(parameters);
                    _sensor.SkeletonStream.Enable();
                    //the event handler that gets the depth+color+skeleton frames when they are all ready
                    try
                    {
                        _sensor.Start();
                    }
                    catch (System.IO.IOException)
                    {
                        Console.WriteLine("Error: Application conflict"); //to inforce that only one app can use the sensor
                    }
                }

            }
            else
            {
                Console.WriteLine("No Kinects are connected");
            }


        }

        public void calibrateSensor(CalibrationListener l)
        {
            isCalibrated = false;
            c.Calibrate(l);
            h.justCalibrated();
            isCalibrated = true;
        }

        
    }
}
