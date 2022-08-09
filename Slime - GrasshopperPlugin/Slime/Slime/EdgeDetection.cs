using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Drawing;


namespace Slime
{
    public class EdgeDetection : GH_Component
    {

        public EdgeDetection()
          : base("Edge Detection", "Edge Detection",
              "Extracting edge of image",
              "Slime", "Subtraction")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "The source bitmap as input", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "M", "Mesh for extracting edge from image, this affects resolution", GH_ParamAccess.item);
            pManager.AddRectangleParameter("Boundary", "B", "Boundary, region to compute", GH_ParamAccess.item, new Rectangle3d());
            pManager.AddNumberParameter("Threshold", "t", "Threshold 0~1 for edge detection, dafault as 0.5", GH_ParamAccess.item, 0.5);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Edges", "E", "Subtracted Edges", GH_ParamAccess.list);
        }
        Bitmap img;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh baseMesh = new Mesh();
            Rectangle3d bound = new Rectangle3d();
            double threshold = 0.5;

            if (!DA.GetData(0, ref img)) return;
            if (!DA.GetData(1, ref baseMesh)) return;
            if (!DA.GetData(2, ref bound)) return;
            if (!DA.GetData(3, ref threshold)) return;

            Rhino.Geometry.Plane cutter = new Rhino.Geometry.Plane(new Point3d(0, 0, threshold), new Vector3d(0, 0, 1));
            Polyline[] edges;

            for (int i = 0; i < baseMesh.Vertices.Count; i++)
            {
                var pt = baseMesh.Vertices[i];
                Color rawColor = img.GetPixel(Math.Min(Convert.ToInt32((pt.X / bound.Width) * img.Width), img.Width - 1), Math.Min(Convert.ToInt32((1 - (pt.Y / bound.Height)) * img.Height), img.Height - 1));
                baseMesh.Vertices.SetVertex(i, pt.X, pt.Y, rawColor.GetBrightness());
            }

            edges = Rhino.Geometry.Intersect.Intersection.MeshPlane(baseMesh, cutter);

            DA.SetDataList(0, edges);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Slime.Properties.Resources.EdgeDetectionIcon;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("9cc56ee4-429b-4ba8-996f-76855f410bb3"); }
        }
    }
}