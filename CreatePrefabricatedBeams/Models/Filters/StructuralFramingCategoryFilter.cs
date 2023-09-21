using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatePrefabricatedBeams.Models.Filters
{
    public class StructuralFramingCategoryFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            int elemCategoryId = elem.Category.Id.IntegerValue;
            int structuralFramingCategoryId = (int)BuiltInCategory.OST_StructuralFraming;

            if (elemCategoryId == structuralFramingCategoryId)
            {
                return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
