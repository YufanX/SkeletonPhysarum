using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Slime
{
    public class BitmapFromFile : GH_Component
    {

        public BitmapFromFile()
          : base("BitmapFromFile", "BitmapFromFile",
              "Construct bitmap from file",
              "Slime", "Bitmap")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "P", "Image file path", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "Constructed Bitmap", GH_ParamAccess.item);
        }
        Bitmap img;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string file = "empty";
  
            if (!DA.GetData(0, ref file)) return;
            img = new Bitmap(file);

            DA.SetData(0, img);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Slime.Properties.Resources.BitmapFromFileIcon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("22c56e3d-bc7c-49ec-aca5-e6140f34205c"); }
        }
    }
}