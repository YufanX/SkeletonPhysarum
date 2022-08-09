using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel.Geometry;



namespace Slime
{
    public class SkeletonPhysarum2DComponent : GH_Component
    {

        public SkeletonPhysarum2DComponent()
          : base("Skeleton Physarum 2D", "Skeleton Physarum 2D",
              "Skeleton-based 2D Physarum system, use timer to trigger, button reset to reset. Output as a bunch of lines(duplication uncleaned)",
              "Slime", "Generation")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "Bitmap", "The source bitmap as input", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh for extracting edge from image, this affects Skeleton result resolution", GH_ParamAccess.item);
            pManager.AddRectangleParameter("Boundary", "Boundary", "Boundary, region to compute", GH_ParamAccess.item, new Rectangle3d());
            pManager.AddNumberParameter("Threshold", "Threshold", "Threshold 0~1 for edge detection, dafault as 0.5", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Edge Division Length", "Edge Division Length", "Length for dividing detected edge(Rhino Unit), this affects Skeleton result resolution", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("Expansion Radius", "Expansion Radius", "Radius of skeleton expansion(Rhino Unit), also used as maximum segment length for filtering skeletonized result(Rhino Unit), this affects Skeleton connectivity", GH_ParamAccess.item, 25);
            pManager.AddTransformParameter("Transform(X)", "Transform(X)", "Transform input for each frame", GH_ParamAccess.list);
            pManager[6].Optional = true;
            pManager.AddLineParameter("Vector Feed", "Vector Feed", "Vector input(Lines) feeding each iteration. *Note: flatten data.", GH_ParamAccess.list);
            pManager[7].Optional = true;
            pManager.AddGenericParameter("Pixel Feed", "Pixel Feed", "Pixel(bitmap) input feeding each iteration, white as background, black as foreground, alpha as ignore.", GH_ParamAccess.item);
            pManager[8].Optional = true;
            pManager.AddBooleanParameter("Inverse?", "Inverse?", "Inverse foreground and background. False for skeletonizing white, redrawing black. True for skeletonizing black, redrawing white", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset","Reset","Reset solution",GH_ParamAccess.item, false);
            
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "Line Output", GH_ParamAccess.list);
        }

        Bitmap sourceImg;
        Bitmap iterImg;
        Bitmap constantPixelFeed;
        Graphics canvas;
        int rawImageWidth;
        int rawImageHeight;
        List<Line> lines;

        Pen pen;
        int counter = 0;

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh baseMesh = new Mesh();
            Rectangle3d bound = new Rectangle3d();
            double threshold = 0.5;
            double edgeDivisionLength = 4;
            double strokeSize = 24;
            List<Transform> transforms = new List<Transform>();
            List<Line> constantVectorFeed = new List<Line>();
            constantPixelFeed = null;

            bool isInversed = false;
            bool reset = false;

            if (!DA.GetData(0, ref sourceImg)) return;
            if (!DA.GetData(1, ref baseMesh)) return;
            if (!DA.GetData(2, ref bound)) return;
            if (!DA.GetData(3, ref threshold)) return;
            if (!DA.GetData(4, ref edgeDivisionLength)) return;
            if (!DA.GetData(5, ref strokeSize)) return;
            DA.GetDataList(6, transforms);
            DA.GetDataList(7, constantVectorFeed);
            DA.GetData(8, ref constantPixelFeed);
            DA.GetData(9, ref isInversed);
            DA.GetData(10, ref reset);

            if (counter == 0)
            {
                rawImageWidth = sourceImg.Width;
                rawImageHeight = sourceImg.Height;

                iterImg = sourceImg.Clone(new Rectangle(0, 0, rawImageWidth, rawImageHeight), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                constantPixelFeed = null;
                canvas = Graphics.FromImage(iterImg);
                lines = new List<Line>();
            }
            counter++;

            if (reset)
            {
                counter = 0;
            }

            if (isInversed)
            {
                pen = new Pen(Color.Black, Convert.ToSingle(rawImageWidth * strokeSize / bound.Width));
            }
            else {
                pen = new Pen(Color.White, Convert.ToSingle(rawImageWidth * strokeSize / bound.Width));
            }
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;




            lines.Clear();

            try
            {
                Skeletonize(threshold, iterImg, baseMesh, bound, edgeDivisionLength, strokeSize, isInversed, ref lines);

                if (isInversed)
                {
                    canvas.Clear(Color.White);

                }
                else
                {
                    canvas.Clear(Color.Black);
                }

                foreach (Line l in lines)
                {
                    foreach (Transform x in transforms)
                    {
                        l.Transform(x);
                    }

                    canvas.DrawLine(pen, Convert.ToInt32(rawImageWidth * (l.From.X - bound.X.Min) / bound.Width), Convert.ToInt32(rawImageHeight * (1 - (l.From.Y - bound.Y.Min) / bound.Height)), Convert.ToInt32(rawImageWidth * (l.To.X - bound.X.Min) / bound.Width), Convert.ToInt32(rawImageHeight * (1 - (l.To.Y - bound.Y.Min) / bound.Height)));
                }

               


            }
            catch (Exception exMsg)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, exMsg.Message+"......Data is empty");
            }

            if (transforms.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Additional Transform Data is empty");
            }

            if (constantVectorFeed.Count != 0)
            {
                foreach (Line l in constantVectorFeed)
                {
                    canvas.DrawLine(pen, Convert.ToInt32(rawImageWidth * (l.From.X - bound.X.Min) / bound.Width), Convert.ToInt32(rawImageHeight * (1-(l.From.Y - bound.Y.Min) / bound.Height)), Convert.ToInt32(rawImageWidth * (l.To.X - bound.X.Min) / bound.Width), Convert.ToInt32(rawImageHeight * (1 - (l.To.Y - bound.Y.Min) / bound.Height)));
                }
            }else{AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Addtional Vector Data is empty"); }

            //Overlay environment image
            if (constantPixelFeed != null)
            {
                canvas.DrawImage(constantPixelFeed, new Rectangle(0, 0, rawImageWidth, rawImageHeight));
            }else{ AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Addtional Pixel Data is empty"); }


            DA.SetDataList(0,lines);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return ;
                return Slime.Properties.Resources.SkeletonPhysarum2DIcon;
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
                Color rawColor = sourceImg.GetPixel(Math.Min(Convert.ToInt32((pt.X / r.Width) * sourceImg.Width), sourceImg.Width - 1), Math.Min(Convert.ToInt32((1 - (pt.Y / r.Height)) * sourceImg.Height), sourceImg.Height - 1));
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
                    var pMid = sourceImg.GetPixel(Convert.ToInt32((0.5 * (l.From.X + l.To.X) / r.Width) * (sourceImg.Width - 1)), Math.Min(Convert.ToInt32((1 - (0.5 * (l.From.Y + l.To.Y) / r.Height)) * sourceImg.Height), sourceImg.Height - 1));
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

        public override Guid ComponentGuid
        {
            get { return new Guid("eee25a8c-3754-4cfe-b70e-f9afe8b488c2"); }
        }
    }
}
