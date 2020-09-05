using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Shape_File_Clipper
{
    internal class ShowShapeFileClipper : Button
    {

        private ShapeFileClipper _shapefileclipper = null;

        protected override void OnClick()
        {
            //already open?
            if (_shapefileclipper != null)
                return;
            _shapefileclipper = new ShapeFileClipper();
            _shapefileclipper.Owner = FrameworkApplication.Current.MainWindow;
            _shapefileclipper.Closed += (o, e) => { _shapefileclipper = null; };
            _shapefileclipper.Show();
            //uncomment for modal
            //_shapefileclipper.ShowDialog();
        }

    }
}
