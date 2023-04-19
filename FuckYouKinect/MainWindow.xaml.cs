using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;


namespace FuckYouKinect
{
    public partial class MainWindow : Window
    {

        #region Member Variables 
        public KinectSensor kinect;
        #endregion Member Variables 

        #region Constructor
        public MainWindow()
        {

            InitializeComponent();
            KinectStart();

        }
        #endregion Constructor

        #region Methods
        public void DrawPoint(Joint point)
        {
            // Create an ellipse.
            Ellipse ellipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Black
            };

            // Position the ellipse according to the point's coordinates.
            Canvas.SetLeft(ellipse, ((point.Position.X * ActualHeight / 2) + (ActualWidth / 2)) - ellipse.Width / 2);
            Canvas.SetTop(ellipse, ((point.Position.Y * -ActualHeight / 2) + (ActualHeight / 2)) - ellipse.Height / 2);

            // Add the ellipse to the canvas.


            myCanvas.Children.Add(ellipse);

        }

        public void DrawBone(Joint joint1, Joint joint2)
        {
            Line line = new Line
            {
                X1 = ((joint1.Position.X * ActualHeight / 2) + (ActualWidth / 2)),
                Y1 = ((joint1.Position.Y * -ActualHeight / 2) + (ActualHeight / 2)),
                X2 = ((joint2.Position.X * ActualHeight / 2) + (ActualWidth / 2)),
                Y2 = ((joint2.Position.Y * -ActualHeight / 2) + (ActualHeight / 2)),
                Stroke = Brushes.Black,
                StrokeThickness = 4
            };
            myCanvas.Children.Add(line);
        }

        public void DrawAllBones(Skeleton user)
        {
            DrawBone(user.Joints[JointType.Head], user.Joints[JointType.ShoulderCenter]);
            DrawBone(user.Joints[JointType.ShoulderCenter], user.Joints[JointType.ShoulderRight]);
            DrawBone(user.Joints[JointType.ShoulderRight], user.Joints[JointType.ElbowRight]);
            DrawBone(user.Joints[JointType.ElbowRight], user.Joints[JointType.WristRight]);
            DrawBone(user.Joints[JointType.WristRight], user.Joints[JointType.HandRight]);
            DrawBone(user.Joints[JointType.ShoulderCenter], user.Joints[JointType.ShoulderLeft]);
            DrawBone(user.Joints[JointType.ShoulderLeft], user.Joints[JointType.ElbowLeft]);
            DrawBone(user.Joints[JointType.ElbowLeft], user.Joints[JointType.WristLeft]);
            DrawBone(user.Joints[JointType.WristLeft], user.Joints[JointType.HandLeft]);
            DrawBone(user.Joints[JointType.ShoulderCenter], user.Joints[JointType.Spine]);
            DrawBone(user.Joints[JointType.Spine], user.Joints[JointType.HipCenter]);
            DrawBone(user.Joints[JointType.HipCenter], user.Joints[JointType.HipRight]);
            DrawBone(user.Joints[JointType.HipRight], user.Joints[JointType.KneeRight]);
            DrawBone(user.Joints[JointType.KneeRight], user.Joints[JointType.AnkleRight]);
            DrawBone(user.Joints[JointType.AnkleRight], user.Joints[JointType.FootRight]);
            DrawBone(user.Joints[JointType.HipCenter], user.Joints[JointType.HipLeft]);
            DrawBone(user.Joints[JointType.HipLeft], user.Joints[JointType.KneeLeft]);
            DrawBone(user.Joints[JointType.KneeLeft], user.Joints[JointType.AnkleLeft]);
            DrawBone(user.Joints[JointType.AnkleLeft], user.Joints[JointType.FootLeft]);
        }

        void KinectStart()
        {
            kinect = KinectSensor
                .KinectSensors
                .FirstOrDefault
                (s => s.Status == KinectStatus.Connected);
            if (kinect != null)
            {
                kinect.SkeletonStream.Enable();
                kinect.SkeletonFrameReady +=
                    new EventHandler<SkeletonFrameReadyEventArgs>
                    (SkeletonFrameReady);

                kinect.Start();
                Closing += MainWindow_Closing;

            }
            else
            {
                Close();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kinect.Stop();
        }

        private static Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;
            if (skeletons != null)
            {
                //Find the closest skeleton 
                for (int i = 0; i < skeletons.Length; i++)
                {
                    if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (skeleton == null)
                        {
                            skeleton = skeletons[i];
                        }
                        else
                        {
                            if (skeleton.Position.Z > skeletons[i].Position.Z)
                            {
                                skeleton = skeletons[i];
                            }
                        }
                    }
                }
            }
            return skeleton;
        }
        void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs args)
        {

            using (var frame = args.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
                    Skeleton skeleton = GetPrimarySkeleton(skeletons);


                    if (skeletons.Length > 0)
                    {
                        var user = skeletons.Where(
                            u => u.TrackingState == SkeletonTrackingState.Tracked
                        ).FirstOrDefault();
                        if (user != null)
                        {
                            myCanvas.Children.Clear();
                            foreach (Joint joint in user.Joints)
                            {
                                // 3D coordinates in meters

                                DrawPoint(joint);
                            }
                            DrawAllBones(user);

                        }
                    }

                }
            }
        }

        private double GetJointAngle(Joint zeroJoint, Joint angleJoint)
        {
            Point zeroPoint = GetJointPoint(zeroJoint);
            Point anglePoint = GetJointPoint(angleJoint);
            Point x = new Point(zeroPoint.X + anglePoint.X, zeroPoint.Y);
            double a;
            double b;
            double c;
            a = Math.Sqrt(Math.Pow(zeroPoint.X - anglePoint.X, 2) +
            Math.Pow(zeroPoint.Y - anglePoint.Y, 2));
            b = anglePoint.X;
            c = Math.Sqrt(Math.Pow(anglePoint.X - x.X, 2) + Math.Pow(anglePoint.Y - x.Y, 2));
            double angleRad = Math.Acos((a * a + b * b - c * c) / (2 * a * b));
            double angleDeg = angleRad * 180 / Math.PI;
            if (zeroPoint.Y < anglePoint.Y)
            {
                angleDeg = 360 - angleDeg;
            }
            return angleDeg;
        }

        #endregion Methods





    }

}
