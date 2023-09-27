using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatePrefabricatedBeams.Models
{
    public class PrefabricatedBeam
    {
        public FamilyInstance BeamInstance { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }

        public PrefabricatedBeam(FamilyInstance beamInstance, double height, double width)
        {
            BeamInstance = beamInstance;
            Height = height;
            Width = width;
        }

        public Line GetLocationLine(Curve directionCurve)
        {
            var beamLocation = BeamInstance.Location as LocationCurve;
            var beamCurve = beamLocation.Curve as Line;
            double zCoord = directionCurve.GetEndPoint(0).Z;

            XYZ startPoint = new XYZ(beamCurve.GetEndPoint(0).X, beamCurve.GetEndPoint(0).Y, zCoord);
            XYZ endPoint = new XYZ(beamCurve.GetEndPoint(1).X, beamCurve.GetEndPoint(1).Y, zCoord);

            var beamUnboundLine = Line.CreateUnbound(startPoint, (endPoint - startPoint));
            var projectResult = beamUnboundLine.Project(directionCurve.GetEndPoint(0));
            var projectPoint = projectResult.XYZPoint;
            double beamWidthInternalUnits = UnitUtils.ConvertToInternalUnits(Width, UnitTypeId.Millimeters);
            XYZ offsetDirection = (directionCurve.GetEndPoint(0) - projectPoint).Normalize() * beamWidthInternalUnits / 2;

            var beamLocationInDirectionLinePlane = Line.CreateBound(startPoint + offsetDirection, endPoint + offsetDirection);

            return beamLocationInDirectionLinePlane;
        }
    }
}
