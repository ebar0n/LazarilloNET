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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Timers;
using System.Media;
using System.IO;
namespace ProyectoUNET_Kinect
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor sensor;
        private byte[] pixelData;
        private byte[] depth32;
        private bool personas,obstacu;


        public MainWindow()
        {
            verificar();
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            
            personas = false;
            obstacu = false;

        }

        void HayPersonas()
        {
                SoundPlayer simpleSound = new SoundPlayer("personas.wav");
                simpleSound.Play();
        }
        void NoHayPersonas()
        {
                SoundPlayer simpleSound = new SoundPlayer("personasfuera.wav");
                simpleSound.Play();

        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.StopSensor();
        }

        private void StartSensor()
        {

            try
            {

                this.sensor = KinectSensor.KinectSensors[0];
                if (this.sensor != null && !this.sensor.IsRunning)
                {
                    this.sensor.Start();
                    this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    this.sensor.ColorFrameReady += sensor_ColorFrameReady;

                    this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    this.sensor.SkeletonStream.Enable();
                    //this.sensor.DepthStream.Range = DepthRange.Near;
                    this.sensor.DepthFrameReady += sensor_DepthFrameReady;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StopSensor()
        {
            if (this.sensor != null && this.sensor.IsRunning)
            {
                this.sensor.Stop();
            }
        }

        void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthimageFrame = e.OpenDepthImageFrame())
            {
                if (depthimageFrame == null)
                {
                    return;
                }
                short[] pixelData = new short[depthimageFrame.PixelDataLength];
                
                depthimageFrame.CopyPixelDataTo(pixelData);
                
                depth32 = new byte[depthimageFrame.PixelDataLength * 4];
                
                Obstaculo.Content = "";
                obstacu = false;
                this.GetColorPixelDataWithDistance(pixelData);//agrega datos solo de profundidad
                
                NumP.Content = "";
                this.TrackPlayer(pixelData);//agrega datos de personas
                
                depthImageControl.Source = BitmapSource.Create(
                depthimageFrame.Width, depthimageFrame.Height, 96, 96, PixelFormats.
                Bgr32, null, depth32, depthimageFrame.Width * 4
                );
            }
        }

        private short[] ReversingBitValueWithDistance(DepthImageFrame depthImageFrame, short[] pixelData)
        {
                short[] reverseBitPixelData = new short[depthImageFrame.PixelDataLength];
                int depth;
                for (int index = 0; index < pixelData.Length; index++)
                {
                    depth = pixelData[index] >> DepthImageFrame.
                        PlayerIndexBitmaskWidth;
                    if (depth < 1500 || depth > 3500)
                    {
                        reverseBitPixelData[index] = (short)~pixelData[index]; ;
                    }
                    else
                    {
                        reverseBitPixelData[index] = pixelData[index];
                    }
                }
                return reverseBitPixelData;
         }

        private void GetColorPixelDataWithDistance(short[] depthFrame)
        {
                for (int depthIndex = 0, colorIndex = 0; depthIndex < depthFrame.Length && colorIndex < this.depth32.Length; depthIndex++, colorIndex += 4)
                {
                    int distance = depthFrame[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    if (distance <= 800)
                    {
                        if (obstacu == false && (depthIndex >= 3 ))
                        {
                            obstacu = true;
                            Obstaculo.Content = "Hay obstaculos muy cerca menores de 800 mm...";
                        }
                        depth32[colorIndex + 2] = 115;
                        depth32[colorIndex + 1] = 169;
                        depth32[colorIndex + 0] = 9;
                    }
                    else if (distance > 800 && distance <= 2000)
                    {
                        if (obstacu == false)
                        {
                            obstacu = true;
                            Obstaculo.Content = "Hay obstaculos muy cercanos menores a 2000 mm...";
                        }
                        depth32[colorIndex + 2] = 255;
                        depth32[colorIndex + 1] = 61;
                        depth32[colorIndex + 0] = 0;
                    }
                    else if (distance > 2000 && distance<=3000)
                    {
                        depth32[colorIndex + 2] = 169;
                        depth32[colorIndex + 1] = 9;
                        depth32[colorIndex + 0] = 115;
                    }
                    else if( distance>3000 ){
                        depth32[colorIndex + 2] = 255;
                        depth32[colorIndex + 1] = 255;
                        depth32[colorIndex + 0] = 255;
                    }
                }
        }

        private void TrackPlayer(short[] depthFrame)
        {
               bool ban = false;
               for (int depthIndex = 0, colorIndex = 0; depthIndex < depthFrame.Length && colorIndex < this.depth32.Length; depthIndex++, colorIndex += 4)
               {
                    int player = depthFrame[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                    if (player > 0)
                    {
                        depth32[colorIndex + 2] = 169;
                        depth32[colorIndex + 1] = 62;
                        depth32[colorIndex + 0] = 9;
                        if (NumP.Content.Equals("") == true)
                        {
                            NumP.Content = "Hay personas";
                            ban = true;
                        }
                    }
                }

               if ( ban==false && personas==true)
               {
                   NoHayPersonas();
                   personas = false;
               }
               else if ( ban==true && personas == false) {
                   HayPersonas();
                   personas = true;
               }


        }


        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
   
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                // Check if the incoming frame is not null
                if (imageFrame == null)
                {
                    return;
                }
                else
                {
                    // Get the pixel data in byte array
                    this.pixelData = new byte[imageFrame.PixelDataLength];
                    // Copy the pixel data
                    imageFrame.CopyPixelDataTo(this.pixelData);
                    // Calculate the stride
                    int stride = imageFrame.Width * imageFrame.BytesPerPixel;
                    // assign the bitmap image source into image control
                    this.VideoControl.Source = BitmapSource.Create(
                        imageFrame.Width,
                        imageFrame.Height,
                        96,
                        96,
                        PixelFormats.Bgr32,
                        null,
                        pixelData,
                        stride);
                }
            }
        }
        

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            

            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.StartSensor();
                
            }
            else
            {
                MessageBox.Show("No hay conexion con el sensor!");
                this.Close();
            }
        }

        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {


            this.estado.Content = e.Status;
    
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    this.StartSensor();
                    break;
                
                case KinectStatus.Disconnected:
                    this.StopSensor();
                    // Device DisConnected;
                    break;

                case KinectStatus.NotPowered:
                    this.StopSensor();
                    break;

                case KinectStatus.Initializing:
                     this.StartSensor();
                     break;
            }
        }

       
        
        public void verificar()
        {

            try
            {
                // Verifica si ya existe una instancia de la aplicación ejecutándose
                System.Diagnostics.Process[] ProcessObj = null;
                String ModualName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
                String ProcessName = System.IO.Path.GetFileNameWithoutExtension(ModualName);

                // MessageBox.Show(ModualName+" / "+ProcessName);
                // Obtiene todas las intancias del proceso, ejecutándose en el computador local
                ProcessObj = System.Diagnostics.Process.GetProcessesByName(ProcessName);

                if (ProcessObj.Length > 1) // si ya hay una aplicación, devolvemos true  
                {
                    MessageBox.Show("Ya hay una instancia de la aplicación ejecutándose", "Notificación", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close(); //Cerrar la aplicación
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       

    }
}
