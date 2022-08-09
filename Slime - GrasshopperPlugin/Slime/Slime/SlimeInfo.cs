using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Slime
{
    public class SlimeInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Slime";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Slime.Properties.Resources.SlimeLibIcon_24;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Skeletonization and Generative Library developed by Yufan Xie";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("7463c6f9-f1c9-46e6-ae32-60620b4f3619");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Yufan Xie";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "xieyufan@uvnlab.com";
            }
        }
    }
}
