using CGALDotNet;
using CGALDotNet.Polyhedra;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace RhinoCGAL
{
    /// <summary>
    /// this class provides basic geometry type conversion between rhinocommon and other CGAL .Net wrapper library.
    /// </summary>
    class GeometryConversion
    {
        /// <summary>
        /// Convert CGAL SurfaceMesh3<EIK> to Rhino surface mesh, 输入的mesh只有三角面
        /// </summary>
        /// <param name="surfaceMesh"></param>
        /// <returns></returns>
        public static Mesh CGALDotNetToRhinoMesh(SurfaceMesh3<EIK> surfaceMesh)
        {
            var halfedges = new MeshHalfedge3[surfaceMesh.EdgeCount * 2 - surfaceMesh.BorderEdgeCount];
            List<LineCurve> lines = new List<LineCurve>();
            surfaceMesh.GetHalfedges(halfedges, surfaceMesh.EdgeCount * 2 - surfaceMesh.BorderEdgeCount);
            foreach (var halfedge in halfedges)
            {
                lines.Add(new LineCurve(
                    new Point3d(halfedge.SourcePoint(surfaceMesh).x, halfedge.SourcePoint(surfaceMesh).y, halfedge.SourcePoint(surfaceMesh).z),
                    new Point3d(halfedge.TargetPoint(surfaceMesh).x, halfedge.TargetPoint(surfaceMesh).y, halfedge.TargetPoint(surfaceMesh).z)
                    ));
            }
            return Mesh.CreateFromLines(lines.ToArray(), 3, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
        }
        public static List<LineCurve> CGALDotNetToRhinoLineCurve(SurfaceMesh3<EIK> surfaceMesh)
        {
            var halfedges = new MeshHalfedge3[surfaceMesh.EdgeCount * 2 - surfaceMesh.BorderEdgeCount];
            List<LineCurve> lines = new List<LineCurve>();
            surfaceMesh.GetHalfedges(halfedges, surfaceMesh.EdgeCount * 2 - surfaceMesh.BorderEdgeCount);
            foreach (var halfedge in halfedges)
            {
                lines.Add(new LineCurve(
                    new Point3d(halfedge.SourcePoint(surfaceMesh).x, halfedge.SourcePoint(surfaceMesh).y, halfedge.SourcePoint(surfaceMesh).z),
                    new Point3d(halfedge.TargetPoint(surfaceMesh).x, halfedge.TargetPoint(surfaceMesh).y, halfedge.TargetPoint(surfaceMesh).z)
                    ));
            }
            return lines;
        }
        public static CGALDotNetGeometry.Numerics.Point3d RhinoPoint3dToCGALPoint3d(Rhino.Geometry.Point3d point)
        {
            return new CGALDotNetGeometry.Numerics.Point3d(point.X, point.Y, point.Z);
        }
        public static Rhino.Geometry.Point3d CGALPoint3dToRhinoPoint3d(CGALDotNetGeometry.Numerics.Point3d point)
        {
            return new Rhino.Geometry.Point3d(point.x, point.y, point.z);
        }

        

        // for CGAL.StraightSkeleton Wrapper
        /// <summary>
        /// convert Rhino polygon to system vector2 list,
        /// makesure the polyline is closed,
        /// remove the last duplicated point
        /// </summary>
        /// <param name="polyLine"></param>
        /// <returns></returns>
        public static List<Vector2> RhinoPolylineToVector2List(Polyline polyLine)
        {
            var result = new List<Vector2>();

            if (polyLine.IsClosed)
            {
                foreach (Point3d pt in polyLine.ToList())
                {
                    Vector2 vec2 = new Vector2((float)pt.X, (float)pt.Y);
                    result.Add(vec2);
                }
                result.RemoveAt(result.Count - 1);
            }
            return result;
        }
        /// <summary>
        /// convert rhino polylines to holes in CGAL wrapper
        /// </summary>
        /// <param name="polyLines"></param>
        /// <returns></returns>
        public static List<List<Vector2>> RhinoPolylineListToVector2ListList(List<Polyline> polyLines)
        {
            if (polyLines != null)
            {
                var result = new List<List<Vector2>>();
                foreach (var polyLine in polyLines)
                {
                    result.Add(RhinoPolylineToVector2List(polyLine));
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// convert the vector2d point to rhino3d point, ignore the Z axis.
        /// </summary>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Point3d Vector2ToRhinoPoint3d(Vector2 vec2)
        {
            Point3d pt = new Point3d();
            pt.X = vec2.X;
            pt.Y = vec2.Y;
            pt.Z = 0;
            return pt;
        }
    }
}
