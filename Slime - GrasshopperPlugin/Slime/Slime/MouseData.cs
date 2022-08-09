using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using Rhino.Display;

namespace Slime
{
    public class MouseData : GH_Component
    {

        public MouseData()
          : base("Mouse Data", "Mouse Data",
              "Get mouse data in current viewport(auto update).",
              "Slime", "User")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Trace", "T", "Trace of mouse in current viewport, connecting current frame with last frame.", GH_ParamAccess.item);
        }
 


        Point3d lastPoint;
        bool initialized = false;


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            System.Drawing.Point screenPosition = System.Windows.Forms.Cursor.Position;

            RhinoView view = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView;
            System.Drawing.Point point = view.ActiveViewport.ScreenToClient(screenPosition);

            Line line = view.ActiveViewport.ClientToWorld(point);
            Plane viewportPlane = view.ActiveViewport.ConstructionPlane();
            double t;

            Rhino.Geometry.Intersect.Intersection.LinePlane(line, viewportPlane, out t);

            if (!initialized)
            {
                lastPoint = line.PointAt(t);
                initialized = true;
            }

            DA.SetData(0, new Line(lastPoint, line.PointAt(t)));

            lastPoint = line.PointAt(t);
            base.ExpireSolution(true);
        }



        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Slime.Properties.Resources.MouseDataIcon;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("086f53c7-99e4-4282-aaa8-8a671069ee16"); }
        }
    }
}