using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectGettingStarted
{
    class Program
    {
        
        static KinectSensor kinect;
        
        
        private static Skeleton[] skeletons;
        
        
        static bool isForwardGestureActive;
        static bool isBackGestureActive;
        
        static void Main(string[] args)
        {
            int exitStatus = 0;

            // Récupère la première kinect connectée
            kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);

            if (kinect == null)
            {
                Console.WriteLine("This application requires a Kinect sensor. Press 'ENTER' to exit.");
                Console.ReadLine();
                exitStatus = -1;
            }
            else
            {
                // Active le skeletal tracking
                kinect.SkeletonStream.Enable();
                kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady); // Initialisation du skeleton listener

                kinect.Start(); // Démarre la kinect
                
                Console.WriteLine("The application is running. Open your application and make your move or press 'ENTER' to exit.");
                Console.ReadLine();

                StopKinect();
            }
            Environment.Exit(exitStatus);
        }
        
        
        private static void StopKinect()
        {
            if (kinect != null)
            {
                kinect.Stop();
                kinect.Dispose();
                kinect = null;
            }
        }

        private static void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                    return;

                if (skeletons == null ||
                    skeletons.Length != skeletonFrame.SkeletonArrayLength)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                }

                skeletonFrame.CopySkeletonDataTo(skeletons);
            }

            Skeleton closestSkeleton = skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked)
                                                .OrderBy(s => s.Position.Z * Math.Abs(s.Position.X))
                                                .FirstOrDefault();

            if (closestSkeleton == null)
                return;

            var head = closestSkeleton.Joints[JointType.Head];
            var rightHand = closestSkeleton.Joints[JointType.HandRight];
            var leftHand = closestSkeleton.Joints[JointType.HandLeft];

            if (head.TrackingState == JointTrackingState.NotTracked ||
                rightHand.TrackingState == JointTrackingState.NotTracked ||
                leftHand.TrackingState == JointTrackingState.NotTracked)
            {
                //Nous n'avons pas une bonne lecture des jointures donc nous n'effectuons aucun traitement
                return;
            }

            
            MoveSlide(head, rightHand, leftHand);
            
        }

        
        private static void MoveSlide(Joint head, Joint rightHand, Joint leftHand)
        {
            if (rightHand.Position.X > head.Position.X + 0.45)
            {
                if (!isForwardGestureActive)
                {
                    isForwardGestureActive = true;
                    Console.WriteLine("Right move");
                    System.Windows.Forms.SendKeys.SendWait("{Right}");
                }
            }
            else
            {
                isForwardGestureActive = false;
            }

            if (leftHand.Position.X < head.Position.X - 0.45)
            {
                if (!isBackGestureActive)
                {
                    isBackGestureActive = true;
                    Console.WriteLine("Left move");
                    System.Windows.Forms.SendKeys.SendWait("{Left}");
                }
            }
            else
            {
                isBackGestureActive = false;
            }
        }
    }
}
