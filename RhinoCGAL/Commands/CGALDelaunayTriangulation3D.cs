using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;
using CGALDotNet;
using CGALDotNetGeometry;
using CGALDotNet.Triangulations;

namespace RhinoCGAL.Commands
{
    public class CGALDelaunayTriangulation3D : Command
    {
        public CGALDelaunayTriangulation3D()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static CGALDelaunayTriangulation3D Instance { get; private set; }

        public override string EnglishName => "CGALDelaunayTriangulation3D";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //get points from selection
            GetObject getObject = new GetObject();
            getObject.SetCommandPrompt("Please select points to create convex hull");
            getObject.GeometryFilter = ObjectType.Point;
            getObject.GroupSelect = true;
            getObject.SubObjectSelect = true;
            getObject.EnableClearObjectsOnEntry(false);

            if (getObject.GetMultiple(1, 0) != GetResult.Object)
            {
                RhinoApp.WriteLine("No point was selected.");
                return getObject.CommandResult();
            }
            ObjRef[] objRefs = getObject.Objects();

            //Create CGALPoints from rhino point3d
            var cgalpoint3Ds = new List<CGALDotNetGeometry.Numerics.Point3d>();
            foreach (ObjRef objref in objRefs)
            {
                cgalpoint3Ds.Add(GeometryConversion.RhinoPoint3dToCGALPoint3d(objref.Point().Location));
            }

            //Create the triangulation
            var tri = new DelaunayTriangulation3<EIK>(cgalpoint3Ds.ToArray());
            //get triangles
            var triangles = new CGALDotNetGeometry.Shapes.Triangle3d[tri.TriangleCount];
            tri.GetTriangles(triangles,tri.TriangleCount);

            //convert triangle to Rhino planar surface
            foreach (CGALDotNetGeometry.Shapes.Triangle3d triangle in triangles)
            {
                Rhino.Geometry.Point3d pointA = GeometryConversion.CGALPoint3dToRhinoPoint3d(triangle.A);
                Rhino.Geometry.Point3d pointB = GeometryConversion.CGALPoint3dToRhinoPoint3d(triangle.B);
                Rhino.Geometry.Point3d pointC = GeometryConversion.CGALPoint3dToRhinoPoint3d(triangle.C);
                var sur= Rhino.Geometry.NurbsSurface.CreateFromCorners(pointA, pointB, pointC);
                RhinoDoc.ActiveDoc.Objects.AddSurface(sur);
            }
            //get segments
            var segments = new CGALDotNetGeometry.Shapes.Segment3d[tri.FiniteEdgeCount];
            tri.GetSegments(segments, tri.FiniteEdgeCount);
            //construct line
            var lines = new List<Rhino.Geometry.Line>();
            foreach (var segment in segments)
            {
                Rhino.Geometry.Point3d pointA = GeometryConversion.CGALPoint3dToRhinoPoint3d(segment.A);
                Rhino.Geometry.Point3d pointB = GeometryConversion.CGALPoint3dToRhinoPoint3d(segment.B);
                lines.Add(new Rhino.Geometry.Line(pointA, pointB));

                // draw lines
                RhinoDoc.ActiveDoc.Objects.AddLine(pointA, pointB);
            }

            return Result.Success;
        }
    }
}