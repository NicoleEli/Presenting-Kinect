using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Forms;

namespace Kinetique
{
    public class GestureRecogniser
    {
        private KinectSensor _sensor;

        bool isForwardGestureActive = false;
        bool isBackGestureActive = false;
        bool isLeftGestureActive = false;
        bool isRightGestureActive = false;
        bool isSwipeLeftGestureActive = false;
        bool isSwipeRightGestureActive = false;
     
        public GestureRecogniser(KinectSensor sensor)
        {
            _sensor = sensor;
            _sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(_sensor_AllFramesReady);
        }
        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            Skeleton first = Kinetique.Calibrator.GetFirstSkeleton(e);

            if (first == null)
            {
                return;
            }

            var head = first.Joints[JointType.Head];
            var rightHand = first.Joints[JointType.HandRight];
            var leftHand = first.Joints[JointType.HandLeft];
            var chest = first.Joints[JointType.ShoulderCenter];

            if (head.TrackingState != JointTrackingState.Tracked ||
                    rightHand.TrackingState != JointTrackingState.Tracked ||
                    leftHand.TrackingState != JointTrackingState.Tracked ||
                    chest.TrackingState != JointTrackingState.Tracked )
            {
                //Don't have a good read on the joints so we cannot process gestures
                return;
            }
            
            if (KinectMainController.doGestureRecognition == true) ProcessGesture(head, rightHand, leftHand,chest);    
        }

        private void ProcessGesture(Joint head, Joint rightHand, Joint leftHand,  Joint chest)
        {   
            // Raise your left hand above your head and right hand to the right to activate right gesture
            if ((leftHand.Position.X < head.Position.X - 0.45) && (rightHand.Position.Y > head.Position.Y + 0.2))
            {
                if (!isBackGestureActive && !isLeftGestureActive && !isSwipeLeftGestureActive && !isSwipeRightGestureActive && !isForwardGestureActive && !isRightGestureActive)
                {
                    isLeftGestureActive = true;
                    //System.Windows.Forms.SendKeys.SendWait("{Left}");
                    KinectMainController.gestureListener.gestureOccurs(new LeftGesture());
                    Console.WriteLine("left");
                }
            }
            else
            {
                isLeftGestureActive = false;
            }
            // Raise your right hand above your head and left hand to the left to activate right gesture
            if ((rightHand.Position.X > head.Position.X + 0.45) && (leftHand.Position.Y > head.Position.Y + 0.2))
            {
                if (!isBackGestureActive && !isLeftGestureActive && !isSwipeLeftGestureActive && !isSwipeRightGestureActive && !isForwardGestureActive && !isRightGestureActive)
                {
                    isRightGestureActive = true;
                    //System.Windows.Forms.SendKeys.SendWait("{Right}");
                    KinectMainController.gestureListener.gestureOccurs(new RightGesture());
                    Console.WriteLine("right");
                }
            }
            else
            {
                isRightGestureActive = false;
            }
            // Raise your left hand above your head and push right hand foward to activate forward gesture
            if ((rightHand.Position.Z < head.Position.Z - 0.4) && (leftHand.Position.Y > head.Position.Y+0.2))
            {
                if (!isBackGestureActive && !isLeftGestureActive && !isSwipeLeftGestureActive && !isSwipeRightGestureActive && !isForwardGestureActive && !isRightGestureActive)
                {
                    isForwardGestureActive = true;
                    //System.Windows.Forms.SendKeys.SendWait("{Up}");
                    KinectMainController.gestureListener.gestureOccurs(new UpGesture());
                    Console.WriteLine("up");
                }
            }
            else
            {
                isForwardGestureActive = false;
            }
            // Raise your right hand above your head and push left hand forward to activate back gesture
            if ((leftHand.Position.Z < head.Position.Z - 0.4) && (rightHand.Position.Y > head.Position.Y + 0.2) )
            {
                if (!isBackGestureActive && !isLeftGestureActive && !isSwipeLeftGestureActive && !isSwipeRightGestureActive && !isForwardGestureActive && !isRightGestureActive)
               {
                    isBackGestureActive = true;
                    //System.Windows.Forms.SendKeys.SendWait("{Down}");
                    KinectMainController.gestureListener.gestureOccurs(new DownGesture());
                    Console.WriteLine("down");
                }
            }
            else
            {
                isBackGestureActive = false;
            }

            if ((rightHand.Position.Z < head.Position.Z - 0.18) && (rightHand.Position.X < chest.Position.X + 0.19) && (rightHand.Position.X > chest.Position.X - 0.19) && (rightHand.Position.Y < chest.Position.Y + 0.19) && (rightHand.Position.Y > chest.Position.Y - 0.19) && (leftHand.Position.X < head.Position.X - 0.45))
            {
                if (!isBackGestureActive && !isLeftGestureActive && !isSwipeLeftGestureActive && !isSwipeRightGestureActive && !isForwardGestureActive && !isRightGestureActive)
                {
                    isSwipeLeftGestureActive = true;
                    KinectMainController.gestureListener.gestureOccurs(new SwLeftGesture());
                    Console.WriteLine("Left Switch");
                }
            }
            else
            {
                isSwipeLeftGestureActive = false;
            }
            if ((leftHand.Position.Z < head.Position.Z - 0.18) && (leftHand.Position.X < chest.Position.X + 0.19) && (leftHand.Position.X > chest.Position.X - 0.19) && (leftHand.Position.Y < chest.Position.Y + 0.19) && (leftHand.Position.Y > chest.Position.Y - 0.19) && (rightHand.Position.X > head.Position.X + 0.45))
            {
                if (!isBackGestureActive && !isLeftGestureActive && !isSwipeLeftGestureActive && !isSwipeRightGestureActive && !isForwardGestureActive && !isRightGestureActive)
                {
                    isSwipeRightGestureActive = true;
                    KinectMainController.gestureListener.gestureOccurs(new SwRightGesture());
                    Console.WriteLine("Right Switch");
                }
            }
            else
            {
                isSwipeRightGestureActive = false;
            }
        }
    }
}
