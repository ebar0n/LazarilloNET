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

namespace ProyectoUNET_Kinect
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor kinectS;
        private WriteableBitmap colorBitmap;
        private byte[] colorPixels;

        public MainWindow()
        {
            verificar();
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            int num = KinectSensor.KinectSensors.Count;//retorna el numero de sensores conectado
            //MessageBox.Show("" + num);
            if (num <= 0)
            {
                MessageBox.Show("No hay ningun 'sensor kinect' conectado", "Notificación", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown(-1); //Cerrar la aplicación
            }
            else
            {
                Closing += MainWindow_Closing;
                //genera el evento para finalisar el uso del sensor
                kinectS = KinectSensor.KinectSensors[0];
                try
                {
                    kinectS.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    colorPixels = new byte[kinectS.ColorStream.FramePixelDataLength];
                    colorBitmap = new WriteableBitmap(kinectS.ColorStream.FrameWidth, kinectS.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                
                }
                catch (Exception){
                    MessageBox.Show("Para iniciar la aplicacion conecte el kinect a una toma de corriente", "Notificación", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                visorC.Source = colorBitmap;
                visorI.Source = colorBitmap;
                visorP.Source = colorBitmap;

                kinectS.AllFramesReady += kinectS_AllFramesReady;

                try
                {
                    kinectS.Start();
                }
                catch (Exception)
                {
                    kinectS = null;
                    MessageBox.Show("OPS! algo salio mal", "Notificación", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Shutdown(-1); //Cerrar la aplicación

                }


            }
        }

        void kinectS_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (kinectS != null)
            {
                
                kinectS.Stop();

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
                    Application.Current.Shutdown(-1); //Cerrar la aplicación
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
