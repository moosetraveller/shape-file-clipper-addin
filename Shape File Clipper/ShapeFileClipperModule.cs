using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Shape_File_Clipper
{
    internal class ShapeFileClipperModule : Module
    {
        private static ShapeFileClipperModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static ShapeFileClipperModule Current
        {
            get
            {
                return _this ?? (_this = (ShapeFileClipperModule)FrameworkApplication.FindModule("Shape_File_Clipper_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
