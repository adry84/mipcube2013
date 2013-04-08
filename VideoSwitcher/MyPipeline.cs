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
            //EnableGesture();
			EnableFaceLocation();
            nframes=0;
            device_lost = false;
			window = w;
	    }

	    public override void OnGesture(ref PXCMGesture.Gesture data) {
		    if (data.active) Trace.WriteLine("OnGesture("+data.label+")");
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
			//	Trace.WriteLine( data.rectangle.x+data.rectangle.w/2);
				window.FacePosition( data.rectangle.x+data.rectangle.w/2, data.rectangle.y+data.rectangle.h/2);
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
    };
}
