using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Kinetique
{
    public class TouchInputRecogniser
    {

        //right hand is pressing screen == handDown[0]
        private Boolean[] handDown = new Boolean[2];

        private KinectSensor _sensor;

        //hand will be pressing screen if closer than metresFromPlaneToDraw metres.
        const float metresFromPlaneToDraw = 0.2f;
        const float metresFromPlaneToMove = 0.6f;

        const float metresAwayFromPlaneToGesture = 1f;

        private myCoord normalisedPlanePerpendicular;

        //used to report to win 8 or win 7 driver
        private MouseInput listener;

        public TouchInputRecogniser(KinectSensor sensor, MouseInput listener)
        {
            _sensor = sensor;
            _sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(_sensor_AllFramesReady);

            this.listener = listener;

            if (KinectMainController.isCalibrated)
                justCalibrated();
        }

        public void justCalibrated()
        {
            //to get the normalised plane perpendicular (for caching):
            //source: http://mathworld.wolfram.com/Point-PlaneDistance.html

            //get a vector parallel to the top and side edges of the screen
            myCoord horizontalEdgeDirection = myCoord.subtract(KinectMainController.topRight, KinectMainController.topLeft);
            myCoord verticalEdgeDirection = myCoord.subtract(KinectMainController.bottomLeft, KinectMainController.topLeft);


            //get a vector perpendicular to the plane (not yet normalised)
            normalisedPlanePerpendicular = myCoord.crossProduct(horizontalEdgeDirection, verticalEdgeDirection);


            //normalise it - will fail if two points on the screen are in the same place (makes the planePerpendicular 0) - TODO I have NOT guarded against this
            normalisedPlanePerpendicular = myCoord.multByScalar(normalisedPlanePerpendicular, 1 / myCoord.norm(normalisedPlanePerpendicular));
        }

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (!KinectMainController.isCalibrated || !KinectMainController.doMouseInput)
                return;

            Skeleton first = Kinetique.Calibrator.GetFirstSkeleton(e);

            if (first == null)
            {
                return;
            }

            var rightHand = first.Joints[JointType.HandRight];

            if (rightHand.TrackingState != JointTrackingState.Tracked)
            {
                //Don't have a good read on the joints so we cannot process gestures
                return;
            }
            myCoord handLeft = new myCoord();

            handLeft.X = first.Joints[JointType.HandLeft].Position.X;
            handLeft.Y = first.Joints[JointType.HandLeft].Position.Y;
            handLeft.Z = first.Joints[JointType.HandLeft].Position.Z;

            myCoord handRight = new myCoord();

            handRight.X = first.Joints[JointType.HandRight].Position.X;
            handRight.Y = first.Joints[JointType.HandRight].Position.Y;
            handRight.Z = first.Joints[JointType.HandRight].Position.Z;

            //distance in metres from screen
            float handRightDistance = Math.Abs(fastDistanceFromPlane(handRight));

            //scalars between 0 and 1 - 0 is the left, and the top
            float handRightX = xCoordPerpFromPlane(handRight);
            float handRightY = yCoordPerpFromPlane(handRight);

            outputToListener(handRightX, handRightY, handRightDistance, 0);



            ////distance in metres from screen
            float handLeftDistance = Math.Abs(fastDistanceFromPlane(handLeft));

            //scalars between 0 and 1 - 0 is the left, and the top
            float handLeftX = xCoordPerpFromPlane(handLeft);
            float handLeftY = yCoordPerpFromPlane(handLeft);

            if (handLeftDistance < metresFromPlaneToDraw && !handDown[1])
            {
                handDown[1] = true;
                if (!handDown[0])
                {
                    listener.MouseDown(mouseX, mouseY, 1);
                    listener.MouseUp(mouseX, mouseY, 1);
                }
            }
            else if (handLeftDistance > metresFromPlaneToDraw && handDown[1])
                handDown[1] = false;

            if (handRightDistance > 1 && handLeftDistance > 1)
                KinectMainController.doGestureRecognition = false;
            else KinectMainController.doGestureRecognition = true;

        }


        float mouseX = -1, mouseY = -1;
        //outputs data to listener. handID should be 0 for right hand, 1 for left hand
        private void outputToListener(float handX, float handY, float handDistance, int handID)
        {
            //Console.WriteLine("(" + handX + ", " + handY + ", " + handDistance + ")");
            if (handDistance < metresFromPlaneToDraw) //if close enough to click
            {
                if (!handDown[handID]) //if wasn't clicking before, mousedown
                {
                    listener.MouseDown(handX, handY, handID);
                    handDown[handID] = true;
                }
                else listener.MouseUpdate(handX, handY, handID); //otherwise just move the (already down) mouse
            }
            else if(handDown[handID]) //if the mouse was down, do mouseup.
            {
                handDown[handID] = false;
                listener.MouseUp(handX, handY, handID);
            }
            else if (handDistance < metresFromPlaneToMove) //if not close enough to click, but close enough to move, and mouse wasn't down
            {
                listener.MouseUpdate(handX, handY, handID); //just move the mouse
            }

            if (handDistance < metresFromPlaneToMove)
            {
                mouseX = handX;
                mouseY = handY;
            }
        }

        //returns the x coordinate of the point on the plane closest to this point
        private float xCoordPerpFromPlane(myCoord point)
        {
            //source: http://mathworld.wolfram.com/Point-PlaneDistance.html

            myCoord horizontalEdgeDirection = myCoord.subtract(KinectMainController.topRight, KinectMainController.topLeft);

            myCoord vectorFromPointToTopLeft = myCoord.subtract(point, KinectMainController.topLeft);

            return myCoord.dotProduct(horizontalEdgeDirection, vectorFromPointToTopLeft)/(float) Math.Pow( myCoord.norm(horizontalEdgeDirection), 2);
            
        }

        //returns the y coordinate of the point on the plane closest to this point
        private float yCoordPerpFromPlane(myCoord point)
        {
            //source: http://mathworld.wolfram.com/Point-PlaneDistance.html

            myCoord verticalEdgeDirection = myCoord.subtract(KinectMainController.topLeft, KinectMainController.bottomLeft);

            myCoord vectorFromPointToTopLeft = myCoord.subtract(KinectMainController.topLeft, point);

            return myCoord.dotProduct(verticalEdgeDirection, vectorFromPointToTopLeft) / (float) Math.Pow(myCoord.norm(verticalEdgeDirection), 2);
        }

        private float fastDistanceFromPlane(myCoord point)
        {
            //source: http://mathworld.wolfram.com/Point-PlaneDistance.html

            //get a vector that represents moving from the point to the top left of the screen
            myCoord vectorFromPointToTopLeft = myCoord.subtract(point, KinectMainController.topLeft);

            return myCoord.dotProduct(normalisedPlanePerpendicular, vectorFromPointToTopLeft);
        }

    }

}
