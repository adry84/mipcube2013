using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VideoSwitcher
{
	class MyPipeline: UtilMPipeline {
        protected int  nframes;
        protected bool device_lost;
		private MainWindow window;
		PXCMFaceAnalysis faceAnalysis;
		PXCMFaceAnalysis.Detection detector;
		public bool shouldQuit;

        public MyPipeline( MainWindow w):base() {
            EnableGesture();
			EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH);
			EnableFaceLocation();
            nframes=0;
            device_lost = false;
			window = w;
	    }

	    public override void OnGesture(ref PXCMGesture.Gesture data) {
		    if (data.active) 
				Trace.WriteLine("OnGesture("+data.label+")");
	    }
        public override bool OnDisconnect()
        {
            if (!device_lost) Trace.WriteLine("Device disconnected");
            device_lost = true;
            return base.OnDisconnect();
        }
        public override void OnReconnect()
        {
            Trace.WriteLine("Device reconnected");
            device_lost = false;
        }
	    public override bool OnNewFrame() 
		{
			if( faceAnalysis == null)
			{
				faceAnalysis = QueryFace();
				detector = (PXCMFaceAnalysis.Detection)faceAnalysis.DynamicCast( PXCMFaceAnalysis.Detection.CUID);
			}

			int fid;
			ulong timeStamp;
			if( faceAnalysis.QueryFace( 0, out fid, out timeStamp) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
			{
				PXCMFaceAnalysis.Detection.Data data;
				detector.QueryData( fid, out data);
			
				if( data.rectangle.x < 10000 && data.rectangle.y < 10000)
				{
					uint facex = data.rectangle.x+data.rectangle.w/2;
					uint facey = data.rectangle.y+data.rectangle.h/2;

					long depth;

					var image = QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH);
					PXCMImage.ImageData imgData;
					image.AcquireAccess( PXCMImage.Access.ACCESS_READ, out imgData);
					unsafe {
						short* ar = (short*)imgData.buffer.planes[0];
						depth = GetFilteredDepth( ar, data.rectangle.x/2, data.rectangle.y/2, data.rectangle.w/2, data.rectangle.h/2, image.imageInfo.width);
					//	Trace.WriteLine( depth);
					}
					image.ReleaseAccess( ref imgData);

					window.FacePosition( facex, facey, depth, data.rectangle.w);
				}
			}

			return !shouldQuit;

			//PXCMGesture gesture = QueryGesture();
			//PXCMGesture.GeoNode ndata;
			//pxcmStatus sts = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, out ndata);
			//if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
			//{
			//	window.HandPosition( ndata.positionImage.x, ndata.positionImage.y);
			//	Trace.WriteLine("node HAND_MIDDLE (" + ndata.positionImage.x +"," +ndata.positionImage.y+")");
			//}
			//return (++nframes<50000);
	    }

		private unsafe long GetFilteredDepth(short* ar, uint x, uint y, uint w, uint h, uint linew)
		{
			//x += w/2;
			//y += h/2;

			return ar[x+(y)*linew];

			int d = 10, count = 0;
			long sum = 0;
			for( int j = -d; j < d; j++)
				for( int i = -d; i < d; i++)
				{
					short value = ar[i+x+(j+y)*linew];
					if( value < 2000)
					{
						sum += value;
						count++;
					}
				}
			sum /= (long)count;
			Trace.WriteLine( sum);
			return sum;
		}
    };
}
