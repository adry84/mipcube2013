﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
	public partial class MainWindow : Window
	{
		MediaElement active;
		List<MediaElement> players;
		Thread thr;
		MyPipeline pipeline; 

		List<MovieInfo> movies;

		List<Ellipse> ellipses;
        //String path = @"file://C:\work\MIPCUBE\v\";
        String path = @"file://C:\Users\Adry\Desktop\mipcube\";

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

			players = new List<MediaElement>();
			ellipses = new List<Ellipse>();
			foreach( var movie in movies)
			{
				MediaElement player = new MediaElement();
				player.Visibility = System.Windows.Visibility.Hidden;
				player.Height = this.Height;
				player.Width = this.Width;
				player.LoadedBehavior = MediaState.Manual;
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
            thr = new Thread( Pipeline);
			thr.Start();
		}

		public void Pipeline()
		{
			pipeline=new MyPipeline(this);
            if (!pipeline.LoopFrames())
                Trace.WriteLine("Failed to initialize or stream data");
            pipeline.Dispose();
		}

		void MediaElement1_MediaOpened(object sender, RoutedEventArgs e)
		{
			Trace.WriteLine( "Loaded movie");
		}

        public void ChangeVolume(int direction)
        {
            Dispatcher.Invoke( (Action)(() => {
            if (active == null)
            {
                // play all
                foreach (var p in players)
                {
                    p.IsMuted = true;
                    p.Play();
                }
                active = players[0];
            }
            else
            {
                double v = active.Volume;
                if (direction == 1)
                {
                    v+=100;
                }
                else if(v>0)
                {
                    v--;
                }
                foreach (var p in players)
                {
                    Trace.WriteLine("change volume "+v);
                    p.Volume = v;
                   
                }
            }
            }));
        }
		private void SwitchVideo( int id)
		{
			Dispatcher.Invoke( (Action)(() => {
				if( active == null)
				{
					// play all
					foreach( var p in players)
					{
						p.IsMuted = true;
						p.Play();
					}
				}
				if( active == players[id])
					return;

				Trace.WriteLine( "Switched to " + movies[id].name);

				active = players[id];

				//active.Position = pos;
				active.Visibility = System.Windows.Visibility.Visible;
				//active.Play();
				active.IsMuted = false;

				foreach( var p in players)
					if( p != active)
					{
						//p.Pause();
						p.IsMuted = true;
						p.Visibility = System.Windows.Visibility.Hidden;
					}

				foreach( var el in ellipses)
				{
					el.Fill = new SolidColorBrush( Color.FromRgb( 0, 255, 0));
				}
				ellipses[id].Fill = new SolidColorBrush( Color.FromRgb( 0, 0, 255));
			}));
		}

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
			 return new Thickness( this.Width/2 + -x * 100, this.Height/2 + -y * 100, 0, 0);
		}

		private Point ComputeFacePosition(double x, double y, long depth, uint size)
		{
			// depth: 300 is close to screen, 600 is far

			double result_y = 1-(depth-250.0)/200;
			result_y = -300.0/size+2;

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

		public double Dist( MovieInfo info, Point p)
		{
			return Math.Sqrt( (p.X-info.x)*(p.X-info.x) + (p.Y-info.y)*(p.Y-info.y));
		}
	}
}
