using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel.Geometry;

namespace Slime
{
    public class Skeletonization2D : GH_Component
    {

        public Skeletonization2D()
          : base("Skeletonization2D", "Skeletonization2D",
              "Voronoi Based Skeletonization Solution",
              "Slime", "Subtraction")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "Bitmap", "The source bitmap as input", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh for extracting edge from image, this affects Skeleton result resolution", GH_ParamAccess.item);
            pManager.AddRectangleParameter("Boundary", "Boundary", "Boundary, region to compute", GH_ParamAccess.item, new Rectangle3d());
            pManager.AddNumberParameter("Threshold", "Threshold", "Threshold 0~1 for edge detection, dafault as 0.5", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Edge Division Length", "Edge Division Length", "Length for dividing detected edge(Rhino Unit), this affects Skeleton result resolution", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("Maximum Skeleton Length", "Maximum Skeleton Length", "Length for filtering skeletonized result(Rhino Unit), this affects Skeleton connectivity", GH_ParamAccess.item, 5);
            pManager.AddBooleanParameter("Inverse", "Inverse", "Inverse foreground and background. False for skeletonizing white. True for skeletonizing black", GH_ParamAccess.item, false);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "Line Output", GH_ParamAccess.list);
        }

        Bitmap img;
        List<Line> lines;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh baseMesh = new Mesh();
            Rectangle3d bound = new Rectangle3d();
            double threshold = 0.5;
            double edgeDivisionLength = 5;
            double maxSkeletonLength = 5;
            bool isInversed = false;

            if (!DA.GetData(0, ref img)) return;
            if (!DA.GetData(1, ref baseMesh)) return;
            if (!DA.GetData(2, ref bound)) return;
            if (!DA.GetData(3, ref threshold)) return;
            if (!DA.GetData(4, ref edgeDivisionLength)) return;
            if (!DA.GetData(5, ref maxSkeletonLength)) return;
            DA.GetData(6, ref isInversed);


            lines = new List<Line>();

            Skeletonize(threshold, img, baseMesh, bound, edgeDivisionLength, maxSkeletonLength, isInversed, ref lines);

            DA.SetDataList(0, lines);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Slime.Properties.Resources.Skeletonization2DIcon;
            }
        }
        public void Skeletonize(double threshold0, Bitmap sourceImg0, Mesh baseMesh0, Rectangle3d r0, double edgeSegmentLength0, double maxSkeletonLength0, bool inverse0, ref List<Line> result)
        {
            double threshold = threshold0;
            Bitmap sourceImg = sourceImg0;
            Mesh baseMesh = baseMesh0;
            Rectangle3d r = r0;
            double edgeSegmentLength = edgeSegmentLength0;
            double maxSkeletonLength = maxSkeletonLength0;
            bool inverse = inverse0;

            List<Point3d> extractedPoints = new List<Point3d>();
            Point3d[] rectCorners = r.BoundingBox.GetCorners();
            Rhino.Geometry.Plane cutter = new Rhino.Geometry.Plane(new Point3d(0, 0, threshold), new Vector3d(0, 0, 1));

            Polyline[] edges;

            for (int i = 0; i < baseMesh.Vertices.Count; i++)
            {
                var pt = baseMesh.Vertices[i];
                Color rawColor = sourceImg.GetPixel(Math.Min(Convert.ToInt32((pt.X / r.Width) * sourceImg.Width), sourceImg.Width - 1), Math.Min(Convert.ToInt32((1-(pt.Y / r.Height)) * sourceImg.Height), sourceImg.Height - 1));
                baseMesh.Vertices.SetVertex(i, pt.X, pt.Y, rawColor.GetBrightness());
            }
 
            edges = Rhino.Geometry.Intersect.Intersection.MeshPlane(baseMesh, cutter);

            List<Node2> unit = new List<Node2>();
            Node2List nodes = new Node2List();

            for (int i = 0; i < edges.Length; i++)
            {
                var edgesCrv = edges[i].ToPolylineCurve();
                for (int j = 0; j < (edgesCrv.Domain.Max - edgesCrv.Domain.Min) / edgeSegmentLength; j++)
                {
                    var tpt = edgesCrv.PointAt(j * edgeSegmentLength);
                    unit.Add(new Node2(tpt.X, tpt.Y));
                }
            }

            foreach (Node2 a in unit)
            {
                nodes.Append(a);
            }

            Node2List outline = new Node2List();
            foreach (Point3d p in rectCorners)
            {
                Node2 n = new Node2(p.X, p.Y);
                outline.Append(n);
            }

            var delaunay = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(nodes, 0.1, false);
            var voronoi = Grasshopper.Kernel.Geometry.Voronoi.Solver.Solve_Connectivity(nodes, delaunay, outline);


            List<Line> centralLine = new List<Line>();
            List<Double> vs = new List<Double>();
            List<Double> ve = new List<Double>();
            List<Point3d> ps = new List<Point3d>();
            List<Point3d> pe = new List<Point3d>();
            foreach (var c in voronoi)
            {
                Polyline pl = c.ToPolyline();
                var seg = pl.GetSegments();

                foreach (Line l in seg)
                {
                    var pStart = sourceImg.GetPixel(Convert.ToInt32((l.From.X / r.Width) * (sourceImg.Width - 1)), Math.Min(Convert.ToInt32((1 - (l.From.Y / r.Height)) * sourceImg.Height), sourceImg.Height - 1));
                    var pEnd = sourceImg.GetPixel(Convert.ToInt32((l.To.X / r.Width) * (sourceImg.Width - 1)), Math.Min(Convert.ToInt32((1 - (l.To.Y / r.Height)) * sourceImg.Height), sourceImg.Height - 1));
                    var pMid = sourceImg.GetPixel(Convert.ToInt32((0.5 * (l.From.X + l.To.X) / r.Width) * (sourceImg.Width - 1)), Math.Min(Convert.ToInt32((1 - (0.5 * (l.From.Y + l.To.Y) / r.Height)) *sourceImg.Height), sourceImg.Height - 1));
                    if (inverse) 
                    {
                        if (pStart.GetBrightness() <= threshold && pMid.GetBrightness() <= threshold && pEnd.GetBrightness() <= threshold && l.Length < maxSkeletonLength)
                        {
                            centralLine.Add(l);
                        }
                    }
                    else
                    {
                        if (pStart.GetBrightness() >= threshold && pMid.GetBrightness() >= threshold && pEnd.GetBrightness() >= threshold && l.Length < maxSkeletonLength)
                        {
                            centralLine.Add(l);
                        }
                    }
                }
            }
            result = centralLine;
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("333035a7-3b78-431e-aa8b-bb71707ffe1e"); }
        }
    }
}