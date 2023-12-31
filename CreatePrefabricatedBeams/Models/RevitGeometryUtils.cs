﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatePrefabricatedBeams.Models
{
    public class RevitGeometryUtils
    {
        // Получение балок пролетного строения с помощью пользовательского выбора
        public static List<Element> GetElementsBySelection(UIApplication uiapp, ISelectionFilter filter, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var pickedElems = sel.PickElementsByRectangle(filter, "Select Beams");
            elementIds = ElementIdToString(pickedElems);

            return pickedElems.ToList();
        }

        // Проверка на то существуют ли элементы с данным Id в модели
        public static bool IsElemsExistInModel(Document doc, IEnumerable<int> elems, Type type)
        {
            if (elems is null)
            {
                return false;
            }

            foreach (var elem in elems)
            {
                ElementId id = new ElementId(elem);
                Element curElem = doc.GetElement(id);
                if (curElem is null || !(curElem.GetType() == type))
                {
                    return false;
                }
            }

            return true;
        }

        // Получение id элементов на основе списка в виде строки
        public static List<int> GetIdsByString(string elems)
        {
            if (string.IsNullOrEmpty(elems))
            {
                return null;
            }

            var elemIds = elems.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => int.Parse(s.Remove(0, 2)))
                         .ToList();

            return elemIds;
        }

        // Получение балок по их id
        public static List<Element> GetElementsById(Document doc, IEnumerable<int> ids)
        {
            var elems = new List<Element>();
            foreach (var id in ids)
            {
                ElementId elemId = new ElementId(id);
                Element beam = doc.GetElement(elemId);
                elems.Add(beam);
            }

            return elems;
        }

        // Метод получения списка линий на поверхности дороги
        public static List<Line> GetRoadLines(UIApplication uiapp, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var selectedOnRoadSurface = sel.PickObjects(ObjectType.Element, "Select Road Lines");
            var directShapesRoadSurface = selectedOnRoadSurface.Select(r => uiapp.ActiveUIDocument.Document.GetElement(r))
                                                               .OfType<DirectShape>();
            elementIds = ElementIdToString(directShapesRoadSurface);
            var curvesRoadSurface = GetCurvesByDirectShapes(directShapesRoadSurface);
            var linesRoadSurface = curvesRoadSurface.OfType<Line>().ToList();

            return linesRoadSurface;
        }

        // Получение линий по их id
        public static List<Curve> GetCurvesById(Document doc, IEnumerable<int> ids)
        {
            var directShapeLines = new List<DirectShape>();
            foreach (var id in ids)
            {
                ElementId elemId = new ElementId(id);
                DirectShape line = doc.GetElement(elemId) as DirectShape;
                directShapeLines.Add(line);
            }

            var lines = GetCurvesByDirectShapes(directShapeLines).OfType<Curve>().ToList();

            return lines;
        }

        // Получение линии на стороне смещения
        public static Curve GetDirectionLine(UIApplication uiapp, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var directionCurvePicked = sel.PickObject(ObjectType.Element, "Выберете линию на стороне смещения");
            Options options = new Options();
            Element curveElement = uiapp.ActiveUIDocument.Document.GetElement(directionCurvePicked);
            elementIds = "Id" + curveElement.Id.IntegerValue;
            var directionLine = curveElement.get_Geometry(options).First() as Curve;

            return directionLine;
        }

        // Получение линии по Id
        public static Curve GetDirectionLineById(Document doc, string elemIdInSettings)
        {
            var elemId = GetIdsByString(elemIdInSettings).First();
            ElementId modelLineId = new ElementId(elemId);
            Element modelLine = doc.GetElement(modelLineId);
            Options options = new Options();
            Curve line = modelLine.get_Geometry(options).First() as Curve;

            return line;
        }

        /* Пересечение линии и плоскости
 * (преобразует линию в вектор, поэтому пересекает любую линию не параллельную плоскости)
 */
        public static XYZ LinePlaneIntersection(Line line, Plane plane, out double lineParameter)
        {
            XYZ planePoint = plane.Origin;
            XYZ planeNormal = plane.Normal;
            XYZ linePoint = line.GetEndPoint(0);

            XYZ lineDirection = (line.GetEndPoint(1) - linePoint).Normalize();

            // Проверка на параллельность линии и плоскости
            if ((planeNormal.DotProduct(lineDirection)) == 0)
            {
                lineParameter = double.NaN;
                return null;
            }

            lineParameter = (planeNormal.DotProduct(planePoint)
              - planeNormal.DotProduct(linePoint))
                / planeNormal.DotProduct(lineDirection);

            return linePoint + lineParameter * lineDirection;
        }

        // Получение линии из списка, которая пересекается с плоскостью
        public static Line GetIntersectCurve(IEnumerable<Line> lines, Plane plane)
        {
            var intersectionLines = new List<Line>();

            XYZ originPlane = plane.Origin;
            XYZ directionLine = plane.XVec;

            var lineByPlane = Line.CreateUnbound(originPlane, directionLine);

            foreach (var line in lines)
            {
                XYZ startPoint = line.GetEndPoint(0);
                XYZ finishPoint = line.GetEndPoint(1);

                XYZ startPointOnBase = new XYZ(startPoint.X, startPoint.Y, 0);
                XYZ finishPointOnBase = new XYZ(finishPoint.X, finishPoint.Y, 0);

                var baseLine = Line.CreateBound(startPointOnBase, finishPointOnBase);

                var result = new IntersectionResultArray();
                var compResult = lineByPlane.Intersect(baseLine, out result);
                if (compResult == SetComparisonResult.Overlap)
                {
                    intersectionLines.Add(line);
                }
            }

            if (intersectionLines.Count == 1)
            {
                return intersectionLines.First();
            }
            else if (intersectionLines.Count > 1)
            {
                return intersectionLines.OrderBy(l => l.Evaluate(0.5, true).DistanceTo(plane.Origin)).First();
            }

            return null;
        }

        // Метод получения строки с ElementId
        private static string ElementIdToString(IEnumerable<Element> elements)
        {
            var stringArr = elements.Select(e => "Id" + e.Id.IntegerValue.ToString()).ToArray();
            string resultString = string.Join(", ", stringArr);

            return resultString;
        }

        // Получение линий на основе элементов DirectShape
        private static List<Curve> GetCurvesByDirectShapes(IEnumerable<DirectShape> directShapes)
        {
            var curves = new List<Curve>();

            Options options = new Options();
            var geometries = directShapes.Select(d => d.get_Geometry(options)).SelectMany(g => g);

            foreach (var geom in geometries)
            {
                if (geom is PolyLine polyLine)
                {
                    var polyCurve = GetCurvesByPolyline(polyLine);
                    curves.AddRange(polyCurve);
                }
                else
                {
                    curves.Add(geom as Curve);
                }
            }

            return curves;
        }

        // Метод получения списка линий на основе полилинии
        private static IEnumerable<Curve> GetCurvesByPolyline(PolyLine polyLine)
        {
            var curves = new List<Curve>();

            for (int i = 0; i < polyLine.NumberOfCoordinates - 1; i++)
            {
                var line = Line.CreateBound(polyLine.GetCoordinate(i), polyLine.GetCoordinate(i + 1));
                curves.Add(line);
            }

            return curves;
        }
    }
}
