using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace VideoSwitcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 														  


    
    public partial class MainWindow : Window
    {
        [DllImport("user32")]
        private static extern IntPtr GetDC(IntPtr Handle);

        [DllImport("user32")]
        private static extern int ReleaseDC(IntPtr Handle, IntPtr DeviceContext);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Red;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Green;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Blue;
        }

        [DllImport("gdi32")]
        private static extern int GetDeviceGammaRamp(IntPtr DeviceContext, ref RAMP lpv);

        [DllImport("gdi32")]
        private static extern int SetDeviceGammaRamp(IntPtr DeviceContext, ref RAMP lpv);


        MediaElement active;
        List<MediaElement> players;
        Thread thr;
        MyPipeline pipeline;

        List<MovieInfo> movies;
		List<List<MovieInfo>> movieSets = new List<List<MovieInfo>>();
		int movieSetIndex = 0;
        byte currentBrightness = 0;
       
        List<Ellipse> ellipses;
        //String path = @"file://C:\work\MIPCUBE\v\";
        String path = @"file://C:\Users\Adry\Desktop\mipcube\";

        bool fullscreen = false;
        private static bool initializedBrightness = false;
        bool infoDisplaying = false;


       
        
       
        private static Int32 hdc;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			movies = new List<MovieInfo>() {
				new MovieInfo() { url = path+@"5943E07C5CE05CEDC9BFE476425B7004_bemyapp_copy_s3.mp4", x = 0, y = 0, name = "middle front" }, 
				new MovieInfo() { url = path+@"0EC6B6B943465CC1AB83BE6D040EF630_bemyapp_copy_s3.mp4", x = 0, y = -1, name = "middle back" }, 
				new MovieInfo() { url = path+@"9C7DA9935B54648C49BB4D60C5B3E555_bemyapp_copy_s3.mp4", x = 1, y = 0, name = "left front" }, 
				new MovieInfo() { url = path+@"313146D7BE7278CE151AF53EE2934746_bemyapp_copy_s3.mp4", x = 1, y = 1, name = "left very front" }, 
			};
			movieSets.Add( movies);

			movies = new List<MovieInfo>() {
				new MovieInfo() { url = path+@"4p-c0.avi", x = -0.5, y = -0.5, name = "bottom right" }, 
				new MovieInfo() { url = path+@"4p-c1.avi", x = 0.5, y = -0.5, name = "bottom left" }, 
				new MovieInfo() { url = path+@"4p-c2.avi", x = 0.5, y = 0.5, name = "top left" }, 
				new MovieInfo() { url = path+@"4p-c3.avi", x = -0.5, y = 0.5, name = "top right" }, 
			};
			movieSets.Add( movies);

			LoadMovieSet( 0);
          
            InitBrightness();
            
			thr = new Thread( Pipeline);
			thr.Start();
        }

		private void LoadMovieSet(int index)
		{
			movieSetIndex = index;
			movies = movieSets[index];

			active = null;

			if( players != null)
			{
				foreach( var player in players)
					mainGrid.Children.Remove( player);
				foreach( var ellipse in ellipses)
					mainGrid.Children.Remove( ellipse);
			}

			players = new List<MediaElement>();
			ellipses = new List<Ellipse>();
			foreach( var movie in movies)
			{
				MediaElement player = new MediaElement();
				player.Visibility = System.Windows.Visibility.Hidden;
				player.Width = mainGrid.RenderSize.Width;
				player.Height = mainGrid.RenderSize.Height;
				player.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
				player.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
				player.LoadedBehavior = MediaState.Manual;
				player.MediaEnded += player_MediaEnded;
				player.MediaOpened += MediaElement1_MediaOpened;
				player.Source = new Uri( movie.url);

				mainGrid.Children.Add( player);

				players.Add( player);

				Ellipse el = new Ellipse();
				el.Width = 10;
				el.Height = 10;
				el.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
				el.VerticalAlignment = System.Windows.VerticalAlignment.Top;
				el.Margin = TransformPos( movie.x, movie.y);	  
				el.Fill = new SolidColorBrush( Color.FromRgb( 0, 255, 0));

				mainGrid.Children.Add( el);

				ellipses.Add( el);

				Panel.SetZIndex( el, 10);
			}

			SwitchVideo(0);
		}

		void player_MediaEnded(object sender, RoutedEventArgs e)
		{
			((MediaElement)sender).Position = TimeSpan.Zero;
			((MediaElement)sender).Play();
		}

        public void Pipeline()
        {
            pipeline = new MyPipeline(this);
            if (!pipeline.LoopFrames())
                Trace.WriteLine("Failed to initialize or stream data");
            pipeline.Dispose();
        }

        void MediaElement1_MediaOpened(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Loaded movie");
        }

        public void PlayAllVideo()
        {
            // play all
            foreach (var p in players)
            {
                p.IsMuted = true;
                p.Play();
            }
            active = players[0];
        }



