using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using ACadSharp;
using ACadSharp.Tables;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class VPortConverter : IConverter
    {
        public bool CanConvertToAcad => true;
        public bool CanConvertToNormal => true;

        public ACadSharp.Tables.VPort ConvertToAcad(ViewportTableRecord source)
        {
            var vp = new ACadSharp.Tables.VPort(source.Name);
            ApplyToAcad(source, vp);
            return vp;
        }

        public void ApplyToAcad(ViewportTableRecord source, ACadSharp.Tables.VPort target)
        {
            target.BottomLeft = new XY(-1, -1);
            target.TopRight = new XY(1, 1);
            target.Center = new XY(source.Center.X, source.Center.Y);
            target.ViewHeight = source.ViewHeight;
            target.Direction = new XYZ(source.Direction.X, source.Direction.Y, source.Direction.Z);
            target.Target = new XYZ(source.Target.X, source.Target.Y, source.Target.Z);
            target.AspectRatio = 1.0;
            target.Origin = XYZ.Zero;
            target.XAxis = XYZ.AxisX;
            target.YAxis = XYZ.AxisY;
            target.RenderMode = RenderMode.Optimized2D;
            target.CircleZoomPercent = 1000;
            target.ShowGrid = false;
            target.SnapOn = false;
            target.GridSpacing = new XY(source.GridSpacingX, source.GridSpacingY);
            target.SnapSpacing = new XY(source.SnapSpacingX, source.SnapSpacingY);
        }

        public ViewportTableRecord ConvertToNormal(ACadSharp.Tables.VPort source)
        {
            return new ViewportTableRecord(
                source.Name,
                new Point3d(source.Center.X, source.Center.Y, 0),
                source.ViewHeight
            )
            {
                Direction = new Vector3d(source.Direction.X, source.Direction.Y, source.Direction.Z),
                Target = new Point3d(source.Target.X, source.Target.Y, source.Target.Z),
                GridSpacingX = source.GridSpacing.X,
                GridSpacingY = source.GridSpacing.Y,
                SnapSpacingX = source.SnapSpacing.X,
                SnapSpacingY = source.SnapSpacing.Y
            };
        }
    }
}
