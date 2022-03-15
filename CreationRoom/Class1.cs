using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationRoom
{
    [Transaction(TransactionMode.Manual)]
    public class CreationRoom : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            CreateRooms(doc);
            return Result.Succeeded;
            
        }
        public void CreateRooms(Document document)
        {
            var levels = new FilteredElementCollector(document)
                 .OfClass(typeof(Level))
                 .ToList(); 
            using (Transaction tr = new Transaction(document))
            {
                tr.Start("Create room");
                foreach (Level level in levels)
                {
                    PlanTopology planTtopology = document.get_PlanTopology(level);
                    PlanCircuitSet circuitSet = planTtopology.Circuits;
                    foreach (PlanCircuit circuit in circuitSet)
                    {
                        if (!circuit.IsRoomLocated)
                        {
                            Room room = document.Create.NewRoom(null, circuit);
                            View view = document.ActiveView;
                             BoundingBoxXYZ bounding = room.get_BoundingBox(null);
                             XYZ center = (bounding.Max + bounding.Min) * 0.5;
                             LocationPoint locPt = (LocationPoint)room.Location;
                             XYZ roomCenter = new XYZ(center.X, center.Y, locPt.Point.Z);
                             UV uV = new UV(roomCenter.X, roomCenter.Y);
                             RoomTag roomTag= document.Create.NewRoomTag(new LinkElementId(room.Id), uV, view.Id);
                        }
                    }
                }
                tr.Commit();
            }
        }
        
    }
}
