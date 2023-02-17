using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using CGALDotNet;
using CGALDotNet.Arrangements;
using System.Collections.Generic;
using CGALDotNet.Polygons;

namespace RhinoCGAL.Commands
{
    public class CGALArrangement2D : Command
    {
        public CGALArrangement2D()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static CGALArrangement2D Instance { get; private set; }

        public override string EnglishName => "CGALArrangement2D";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //get lines from selection
            GetObject getObject = new GetObject();
            getObject.SetCommandPrompt("Please select 2d objects to create 2D arrangement");
            getObject.GeometryFilter = ObjectType.AnyObject;
            getObject.GroupSelect = true;
            getObject.SubObjectSelect = true;
            getObject.EnableClearObjectsOnEntry(false);

            if (getObject.GetMultiple(1, 0) != GetResult.Object)
            {
                RhinoApp.WriteLine("No line was selected.");
                return getObject.CommandResult();
            }
            ObjRef[] objRefs = getObject.Objects();


            // create new arrangement
            var arr = new Arrangement2<EEK>();

            //get line objects as curve
            foreach (ObjRef objRef in objRefs)
            {
                //rhino point3d to cgal point2d
                Point3d A = objRef.Curve().PointAtStart;
                Point3d B = objRef.Curve().PointAtEnd;
                CGALDotNetGeometry.Numerics.Point2d cgalptA = new CGALDotNetGeometry.Numerics.Point2d(A.X, A.Y);
                CGALDotNetGeometry.Numerics.Point2d cgalptB = new CGALDotNetGeometry.Numerics.Point2d(B.X, B.Y);
                //create segment
                arr.InsertSegment(cgalptA, cgalptB, false);
            }
            //get polygon object as rhino polyline
            //foreach (ObjRef objRef in objRefs)
            //{
            //    var vertices1 = objRef.Brep().Vertices;
            //    foreach (var vertex in vertices1)
            //    { 

            //    }

            //}
            //var box = PolygonFactory<EEK>.CreateBox(-5, 5);
            //arr.InsertPolygon(box, true);

            //retrieve vertices
            var vertices = new ArrVertex2[arr.VertexCount];
            arr.GetVertices(vertices, vertices.Length);
            foreach (var vertex in vertices)
            {
                Point3d pt= new Point3d(vertex.Point.x, vertex.Point.y, 0);
                //RhinoDoc.ActiveDoc.Objects.AddPoint(pt);
            }
            
            //retrieve faces
            var faces = new ArrFace2[arr.FaceCount];
            arr.GetFaces(faces, faces.Length);
            foreach (var face in faces)
            {
                List<Point3d> facePoints = new List<Point3d>();
                foreach (var vertex in face.EnumerateVertices(arr))
                {
                    facePoints.Add(new Point3d(vertex.Point.x, vertex.Point.y, 0));
                }
                facePoints.Add(facePoints[0]);//make closed polyline
                var pL=new Polyline(facePoints);
                RhinoDoc.ActiveDoc.Objects.AddPolyline(pL);
            }
            //Brep.CreateEdgeSurface(pL);
            return Result.Success;
        }
    }
}