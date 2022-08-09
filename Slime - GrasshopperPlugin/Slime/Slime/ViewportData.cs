using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino.Display;

namespace Slime
{
    public class ViewportData : GH_Component
    {

        public ViewportData()
          : base("Viewport Data", "Viewport Data",
              "Get data of current active camera data",
              "Slime", "User")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Plane of current viewport camera", GH_ParamAccess.item);
            pManager.AddPointParameter("Target", "T", "Target of current viewport camera", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RhinoViewport viewportActive = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            Rhino.Geometry.Plane cameraPlane = new Rhino.Geometry.Plane(viewportActive.CameraLocation, viewportActive.CameraX, viewportActive.CameraY);
            Point3d target = viewportActive.CameraTarget;
            DA.SetData(0, cameraPlane);
            DA.SetData(1, target);
            base.ExpireSolution(true);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Slime.Properties.Resources.ViewportDataIcon;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("b9bdd8a5-b946-4dbd-b99f-dadbb9cd1ccb"); }
        }
    }
}