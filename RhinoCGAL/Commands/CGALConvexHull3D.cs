using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using CGALDotNet;
using CGALDotNetGeometry;

namespace RhinoCGAL
{
    public class CGALConvexHull3D : Command
    {
        public CGALConvexHull3D()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static CGALConvexHull3D Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "CGALConvexHull3D";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //get 3d points
            //ObjRef[] objRefs=Utilities.CustomGetObject.GetObjectFromSelection("Please select points to create convex hull", ObjectType.Point);

            GetObject getObject = new GetObject();
            getObject.SetCommandPrompt("Please select points to create convex hull");
            getObject.GeometryFilter = ObjectType.Point;
            getObject.GroupSelect = true;
            getObject.SubObjectSelect = true;
            getObject.EnableClearObjectsOnEntry(false);
            //getObject.EnableUnselectObjectsOnExit(false);
            //getObject.DeselectAllBeforePostSelect=false;  
            if (getObject.GetMultiple(1, 0) != GetResult.Object)
            {
                RhinoApp.WriteLine("No point was selected.");
                return getObject.CommandResult();
            }
            ObjRef[] objRefs = getObject.Objects();

            //Create CGALPoints

            var cgalpoint3Ds = new List<CGALDotNetGeometry.Numerics.Point3d>();
            foreach (ObjRef objref in objRefs)
            {
                cgalpoint3Ds.Add(GeometryConversion.RhinoPoint3dToCGALPoint3d(objref.Point().Location));
            }

            //create ConvexHull

            var convexHullOperation= new CGALDotNet.Hulls.ConvexHull3<EIK>();
            //CGALDotNet.Polyhedra.Polyhedron3<EIK> resultPolyhedron = convexHullOperation.CreateHullAsPolyhedron(cgalpoint3Ds.ToArray(), cgalpoint3Ds.Count);
            CGALDotNet.Polyhedra.SurfaceMesh3<EIK> resultSurfaceMesh = convexHullOperation.CreateHullAsSurfaceMesh(cgalpoint3Ds.ToArray(), cgalpoint3Ds.Count);
            //resultSurfaceMesh.GetPoints(new CGALDotNetGeometry.Numerics.Point3d[] aa, resultSurfaceMesh.VertexCount);

            //convert to Rhino mesh

            Mesh resultMesh = GeometryConversion.CGALDotNetToRhinoMesh(resultSurfaceMesh);
            
            //convert to Rhino point3d
            List<Point3d> point3Ds = new List<Point3d>();
            foreach (var pt in resultSurfaceMesh)
            {
                point3Ds.Add(new Point3d(pt.x,pt.y,pt.z));
            }

            //Add points to canvas
            RhinoDoc.ActiveDoc.Objects.AddPoints(point3Ds);
            RhinoDoc.ActiveDoc.Objects.AddMesh(resultMesh);


            return Result.Success;
        }
    }
}