/// VIDEO ACTIONS


        public void displayInfo()
        {
            if (!infoDisplaying)
            {
                //TODO SHOW SOME LABEL OVER THE VIDEO
            }
            else
            {
                //TODO MASK SOME LABEL
            }
            infoDisplaying = !infoDisplaying;
        }

        public void changeFullscreen()
        {
         

			Dispatcher.Invoke((Action)(() =>
            {
				if (fullscreen == false)
				{
					this.WindowStyle = WindowStyle.None;
					this.WindowState = WindowState.Maximized;
				}
				else 
				{
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;         
                }


                foreach (MediaElement player in players)
                {
                  
                    player.Width = mainGrid.RenderSize.Width;
                    player.Height = mainGrid.RenderSize.Height;

                }


                fullscreen = !fullscreen;

			}));
        }


        public void ChangeVolume(int direction)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (active == null)
					return;

				double v = active.Volume;
                if (direction == 1)
                {
                    v += 1;
                }
                else if (v > 0)
                {
                    v -= 1;
                }
                foreach (var p in players)
                {
                    Trace.WriteLine("change volume " + v);
                    p.Volume = v;

                }
            }));
        }

        // Change the speed of the media. 
        public void ChangeMediaSpeedRatio(int val)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (active == null)
					return;
               

                    double v = active.SpeedRatio;
                    if (val>0 )
                    {
                        v += val;
                    }
                    else if (v > 0)
                    {
                        v -= val;
                    }
                    foreach (var p in players)
                    {
                        Trace.WriteLine("change speed " + v);
                        p.SpeedRatio = (double)v;
                    }
                
            }));
        }

        private void SwitchVideo(int id)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (active == null)
                {
                    PlayAllVideo(); 
					active.Visibility = System.Windows.Visibility.Visible;
					ellipses[id].Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                }
                if (active == players[id])
                    return;

                Trace.WriteLine("Switched to " + movies[id].name);

                active = players[id];

                //active.Position = pos;
                active.Visibility = System.Windows.Visibility.Visible;
                //active.Play();
                active.IsMuted = false;

                foreach (var p in players)
                    if (p != active)
                    {
                        //p.Pause();
                        p.IsMuted = true;
                        p.Visibility = System.Windows.Visibility.Hidden;
                    }

                foreach (var el in ellipses)
                {
                    el.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
                ellipses[id].Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            }));
        }

        public void stopVideoActive()
        {
            if (active == null)
            {
                PlayAllVideo();
            }
            active.Stop();
        }

        public void playVideoActive()
        {
            if (active == null)
            {
                PlayAllVideo();
            }
            active.Play();
        }


        public  void InitBrightness()
        {
            
            IntPtr gammaDC = GetDC(IntPtr.Zero);
            if (gammaDC != System.IntPtr.Zero)
            {
                RAMP gammaArray = new RAMP();
                GetDeviceGammaRamp(gammaDC, ref gammaArray);
                currentBrightness = (byte)(gammaArray.Red[1] - 128);
            }
            ReleaseDC(IntPtr.Zero, gammaDC);
           
        }

        public static void SetBrightness(byte Brightness)
        {
            IntPtr gammaDC = GetDC(IntPtr.Zero);
            if (gammaDC != System.IntPtr.Zero)
            {
                RAMP gammaArray = new RAMP();
                gammaArray.Red = new ushort[256];
                gammaArray.Green = new ushort[256];
                gammaArray.Blue = new ushort[256];
                for (int i = 0; i < 256; i++)
                    gammaArray.Red[i] = gammaArray.Green[i] = gammaArray.Blue[i] = (ushort)Math.Min(i * (Brightness + 128), ushort.MaxValue);
                SetDeviceGammaRamp(gammaDC, ref gammaArray);
            }
            ReleaseDC(IntPtr.Zero, gammaDC);
        }

        

       
