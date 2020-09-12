using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomo.ShapeFileClipper
{
    internal class ShowArcCoordinateSystemSelectionWindow : Button
    {

        private ArcCoordinateSystemSelectionWindow _arccoordinatesystemselectionwindow = null;

        protected override void OnClick()
        {
            //already open?
            if (_arccoordinatesystemselectionwindow != null)
                return;
            _arccoordinatesystemselectionwindow = new ArcCoordinateSystemSelectionWindow();
            _arccoordinatesystemselectionwindow.Owner = FrameworkApplication.Current.MainWindow;
            _arccoordinatesystemselectionwindow.Closed += (o, e) => { _arccoordinatesystemselectionwindow = null; };
            _arccoordinatesystemselectionwindow.Show();
            //uncomment for modal
            //_arccoordinatesystemselectionwindow.ShowDialog();
        }

    }
}
