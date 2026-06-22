using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public abstract class Entity : DBObject
    {
        [Category("General")]
        [DisplayName("Layer")]
        public string Layer { get; set; } = "0";

        
        public ObjectId LayerId { get; set; }

        [Category("General")]
        [DisplayName("Color")]
        [ReadOnly(true)]
        public EntityColor Color { get; set; } = EntityColor.ByLayer;

        [Category("General")]
        [DisplayName("Linetype")]
        public string Linetype { get; set; } = "ByLayer";

        
        public ObjectId LinetypeId { get; set; }

        [Category("General")]
        [DisplayName("Lineweight")]
        public LineWeight LineWeight { get; set; } = LineWeight.ByLayer;

        [Category("General")]
        [DisplayName("Linetype Scale")]
        public double LinetypeScale { get; set; } = 1.0;

        [Category("General")]
        [DisplayName("Transparency")]
        public Transparency Transparency { get; set; } = Transparency.ByLayer;

        [Category("General")]
        [DisplayName("Visible")]
        public bool Visible { get; set; } = true;

        
        public ObjectId BlockId { get; set; }

        
        public Extents3d Bounds { get; protected set; }

        
        public bool IsHighlighted { get; private set; }

        public abstract Entity Clone();

        protected void CopyEntityPropertiesTo(Entity target)
        {
            target.Layer = this.Layer;
            target.LayerId = this.LayerId;
            target.Color = this.Color;
            target.Linetype = this.Linetype;
            target.LinetypeId = this.LinetypeId;
            target.LineWeight = this.LineWeight;
            target.LinetypeScale = this.LinetypeScale;
            target.Transparency = this.Transparency;
            target.Visible = this.Visible;
            target.BlockId = this.BlockId;
            target.IsErased = this.IsErased;
        }

        
        public abstract Extents3d GeometricExtents { get; }

        public Entity GetTransformedCopy(Matrix3d transform)
        {
            var copy = Clone();
            copy.TransformBy(transform);
            return copy;
        }

        
        public virtual bool IsContentSnappable => false;

        
        public virtual bool IsSnappable => true;

        public virtual IEnumerable<(Point3d Point, SnapType Type)> GetOsnapPoints()
        {
            yield break;
        }

        public virtual IEnumerable<Point3d> GetGripPoints()
        {
            yield break;
        }

        public virtual void MoveGripPointsAt(Point3dCollection grips, Vector3d offset)
        {
        }

        public virtual IEnumerable<Point3d> GetStretchPoints()
        {
            yield break;
        }

        public virtual void MoveStretchPointsAt(Point3dCollection stretches, Vector3d offset)
        {
        }

        
        public abstract void TransformBy(Matrix3d transform);

        
        public virtual Matrix3d BlockTransform => Matrix3d.Identity;

        public virtual double GetDistanceTo(Point3d point)
        {
            var curve = GetGeometricCurve();
            return curve?.GetDistanceTo(point) ?? double.MaxValue;
        }

        
        public virtual Curve3d? GetGeometricCurve() => null;

        public virtual void IntersectWith(Entity entity, Intersect intersectType, Point3dCollection points)
        {
            var myCurve = GetGeometricCurve();
            var otherCurve = entity.GetGeometricCurve();
            if (myCurve != null && otherCurve != null)
                myCurve.IntersectWith(otherCurve, points);
        }

        public virtual void Erase()
        {
            IsErased = true;
        }

        public virtual void Highlight()
        {
            IsHighlighted = true;
        }

        public virtual void Unhighlight()
        {
            IsHighlighted = false;
        }

        public virtual void Draw()
        {
        }

        public virtual void List()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"                  {GetType().Name}");
            sb.AppendLine($"Layer: {Layer}");
            sb.AppendLine($"Color: {Color}");
            sb.AppendLine($"Linetype: {Linetype}");
            sb.AppendLine($"LineWeight: {LineWeight}");
            sb.AppendLine($"LinetypeScale: {LinetypeScale}");
            sb.AppendLine($"Visible: {Visible}");
            System.Diagnostics.Debug.WriteLine(sb.ToString());
        }

        public virtual void SetDatabaseDefaults(Database database)
        {
        }
    }
}