/// END VIDEO ACTIONS

    
 #region Event Handlers

        public void Play_Click(object sender, RoutedEventArgs e)
        {
            if (active == null)
            {
                PlayAllVideo();
            }
            active.Play();
        }

        public void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (active == null)
            {
                PlayAllVideo();
            }
            active.Stop();
        }

        public void Forward_Click(object sender, RoutedEventArgs e)
        {
           
            ChangeMediaSpeedRatio(10);
        }

        public void Rewind_Click(object sender, RoutedEventArgs e)
        {
            ChangeMediaSpeedRatio(-10);
        }


        public void BrightM_Click(object sender, RoutedEventArgs e)
        {

            if (currentBrightness > 0)
                currentBrightness--;

            Trace.WriteLine("currentBrightness : " + currentBrightness);
            SetBrightness(currentBrightness);
        }
        public void BrightP_Click(object sender, RoutedEventArgs e)
        {

            if (currentBrightness < 255)
                currentBrightness++;
            Trace.WriteLine("currentBrightness : " + currentBrightness);
            SetBrightness(currentBrightness);
        }
        public void Bright_Click(object sender, RoutedEventArgs e)
        {
           
            if (currentBrightness < 255)
                currentBrightness++;
            else if(currentBrightness>0)
                currentBrightness--;

            Trace.WriteLine("currentBrightness : " + currentBrightness);
            SetBrightness(currentBrightness);
        }

        public void Screen_Click(object sender, RoutedEventArgs e)
        {
            changeFullscreen();
        }

#endregion


        public void HandPosition(float x, float y)
        {

        }

        public void FacePosition(float x, float y, long depth, uint size)
        {
            Point p = ComputeFacePosition( x, y, depth, size);

			//Trace.WriteLine( p.X + " " + p.Y);
			Dispatcher.Invoke( (Action)(() => {
				Me.Margin = TransformPos( p.X, p.Y);
			}));

			int minIndex = 0;
			double minDist = 9999999;
			for( int i = 0; i < movies.Count; i++)
				if( Dist( movies[i], p) < minDist)
				{
					minDist = Dist( movies[i], p);
					minIndex = i;
				}

			SwitchVideo( minIndex);
        }

        private Thickness TransformPos(double x, double y)
		{
			 return new Thickness( mainGrid.RenderSize.Width-120 + -x * 100, mainGrid.RenderSize.Height-120 + -y * 100, 0, 0);
		}

		private Point ComputeFacePosition(double x, double y, long depth, uint size)
		{
			// depth: 300 is close to screen, 600 is far

			double result_y1 = 1-(depth-250.0)/200;
			double result_y2 = -300.0/size+2;
			double result_y = result_y1*0 + result_y2*1;

			return new Point( (x/640-0.5)*2, result_y);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			foreach( var p in players)
			{
				p.Stop();
			}
			pipeline.shouldQuit = true;
			Thread.Sleep(1000);
			thr.Abort();
		}

        public double Dist(MovieInfo info, Point p)
        {
            return Math.Sqrt((p.X - info.x) * (p.X - info.x) + (p.Y - info.y) * (p.Y - info.y));
        }

		internal void PrevMovieSet()
		{
			movieSetIndex--;
			if( movieSetIndex < 0)
				movieSetIndex = movieSets.Count-1;

			Dispatcher.Invoke( (Action)(() => {
				LoadMovieSet( movieSetIndex);
			}));
		}

		internal void NextMovieSet()
		{
			movieSetIndex++;
			if( movieSetIndex == movieSets.Count)
				movieSetIndex = 0;

			Dispatcher.Invoke( (Action)(() => {
				LoadMovieSet( movieSetIndex);
			}));
		}

		private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			foreach( var p in players)
				p.Volume += e.Delta / 1000.0;
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if( players != null)
			{
				foreach( var p in players)
				{
					p.Width = mainGrid.RenderSize.Width;
					p.Height = mainGrid.RenderSize.Height;
				}

				for( int i = 0; i < movies.Count; i++)
				{
					Ellipse el = ellipses[i];
					el.Margin = TransformPos( movies[i].x, movies[i].y);
				}
			}
		}

		private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			changeFullscreen();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if( fullscreen && e.Key == Key.Escape)
				changeFullscreen();
		}

        private void VolP_Click(object sender, RoutedEventArgs e)
        {
            ChangeVolume(1);
        }
        private void VolM_Click(object sender, RoutedEventArgs e)
        {
            ChangeVolume(0);
        }
    }
}