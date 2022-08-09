using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Slime
{
    public class Inverse : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Inverse class.
        /// </summary>
        public Inverse()
          : base("Inverse", "Inverse",
              "Inverse color of bitmap",
              "Slime", "Bitmap")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "The source bitmap as input", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "Bitmap result after chroma key", GH_ParamAccess.item);
        }
        Bitmap img;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref img)) return;

            Bitmap output = new Bitmap(img.Width, img.Height);

            for (int y = 0; y < output.Height; y++)
            {
                for (int x = 0; x < output.Width; x++)
                {
                    Color pixelColor = img.GetPixel(x, y);
                    Color inversedColor = Color.FromArgb(pixelColor.A, 255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);
                    output.SetPixel(x, y, inversedColor);
                }
            }

            DA.SetData(0, output);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Slime.Properties.Resources.InverseIcon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("44b6b56b-db7d-4893-aedc-6d942839a978"); }
        }
    }
}