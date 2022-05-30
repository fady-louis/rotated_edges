using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace RevitTemplate.Models
{
    public class GetElementBoundingLinesRotion
    {
        public Document _ElementDocuemnt { get; set; }
        public FamilySymbol _Element{ get; set; }

        public List<Curve> Lines { get; set; }
        public List<XYZ> _points { get; set; }
        public GetElementBoundingLinesRotion(Element Element_a)
        {
            List<XYZ> Elementpoints = new List<XYZ>();
            _ElementDocuemnt = Element_a.Document;
            _Element = Element_a as FamilySymbol;
            XYZ _AxisLine;
            double _Angle = GetElementRotaionAngel(Element_a);
            var PTlIST = GetActualBoundingBoxPoints(Element_a, _Angle, out _AxisLine);
            Lines = GetListOfLinesOfTheObject(PTlIST, out Elementpoints, _AxisLine, _Angle);
            _points = Elementpoints;
        }
        public double GetElementRotaionAngel(Element familyInstance)
        {
            try
            {
                if (familyInstance != null)
                {
                    var type = familyInstance.Location;
                    switch (type.GetType().ToString())
                    {
                        case "Autodesk.Revit.DB.LocationPoint":
                            var LocationPt = familyInstance.Location as LocationPoint;
                            var FamilyRotationRadiant = LocationPt.Rotation;
                            FamilyRotation = FamilyRotationRadiant * 180 / Math.PI;
                            return FamilyRotation;
                            break;

                        case "Autodesk.Revit.DB.LocationCurve":
                            var LocationCrv = familyInstance.Location as LocationCurve;
                            var firstPT = LocationCrv.Curve.GetEndPoint(0);
                            var SecondtPT = LocationCrv.Curve.GetEndPoint(1);
                            var projectLocationPoint = familyInstance.Document.ActiveProjectLocation.Location as LocationPoint;
                            var angel = (Math.Atan2(Math.Sqrt(Math.Pow((firstPT.Y - SecondtPT.Y), 2)), Math.Sqrt(Math.Pow((firstPT.X - SecondtPT.X), 2)))) * ((180 / Math.PI));
                            FamilyRotation = angel;
                            break;
                    }
                }
            }
            catch { }
            return FamilyRotation;
        }
        public List<XYZ> GetActualBoundingBoxPoints(Element element, double angel ,out XYZ _AxiesPoint)
        {
            List<XYZ> points = new List<XYZ>();
            XYZ ElementLocationCenter;
            ///Get POint to rotate around
            Line AxisLine;
            #region GetElementAxiesLineToRotatByLocationPoints
            var type = element.Location;
            var Location = element.Location as LocationPoint;
            var LocationPointBoundingBox = element.get_BoundingBox(element.Document.ActiveView);
            var ElementLocationLine = Line.CreateBound(LocationPointBoundingBox.Max, LocationPointBoundingBox.Min).Evaluate(0.5, true);
            ElementLocationCenter = ElementLocationLine;
            var SecondPointToCreateVerticalAxies = new XYZ(ElementLocationLine.X, ElementLocationLine.Y, ElementLocationLine.Z + 100);
            AxisLine = Line.CreateBound(ElementLocationLine, SecondPointToCreateVerticalAxies);
            #endregion
            if (angel != 0)
            {
                bool ElemntRotated = element.Location.Rotate(AxisLine, - ((angel * Math.PI) / 180));
                var Rotaion_002 = GetElementRotaionAngel(element);
            }
            var bBox = element.get_BoundingBox(element.Document.ActiveView);
            points.Add(bBox.Min);
            points.Add(bBox.Max);
            if (angel != 0)
            {
                bool ElemntRotatedt = element.Location.Rotate(AxisLine, ((angel * Math.PI) / 180));
            }
            _AxiesPoint = ElementLocationCenter;
            return points;
        }
        public List<Curve> GetListOfLinesOfTheObject(List<XYZ> BoundingBoxPoints , out List<XYZ> ElementPoints, XYZ _AxisLine, double angel)
        {
            List<XYZ> Points = new List<XYZ>();
            List<XYZ> RotatedPoints = new List<XYZ>();
            List<Curve> RotatedLines = new List<Curve>();
            #region Get the element bounding box Points
            var A_Point = BoundingBoxPoints[0];
            var B_Point = BoundingBoxPoints[1];
            var Point_AY = new XYZ(BoundingBoxPoints[1].X, BoundingBoxPoints[0].Y, BoundingBoxPoints[0].Z);
            var Point_AX = new XYZ(BoundingBoxPoints[0].X, BoundingBoxPoints[1].Y, BoundingBoxPoints[0].Z);
            var Point_AZ = new XYZ(BoundingBoxPoints[0].X, BoundingBoxPoints[0].Y, BoundingBoxPoints[1].Z);

            var Point_BX = new XYZ(BoundingBoxPoints[0].X, BoundingBoxPoints[1].Y, BoundingBoxPoints[1].Z);
            var Point_BY = new XYZ(BoundingBoxPoints[1].X, BoundingBoxPoints[0].Y, BoundingBoxPoints[1].Z);
            var Point_BZ = new XYZ(BoundingBoxPoints[1].X, BoundingBoxPoints[1].Y, BoundingBoxPoints[0].Z); 
            #endregion
            #region Points To List
            ///Add the BoundingBox Points To the List
            Points.Add(A_Point);
            Points.Add(B_Point);
            Points.Add(Point_AX);
            Points.Add(Point_AY);
            Points.Add(Point_AZ);
            Points.Add(Point_BX);
            Points.Add(Point_BY);
            Points.Add(Point_BZ);
            #endregion
            ///create lines from the element Points
            #region Lines
            List<Line> Lines = new List<Line>()
                        {
                            Line.CreateBound(A_Point, Point_AZ),
                            Line.CreateBound(A_Point, Point_AY),
                            Line.CreateBound(A_Point, Point_AX),
                            Line.CreateBound(B_Point, Point_BX),
                            Line.CreateBound(B_Point, Point_BY),
                            Line.CreateBound(B_Point, Point_BZ),

                            Line.CreateBound(Point_AZ, Point_BX),
                            Line.CreateBound(Point_AZ, Point_BY),
                            Line.CreateBound(Point_AY, Point_BY),
                            Line.CreateBound(Point_AY, Point_BZ),
                            Line.CreateBound(Point_AX, Point_BX),
                            Line.CreateBound(Point_AX, Point_BZ)
                    };

            #endregion


            foreach (Line _LINE in Lines)
            {
                GeometryObject obj = _LINE;
               Outline outline = new Outline(A_Point , B_Point);
                
                var ptbefore =  _LINE.GetEndPoint(0);
                ElementTransformUtils.RotateElement(_ElementDocuemnt, new ElementId(_LINE.Id), Line.CreateBound(_AxisLine, new XYZ(_AxisLine.X, _AxisLine.Y, _AxisLine.Z + 10)), ((angel * Math.PI) / 180));
                var ptafter = _LINE.GetEndPoint(0);
/*                Curve Crv =_LINE.CreateTransformed(Transform.CreateRotation(_AxisLine, ((angel * Math.PI) / 180)));*/
                RotatedPoints.Add(_LINE.GetEndPoint(0));
                RotatedPoints.Add(_LINE.GetEndPoint(1));
/*                .NewFamilyInstance(_LINE.GetEndPoint(0), _Element, (Level)(_ElementDocuemnt.GetElement(_ElementDocuemnt.ActiveView.LevelId)), Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
*/                var newelement =new 
                RotatedLines.Add(_LINE);
            }   
            ElementPoints= RotatedPoints;
            return RotatedLines;
        }

        public List<Line> get_edges(Element element)
        {
            Element element1 = element;
            double angel = GetElementRotaionAngel(element1);
            List<XYZ> points = GetActualBoundingBoxPoints(element1, angel, out XYZ _AxiesPoint);

            //get cordinates
            double MinX = points[0].X;
            double MinY = points[0].Y;
            double MinZ = points[0].Z;

            double MaxX = points[1].X;
            double MaxY = points[1].Y;
            double MaxZ = points[1].Z;

            //get points
            XYZ Back_Down_Lift = new XYZ(MinX, MinY, MinZ);
            XYZ Forward_Down_Lift = new XYZ(MinX, MaxY, MinZ);
            XYZ Forward_Down_Right = new XYZ(MaxX, MaxY, MinZ);
            XYZ Back_Down_Right = new XYZ(MaxX, MinY, MinZ);
            XYZ Back_High_Lift = new XYZ(MinX, MinY, MaxZ);
            XYZ Forward_High_Lift = new XYZ(MinX, MaxY, MaxZ);
            XYZ Forward_High_Right = new XYZ(MaxX, MaxY, MaxZ);
            XYZ Back_High_Right = new XYZ(MaxX, MinY, MaxZ);

            //get edges

            //down edges
            Line Back_Down = Line.CreateBound(Back_Down_Lift, Back_Down_Right);
            Line Right_Down = Line.CreateBound(Forward_Down_Right, Back_Down_Right);
            Line Lift_Down = Line.CreateBound(Forward_Down_Lift, Back_Down_Lift);
            Line Forward_Down = Line.CreateBound(Forward_Down_Lift, Forward_Down_Right);

            //Mid edges
            Line Back_Lift_Mid = Line.CreateBound(Back_Down_Lift, Back_High_Lift);
            Line Back_Right_Mid = Line.CreateBound(Back_Down_Right, Back_High_Right);
            Line Forward_Right_Mid = Line.CreateBound(Forward_Down_Right, Forward_High_Right);
            Line Forward_lift_Mid = Line.CreateBound(Forward_Down_Lift, Forward_High_Lift);

            //High edges
            Line Back_High = Line.CreateBound(Back_High_Lift, Back_High_Right);
            Line Right_High = Line.CreateBound(Forward_High_Right, Back_High_Right);
            Line Lift_High = Line.CreateBound(Forward_High_Lift, Back_High_Lift);
            Line Forward_High = Line.CreateBound(Forward_High_Lift, Forward_High_Right);


            //rotate
            bool Back_Down_Rotated = element.Location.Rotate(Back_Down, ((angel * Math.PI) / 180));
            bool Right_Down_Rotated = element.Location.Rotate(Right_Down, ((angel * Math.PI) / 180));
            bool Lift_Down_Rotated = element.Location.Rotate(Lift_Down, ((angel * Math.PI) / 180));
            bool Forward_Down_Rotated = element.Location.Rotate(Forward_Down, ((angel * Math.PI) / 180));

            bool Back_High_Rotated = element.Location.Rotate(Back_High, ((angel * Math.PI) / 180));
            bool Right_High_Rotated = element.Location.Rotate(Right_High, ((angel * Math.PI) / 180));
            bool Lift_High_Rotated = element.Location.Rotate(Lift_High, ((angel * Math.PI) / 180));
            bool Forward_High_Rotated = element.Location.Rotate(Forward_High, ((angel * Math.PI) / 180));

            bool Forward_lift_Mid_Rotated = element.Location.Rotate(Forward_lift_Mid, ((angel * Math.PI) / 180));
            bool Back_Right_Mid_Rotated = element.Location.Rotate(Back_Right_Mid, ((angel * Math.PI) / 180));
            bool Back_Lift_Mid_Rotated = element.Location.Rotate(Back_Lift_Mid, ((angel * Math.PI) / 180));
            bool Forward_Right_Mid_Rotated = element.Location.Rotate(Forward_Right_Mid, ((angel * Math.PI) / 180));
            //Return 
            List<Line> edges = new List<Line>();
            edges.Add(Back_Down);
            edges.Add(Right_Down);
            edges.Add(Lift_Down);
            edges.Add(Forward_Down);

            edges.Add(Back_High);
            edges.Add(Right_High);
            edges.Add(Lift_High);
            edges.Add(Forward_High);

            edges.Add(Back_Lift_Mid);
            edges.Add(Back_Right_Mid);
            edges.Add(Forward_lift_Mid);
            edges.Add(Forward_Right_Mid);

            return edges;

        }

    }
    public enum LocationType
    {
        LocationPoint=1,
        LocationCurve=2
    }
}
