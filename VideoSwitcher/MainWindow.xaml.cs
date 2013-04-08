using System;
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
		MediaElement[] players;
		Thread thr;
		MyPipeline pipeline; 

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MediaElement1.Source = new Uri( @"file://C:\work\MIPCUBE\v\A5606EEBBA3251B5FF34B0E831B21791_bemyapp_copy_s3.mp4");
			MediaElement2.Source = new Uri( @"file://C:\work\MIPCUBE\v\0EC6B6B943465CC1AB83BE6D040EF630_bemyapp_copy_s3.mp4");
			MediaElement3.Source = new Uri( @"file://C:\work\MIPCUBE\v\5943E07C5CE05CEDC9BFE476425B7004_bemyapp_copy_s3.mp4");
			MediaElement4.Source = new Uri( @"file://C:\work\MIPCUBE\v\9C7DA9935B54648C49BB4D60C5B3E555_bemyapp_copy_s3.mp4");
			MediaElement5.Source = new Uri( @"file://C:\work\MIPCUBE\v\313146D7BE7278CE151AF53EE2934746_bemyapp_copy_s3.mp4");

			players = new MediaElement[] { MediaElement1, MediaElement2, MediaElement3, MediaElement4, MediaElement5 };

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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			SwitchVideo( 0);
		}

		private void Button2_Click(object sender, RoutedEventArgs e)
		{
			SwitchVideo( 1);
		}

		private void Button3_Click(object sender, RoutedEventArgs e)
		{
			SwitchVideo( 2);
		}

		private void Button4_Click(object sender, RoutedEventArgs e)
		{
			SwitchVideo( 3);
		}

		private void Button5_Click(object sender, RoutedEventArgs e)
		{
			SwitchVideo( 4);
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

				Trace.WriteLine( "Switched to position " + id);

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
			}));
		}

		public void HandPosition(float x, float y)
		{
			
		}

		public void FacePosition(float x, float y)
		{
			int video = (int)(x/128);
			if( video < 0)
				video = 0;
			if( video > 4)
				video = 4;
			SwitchVideo( video);
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
	}
}
