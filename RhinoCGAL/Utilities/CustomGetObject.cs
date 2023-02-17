using Rhino;
using Rhino.DocObjects;
using Rhino.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoCGAL.Utilities
{
    class CustomGetObject
    {
        public static ObjRef[] GetObjectFromSelection(string commandPrompt,ObjectType objectType)
        {
            var getObject = new Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt(commandPrompt);
            getObject.GeometryFilter = ObjectType.Point;
            getObject.GroupSelect = true;
            getObject.SubObjectSelect = true;
            getObject.EnableClearObjectsOnEntry(false);
            //getObject.EnableUnselectObjectsOnExit(false);
            //getObject.DeselectAllBeforePostSelect=false;  
            if (getObject.GetMultiple(1, 0) != GetResult.Object)
            {
                RhinoApp.WriteLine("No object was selected.");
            }
            return getObject.Objects();
        }
    }
}
