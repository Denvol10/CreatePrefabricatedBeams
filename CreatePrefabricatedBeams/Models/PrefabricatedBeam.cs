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
        FamilyInstance Instance { get; set; }
        Reference TargetEdge { get; set; }
        double Height { get; set; }
        double Width { get; set; }


    }
}
