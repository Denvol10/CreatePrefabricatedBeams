﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using CreatePrefabricatedBeams.Models;
using CreatePrefabricatedBeams.Models.Filters;
using System.Windows.Documents.DocumentStructures;

namespace CreatePrefabricatedBeams
{
    public class RevitModelForfard
    {
        private UIApplication Uiapp { get; set; } = null;
        private Application App { get; set; } = null;
        private UIDocument Uidoc { get; set; } = null;
        private Document Doc { get; set; } = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            Uiapp = uiapp;
            App = uiapp.Application;
            Uidoc = uiapp.ActiveUIDocument;
            Doc = uiapp.ActiveUIDocument.Document;
        }

        #region Балки пролетного строения
        public List<Element> BeamElements { get; set; }

        private string _beamElementIds;
        public string BeamElementIds
        {
            get => _beamElementIds;
            set => _beamElementIds = value;
        }

        public void GetBeamElementsBySelection()
        {
            BeamElements = RevitGeometryUtils.GetElementsBySelection(Uiapp, new StructuralFramingCategoryFilter(), out _beamElementIds);
        }
        #endregion

        #region Линия на поверхности 1
        public List<Line> RoadLines1 { get; set; }

        private string _roadLineElemIds1;
        public string RoadLineElemIds1
        {
            get => _roadLineElemIds1;
            set => _roadLineElemIds1 = value;
        }

        public void GetRoadLine1()
        {
            RoadLines1 = RevitGeometryUtils.GetRoadLines(Uiapp, out _roadLineElemIds1);
        }
        #endregion

        #region Получение линии на поверхности 1 из Settings
        public void GetRoadLines1BySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            RoadLines1 = RevitGeometryUtils.GetCurvesById(Doc, elemIds).OfType<Line>().ToList();
        }
        #endregion

        #region Линия на поверхности 2
        public List<Line> RoadLines2 { get; set; }

        private string _roadLineElemIds2;
        public string RoadLineElemIds2
        {
            get => _roadLineElemIds2;
            set => _roadLineElemIds2 = value;
        }

        public void GetRoadLine2()
        {
            RoadLines2 = RevitGeometryUtils.GetRoadLines(Uiapp, out _roadLineElemIds2);
        }
        #endregion

        #region Получение линии на поверхности 2 из Settings
        public void GetRoadLines2BySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            RoadLines2 = RevitGeometryUtils.GetCurvesById(Doc, elemIds).OfType<Line>().ToList();
        }
        #endregion

        #region Линия на стороне смещения
        public Curve DirectionLine { get; set; }

        private string _directionLineId;
        public string DirectionLineId
        {
            get => _directionLineId;
            set => _directionLineId = value;
        }

        public void GetDirectionLine()
        {
            DirectionLine = RevitGeometryUtils.GetDirectionLine(Uiapp, out _directionLineId);
        }
        #endregion

        #region Получение границы 1 из Settings
        public void GetDirectionLineBySettings(string elemIdInSettings)
        {
            DirectionLine = RevitGeometryUtils.GetDirectionLineById(Doc, elemIdInSettings);
        }
        #endregion

        #region Проверка на то существуют линии оси и линии на поверхности в модели
        public bool IsLinesExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(DirectShape));
        }
        #endregion

        #region Проверка на то существует линия на стороне смещения в модели
        public bool IsDirectionLineExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(ModelLine));
        }
        #endregion

        // Проверка на то существуют ли балки в модели
        public bool IsElementsExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(FamilyInstance));
        }

        // Получение блоков из Settings
        public void GetBeamsBySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            BeamElements = RevitGeometryUtils.GetElementsById(Doc, elemIds);
        }

        // Перемещение балок
        public void MoveBeams(double beamHeight, double beamWidth, double roadSurfaceThikness, double slabThikness)
        {
            var prefabricatedBeams = BeamElements.Select(b => new PrefabricatedBeam(b as FamilyInstance, beamHeight, beamWidth));

            using (Transaction trans = new Transaction(Doc, "Move Beams"))
            {
                trans.Start();
                foreach(var prefabBeam in prefabricatedBeams)
                {
                    LocationCurve oldLocation;
                    var newLocationLine = prefabBeam.GetNewLocation(DirectionLine,
                                                                    RoadLines1,
                                                                    RoadLines2,
                                                                    roadSurfaceThikness,
                                                                    slabThikness,
                                                                    out oldLocation);
                    oldLocation.Curve = newLocationLine;

                    Parameter zAlignment = prefabBeam.BeamInstance.get_Parameter(BuiltInParameter.Z_JUSTIFICATION);
                    if (zAlignment.AsInteger() == 0)
                    {
                        zAlignment.Set(3);
                    }

                    double actualLength = prefabBeam.BeamInstance.get_Parameter(BuiltInParameter.STRUCTURAL_FRAME_CUT_LENGTH).AsDouble();
                    double length = prefabBeam.BeamInstance.get_Parameter(BuiltInParameter.INSTANCE_LENGTH_PARAM).AsDouble();
                    double differenceLength = actualLength - length;
                    Parameter startExtensionParam = prefabBeam.BeamInstance.get_Parameter(BuiltInParameter.START_EXTENSION);
                    startExtensionParam.Set(differenceLength);
                }

                trans.Commit();
            }
        }
    }
}
