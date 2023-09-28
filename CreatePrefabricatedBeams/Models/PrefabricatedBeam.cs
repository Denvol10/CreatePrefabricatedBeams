using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
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

        public Line GetNewLocation(Curve directionCurve,
                                   IEnumerable<Line> lineOnRoad1,
                                   IEnumerable<Line> lineOnRoad2,
                                   double roadSurfaceThikness,
                                   double slabThikness, out LocationCurve oldLocation)
        {
            XYZ offsetDirection;
            var lineOnBasePlane = GetLocationLine(directionCurve, out offsetDirection, out oldLocation);
            XYZ normalVector = lineOnBasePlane.GetEndPoint(1) - lineOnBasePlane.GetEndPoint(0);
            Plane startPlane = Plane.CreateByNormalAndOrigin(normalVector, lineOnBasePlane.GetEndPoint(0));
            Plane endPlane = Plane.CreateByNormalAndOrigin(normalVector, lineOnBasePlane.GetEndPoint(1));

            Line startLineOnRoad1 = RevitGeometryUtils.GetIntersectCurve(lineOnRoad1, startPlane);
            Line startLineOnRoad2 = RevitGeometryUtils.GetIntersectCurve(lineOnRoad2, startPlane);

            Line endLineOnRoad1 = RevitGeometryUtils.GetIntersectCurve(lineOnRoad1, endPlane);
            Line endLineOnRoad2 = RevitGeometryUtils.GetIntersectCurve(lineOnRoad2, endPlane);

            XYZ startPointOnRoad1 = RevitGeometryUtils.LinePlaneIntersection(startLineOnRoad1, startPlane, out _);
            XYZ startPointOnRoad2 = RevitGeometryUtils.LinePlaneIntersection(startLineOnRoad2, startPlane, out _);

            XYZ endPointOnRoad1 = RevitGeometryUtils.LinePlaneIntersection(endLineOnRoad1, endPlane, out _);
            XYZ endPointOnRoad2 = RevitGeometryUtils.LinePlaneIntersection(endLineOnRoad2, endPlane, out _);

            XYZ lineDirection1 = startPointOnRoad1 - startPointOnRoad2;
            XYZ lineDirection2 = endPointOnRoad1 - endPointOnRoad2;

            var startAcrossLineOnRoad = Line.CreateUnbound(startPointOnRoad1, lineDirection1);
            var endAcrossLineOnRoad = Line.CreateUnbound(endPointOnRoad1, lineDirection2);

            Line startVerticalLine = Line.CreateUnbound(startPlane.Origin, XYZ.BasisZ);
            Line endVerticalLine = Line.CreateUnbound(endPlane.Origin, XYZ.BasisZ);

            IntersectionResultArray interResult1;
            var compResult1 = startAcrossLineOnRoad.Intersect(startVerticalLine, out interResult1);
            XYZ startPointOnRoad = null;
            if (compResult1 == SetComparisonResult.Overlap)
            {
                foreach (var elem in interResult1)
                {
                    if (elem is IntersectionResult result)
                    {
                        startPointOnRoad = result.XYZPoint;
                    }
                }
            }

            IntersectionResultArray interResult2;
            var compResult2 = endAcrossLineOnRoad.Intersect(endVerticalLine, out interResult2);
            XYZ endPointOnRoad = null;
            if (compResult2 == SetComparisonResult.Overlap)
            {
                foreach (var elem in interResult2)
                {
                    if (elem is IntersectionResult result)
                    {
                        endPointOnRoad = result.XYZPoint;
                    }
                }
            }

            XYZ horisontalOffset = offsetDirection.Negate();
            XYZ verticalOffset = UnitUtils.ConvertToInternalUnits((roadSurfaceThikness
                + slabThikness + Height), UnitTypeId.Millimeters) * XYZ.BasisZ.Negate();

            Line newLocationLine = Line.CreateBound((startPointOnRoad + verticalOffset + horisontalOffset),
                                                    (endPointOnRoad + verticalOffset + horisontalOffset));

            return newLocationLine;
        }

        private Line GetLocationLine(Curve directionCurve, out XYZ offsetDirection, out LocationCurve oldLocation)
        {
            oldLocation = BeamInstance.Location as LocationCurve;
            var beamCurve = oldLocation.Curve as Line;
            double zCoord = directionCurve.GetEndPoint(0).Z;

            XYZ startPoint = new XYZ(beamCurve.GetEndPoint(0).X, beamCurve.GetEndPoint(0).Y, zCoord);
            XYZ endPoint = new XYZ(beamCurve.GetEndPoint(1).X, beamCurve.GetEndPoint(1).Y, zCoord);

            var beamUnboundLine = Line.CreateUnbound(startPoint, (endPoint - startPoint));
            var projectResult = beamUnboundLine.Project(directionCurve.GetEndPoint(0));
            var projectPoint = projectResult.XYZPoint;
            double beamWidthInternalUnits = UnitUtils.ConvertToInternalUnits(Width, UnitTypeId.Millimeters);
            offsetDirection = (directionCurve.GetEndPoint(0) - projectPoint).Normalize() * beamWidthInternalUnits / 2;

            var beamLocationInDirectionLinePlane = Line.CreateBound(startPoint + offsetDirection, endPoint + offsetDirection);

            return beamLocationInDirectionLinePlane;
        }
    }
}
