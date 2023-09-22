using System;
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

    }
}
