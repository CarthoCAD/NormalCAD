using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class ViewportTableRecord : SymbolTableRecord
    {
        public Point3d Center { get; set; } = Point3d.Origin;
        public double ViewHeight { get; set; } = 100.0;
        public Vector3d Direction { get; set; } = new Vector3d(0, 0, 1);
        public Point3d Target { get; set; } = Point3d.Origin;

        public double GridSpacingX { get; set; } = 10.0;
        public double GridSpacingY { get; set; } = 10.0;
        public double SnapSpacingX { get; set; } = 0.5;
        public double SnapSpacingY { get; set; } = 0.5;

        public ViewportTableRecord()
        {
        }

        public ViewportTableRecord(string name, Point3d center, double viewHeight)
        {
            Name = name;
            Center = center;
            ViewHeight = viewHeight;
        }
    }
}
