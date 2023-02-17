using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using CGAL.Wrapper;
using System.Collections.Generic;
using System.Diagnostics;

namespace RhinoCGAL.Commands
{
    public class CGALStraightSkeleton : Command
    {
        public CGALStraightSkeleton()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static CGALStraightSkeleton Instance { get; private set; }

        public override string EnglishName => "CGALStraightSkeleton";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var getOption = new GetOption();
            getOption.SetCommandPrompt("select option");

            //flag to differentiate options
            int flag = 0;

            while (true)
            {
                getOption.ClearCommandOptions();
                var polygonOption = getOption.AddOptionList("PolygonType", 
                                                               new List<string>() {"with_holes","without_holes"},0);
                
                var res = getOption.Get();
                if (res == GetResult.Option)
                {
                    var option = getOption.Option();
                    if (option.Index == polygonOption)
                        flag = option.CurrentListOptionIndex;
                    break;
                }
                else
                {
                    Rhino.RhinoApp.WriteLine("result was cancel");
                    return Result.Cancel;
                }
            }


            #region get outer polygon

            GetObject getObject = new GetObject();
            getObject.SetCommandPrompt("Please select outside polyline contour");
            getObject.GeometryFilter = ObjectType.Curve;
            getObject.GroupSelect = true;
            getObject.SubObjectSelect = true;
            getObject.EnableClearObjectsOnEntry(true);
            //getObject.EnableUnselectObjectsOnExit(false);
            getObject.DeselectAllBeforePostSelect = true;
            if (getObject.GetMultiple(1, 1) != GetResult.Object)
            {
                RhinoApp.WriteLine("No object is selected.");
                return getObject.CommandResult();
            }
            ObjRef objRef = getObject.Object(0);

            //outer contour data
            Polyline outerPolyline;

            if (!objRef.Curve().IsPolyline())
            {
                RhinoApp.WriteLine("No Polyline was selected.");
                return Result.Failure;
            }
            else
            {

                objRef.Curve().TryGetPolyline(out outerPolyline);

                //复制一份，在command prompt中显示
                Polyline temp = outerPolyline.Duplicate();
                //删掉第一个点，防止首尾重复
                temp.RemoveAt(0);
                //有多少个vertex
                RhinoApp.WriteLine(temp.Count.ToString());
                foreach (var pt in temp.ToArray())
                {
                    RhinoApp.WriteLine(pt.X.ToString() + " " + pt.Y.ToString());
                }

            }
            //check
            if (PolygonCheck(outerPolyline) == false)
            {
                return Result.Failure;
            }
            #endregion


            #region get interior polygons(holes) as List<Polyline>
            List<Polyline> polyLines = new List<Polyline>();
            if (flag == 0)
            {

                GetObject getObject2 = new GetObject();
                getObject2.SetCommandPrompt("Please select inside holes:");
                getObject2.GeometryFilter = ObjectType.Curve;
                getObject2.GroupSelect = true;
                getObject2.SubObjectSelect = true;
                getObject2.EnableClearObjectsOnEntry(true);
                getObject2.DisablePreSelect();
                //getObject.EnableUnselectObjectsOnExit(false);
                getObject2.DeselectAllBeforePostSelect = true;
                if (getObject2.GetMultiple(1, 0) != GetResult.Object)
                {
                    RhinoApp.WriteLine("No object is selected.");
                    return getObject2.CommandResult();
                }
                ObjRef[] objRefs = getObject2.Objects();




                foreach (ObjRef objRef2 in objRefs)
                {
                    if (!objRef2.Curve().IsPolyline())
                    {
                        RhinoApp.WriteLine("No Polyline is selected.");
                        return Result.Failure;
                    }
                    else
                    {
                        objRef2.Curve().TryGetPolyline(out Polyline pLine2);

                        //check and reorder the direction
                        if (PolygonCheck(pLine2) == false)
                        {
                            return Result.Failure;
                        }

                        polyLines.Add(pLine2);
                    }
                }
            }
            else
            {
                polyLines = null;
            }

            #endregion


            //data conversion from rhino to cgal wrapper
            var outer = GeometryConversion.RhinoPolylineToVector2List(outerPolyline);
            var holes = GeometryConversion.RhinoPolylineListToVector2ListList(polyLines);


            //Generate Skeleton
            Stopwatch w = new Stopwatch();
            w.Start();
            var ssk = CGAL.Wrapper.StraightSkeleton.Generate(outer,holes);
            w.Stop();


            //draw the skeleton in Rhino doc
            Stopwatch w2 = new Stopwatch();
            w2.Start();

            foreach (var edge in ssk.Skeleton)
            {
                //create layer
                RhinoDoc.ActiveDoc.Layers.Add("Skeleton",System.Drawing.Color.DarkRed);
                //set layer
                ObjectAttributes attributes = new ObjectAttributes();
                attributes.LayerIndex= doc.Layers.FindName("Skeleton").Index;
                //add geometry to current doc
                RhinoDoc.ActiveDoc.Objects.AddLine(GeometryConversion.Vector2ToRhinoPoint3d(edge.Start.Position), GeometryConversion.Vector2ToRhinoPoint3d(edge.End.Position),attributes);
            }

            //draw the spokes in Rhino doc
            foreach (var edge in ssk.Spokes)
            {
                //create layer
                RhinoDoc.ActiveDoc.Layers.Add("Spokes", System.Drawing.Color.DarkGreen);
                //set layer
                ObjectAttributes attributes = new ObjectAttributes();
                attributes.LayerIndex = doc.Layers.FindName("Spokes").Index;
                //add geometry to current doc
                RhinoDoc.ActiveDoc.Objects.AddLine(GeometryConversion.Vector2ToRhinoPoint3d(edge.Start.Position), GeometryConversion.Vector2ToRhinoPoint3d(edge.End.Position),attributes);
            }
            w2.Stop();

            //test efficiency
            string time = string.Format("Calculation:{0}ms; Draw:{1}ms", w.ElapsedMilliseconds, w2.ElapsedMilliseconds);
            RhinoApp.WriteLine(time);


            return Result.Success;
        }

        /// <summary>
        /// sanity check for the input polygon
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        public static bool PolygonCheck(Polyline pLine)
        {
            //check is closed
            if (!pLine.IsClosed)
            {
                RhinoApp.WriteLine("The selected polyline is not closed.");
                return false;
            }

            //check orientation
            PolylineCurve a = new PolylineCurve(pLine);

            if (a.ClosedCurveOrientation() == CurveOrientation.CounterClockwise)
            {
                pLine.Reverse();
                RhinoApp.WriteLine("Reverse the direction to keep clockwise loop.");
            }
            return true;
        }
    }
}