using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Slime
{
    public class ChromaKey : GH_Component
    {

        public ChromaKey()
          : base("Chroma Key", "Chroma Key",
              "Remove color from bitmap, creating alpha areas.",
              "Slime", "Bitmap")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "The source bitmap as input", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "C", "Target color for chroma keying", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "t", "Tolerance for", GH_ParamAccess.item,0);
            pManager.AddBooleanParameter("Inverse", "I", "True for removing target color. False for keeping target color only.", GH_ParamAccess.item,true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "Bitmap result after chroma key", GH_ParamAccess.item);
        }
        Bitmap img;
        Color key;
        Double tolerance = 0;
        bool isInversed;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref img)) return;
            if (!DA.GetData(1, ref key)) return;
            if (!DA.GetData(2, ref tolerance)) return;
            DA.GetData(3, ref isInversed);

            Bitmap output = new Bitmap(img.Width, img.Height);

            for (int y = 0; y < output.Height; y++)
            {
                for (int x = 0; x < output.Width; x++)
                {
                    Color pixelColor = img.GetPixel(x, y);
                    if ((Math.Abs(pixelColor.R-key.R)+ Math.Abs(pixelColor.G - key.G)+ Math.Abs(pixelColor.B - key.B))/3 <= tolerance){ 
                        pixelColor = Color.Transparent;
                    }
                    output.SetPixel(x, y, pixelColor);
                }
            }

            DA.SetData(0, output);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Slime.Properties.Resources.ChromaKeyIcon;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("b48117cd-a9c5-462a-a986-af75da64f8cd"); }
        }
    }
}