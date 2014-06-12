using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Kinetique
{
    public interface CalibrationListener
    {
        //does these in order
        void starting();
        void bottomLeftDone();
        void topLeftDone();
        void topRightDone();
        void allDone();
    }

    public class myCoord
    {
        public float X, Y, Z;

        //returns vector one minus vector two
        public static myCoord subtract(myCoord one, myCoord two)
        {
            myCoord toReturn = new myCoord();
            toReturn.X = one.X - two.X;
            toReturn.Y = one.Y - two.Y;
            toReturn.Z = one.Z - two.Z;
            return toReturn;
        }

        //returns the cross product of vector one and vector two
        public static myCoord crossProduct(myCoord one, myCoord two)
        {
            //computed by definition of cross product
            myCoord answer = new myCoord();
            answer.X = one.Y * two.Z - one.Z * two.Y;
            answer.Y = one.Z * two.X - one.X * two.Z;
            answer.Z = one.X * two.Y - one.Y - two.X;

            return answer;
        }

        //returns the euclidean norm of a 3d vector
        public static float norm(myCoord vector)
        {
            return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        //returns the scalar multiple of a given vector, without altering the original
        public static myCoord multByScalar(myCoord vector, float scalar)
        {
            myCoord toReturn = new myCoord();

            toReturn.X = vector.X * scalar;
            toReturn.Y = vector.Y * scalar;
            toReturn.Z = vector.Z * scalar;

            return toReturn;
        }

        //returns one 'dotproduct' two.
        public static float dotProduct(myCoord one, myCoord two)
        {
            return one.X * two.X + one.Y * two.Y + one.Z * two.Z;
        }


        //returns the distance of 'point' from the plane defined by the points planeOne, planeTwo, and planeThree. May be negative.
        public static float distanceFromPlane(myCoord planeOne, myCoord planeTwo, myCoord planeThree, myCoord point)
        {
            myCoord vectorFromPointToPlaneOne = subtract(point, planeOne);

            //compute plane perpendicular
            myCoord normalisedPlanePerpendicular = crossProduct(subtract(planeTwo, planeOne), subtract(planeThree, planeOne));
            //normalise it
            normalisedPlanePerpendicular = multByScalar(normalisedPlanePerpendicular, 1 / norm(normalisedPlanePerpendicular));


            //dot it with the vector from planeOne to point
            return myCoord.dotProduct(normalisedPlanePerpendicular, vectorFromPointToPlaneOne);
        }

    }

    public class Calibrator
    {
        private KinectSensor _sensor;                                    //this is the sensor throught the entire application (from the online demo)
        const int skeletonCount = 6;                                    //max number of skeletons the kinect can track (same)
        static Skeleton[] allSkeletons = new Skeleton[skeletonCount];   //array of tracked skeletons (same)
        static float xCoord, yCoord, zCoord;

        public static Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }


                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();

                return first;

            }
        }

        //event handler for AllFramesRead
        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //Get a skeleton
            Skeleton first = GetFirstSkeleton(e);

            if (first == null)
            {
                return;
            }

            //now extract the data we need..
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null ||
                    _sensor == null)
                {
                    return;
                }
                
                //..which is the coordinates of the right hand (seen as the left hand because we are facing backwards)
                xCoord = first.Joints[JointType.HandRight].Position.X;
                yCoord = first.Joints[JointType.HandRight].Position.Y;
                zCoord = first.Joints[JointType.HandRight].Position.Z;

                //Console.Clear();
                //Console.WriteLine("X: " + first.Joints[JointType.HandRight].Position.X + 
                //                 "\nY: " + first.Joints[JointType.HandRight].Position.Y + 
                //                 "\nZ: " + first.Joints[JointType.HandRight].Position.Z);
               
            }
        }


        //main method for calibration
        public void Calibrate(CalibrationListener listener) {
            if(listener!=null)
                listener.starting();
            
            //for all 4 corners get the coordinates
            KinectMainController.bottomLeft  = getCorner();
            Console.WriteLine(KinectMainController.bottomLeft.X + " " + KinectMainController.bottomLeft.Y + " " + KinectMainController.bottomLeft.Z);
            if (listener != null) 
                listener.bottomLeftDone();

            KinectMainController.topLeft  = getCorner();
            Console.WriteLine(KinectMainController.topLeft.X + " " + KinectMainController.topLeft.Y + " " + KinectMainController.topLeft.Z);
            if (listener != null) 
                listener.topLeftDone();

            KinectMainController.topRight = getCorner();
            Console.WriteLine(KinectMainController.topRight.X + " " + KinectMainController.topRight.Y + " " + KinectMainController.topRight.Z);
            if (listener != null) 
                listener.topRightDone();

            //don't store 4th corner, but this is the bottom right one.
            getCorner();
            //Console.WriteLine(KinectMainController.bottomRight.X + " " + KinectMainController.bottomRight.Y + " " + KinectMainController.bottomRight.Z);
            if (listener != null) 
                listener.allDone();        
        }

        private float Variance(Queue<float> sampleSpace)
        {
            float mean = 0.0f;
            float count = 0.0f;
            float var = 0.0f;


            foreach (float x in sampleSpace)
            {
                mean += x; count += 1;
            }

            mean = mean / count;
            
            foreach (float x in sampleSpace)
            {
                var += (x - mean)*(x - mean);
                
            }

            var /= count;

            return var;
        }

        private myCoord getCorner()
        {
            const float eps = 0.00006f;                     //epsilon value, dictates precision for the variance
            myCoord sample = new myCoord();                 //sample data we get after each time span
            Queue<float> xQueue = new Queue<float>(10);     //queues for the x, y, z values
            Queue<float> yQueue = new Queue<float>(10);     //we get data every 0.2 seconds, and stop when we detect that the
            Queue<float> zQueue = new Queue<float>(10);     //hand has stayed still for 2 seconds, hence analyse the last 10 values

            System.DateTime t1 = System.DateTime.Now;       //current time
            System.TimeSpan span = new System.TimeSpan(0, 0, 0, 0, 200);  //time span of 0.2 seconds between each sampling of the position
            System.DateTime t3 = new System.DateTime();     //time at which we have to get the new sample

            //fill the queues with the initial 10 samples
            while (xQueue.Count < 10)
            {
                t3 = t1.Add(span);      //when we get the next round of data
                xQueue.Enqueue(xCoord); yQueue.Enqueue(yCoord); zQueue.Enqueue(zCoord); //get current round of data
                
                //wait until next round
                while (t1 < t3)
                {
                    t1 = System.DateTime.Now;
                }
            }

            //compute the initial variances of position data
            float xVar = Variance(xQueue);
            float yVar = Variance(yQueue);
            float zVar = Variance(zQueue);

            //while the variances are to high, meaning the hand isn't still, keep tracking the hand
            while ((xVar > eps | yVar > eps | zVar > eps) || (xQueue.Average() == 0 && yQueue.Average() == 0 && zQueue.Average() == 0))
            {
                t3 = t1.Add(span);      //when we get next round of data
                
                xQueue.Dequeue(); yQueue.Dequeue(); zQueue.Dequeue();                       //discard old data
                xQueue.Enqueue(xCoord); yQueue.Enqueue(yCoord); zQueue.Enqueue(zCoord);     //get current round
                xVar = Variance(xQueue); yVar = Variance(yQueue); zVar = Variance(zQueue);  //compute new variance
                
                //wait until next round
                while (t1 < t3)
                {
                    t1 = System.DateTime.Now;
                }
            }

            //when the hand hasn't moved return it's position, averaging over the last 10 positions 
            sample.X = xQueue.Average();
            sample.Y = yQueue.Average();
            sample.Z = zQueue.Average();
            
            Console.Beep();     //give an audio confirmation to the user
            return sample;
        }


        //stuff that get's run
//        static void Main(string[] args)
//        {
            //SetupKinectManually();      //enable the kinect
            //Console.WriteLine("Ready");
            //Calibrate();                //do the 4-point calibration
            //Console.ReadLine();         //press a key to exit application


            //TODO this bit is new
            //Console.WriteLine("moving on to tom's bit");

            //TODO - emil needs to correct the 4 points to a plane. for the moment, I assume these 3 arbitrary points are the ones. They must be these points.
            //Kinetique.TouchInputRecogniser touchInputRecogniser = new Kinetique.TouchInputRecogniser(_sensor, leftUp, rightUp, leftDown);



            //Console.ReadLine();         //press a key to exit application


            //StopKinect(_sensor);        //disable the kinect before exiting
//        }

        public Calibrator(KinectSensor _sensor)
        {
            _sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(_sensor_AllFramesReady);
            this._sensor = _sensor;
        }
    }
}
