using System;
using NormalCAD.Core.Geometry;
using Xunit;

namespace NormalCAD.Tests.Core.Geometry
{
    public class CompositeCurve3dTests
    {
        private static CompositeCurve3d Square10()
        {
            return new CompositeCurve3d(
                new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0)),
                new LineSegment3d(new Point3d(10, 0, 0), new Point3d(10, 10, 0)),
                new LineSegment3d(new Point3d(10, 10, 0), new Point3d(0, 10, 0)),
                new LineSegment3d(new Point3d(0, 10, 0), new Point3d(0, 0, 0))
            );
        }

        [Fact]
        public void Length_Square10_Returns40()
        {
            var comp = Square10();
            Assert.Equal(40, comp.Length, 1e-9);
        }

        [Fact]
        public void GetPointAtDist_Square10_QuarterWay_ReturnsCorner()
        {
            var comp = Square10();
            var pt = comp.GetPointAtDist(10);
            Assert.Equal(10, pt.X, 1e-9);
            Assert.Equal(0, pt.Y, 1e-9);
        }

        [Fact]
        public void GetPointAtDist_Square10_HalfWay_ReturnsFarCorner()
        {
            var comp = Square10();
            var pt = comp.GetPointAtDist(20);
            Assert.Equal(10, pt.X, 1e-9);
            Assert.Equal(10, pt.Y, 1e-9);
        }

        [Fact]
        public void GetClosestPointTo_Square10_Outside_ReturnsNearestEdgePoint()
        {
            var comp = Square10();
            var pt = new Point3d(5, -5, 0);
            var closest = comp.GetClosestPointTo(pt);
            Assert.Equal(5, closest.X, 1e-9);
            Assert.Equal(0, closest.Y, 1e-9);
        }

        [Fact]
        public void GetDistanceTo_Square10_PointOnEdge_ReturnsZero()
        {
            var comp = Square10();
            var pt = new Point3d(5, 0, 0);
            Assert.Equal(0, comp.GetDistanceTo(pt), 1e-9);
        }

        [Fact]
        public void GetDistanceTo_Square10_PointOutside_ReturnsCorrectDistance()
        {
            var comp = Square10();
            var pt = new Point3d(5, -3, 0);
            Assert.Equal(3, comp.GetDistanceTo(pt), 1e-9);
        }

        [Fact]
        public void ComputeEnclosedArea_Square10_Returns100()
        {
            var comp = Square10();
            Assert.Equal(100, comp.ComputeEnclosedArea(), 1e-9);
        }

        [Fact]
        public void ComputeEnclosedArea_RightTriangle_ReturnsCorrect()
        {
            var comp = new CompositeCurve3d(
                new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0)),
                new LineSegment3d(new Point3d(10, 0, 0), new Point3d(0, 10, 0)),
                new LineSegment3d(new Point3d(0, 10, 0), new Point3d(0, 0, 0))
            );
            Assert.Equal(50, comp.ComputeEnclosedArea(), 1e-9);
        }

        [Fact]
        public void ComputeEnclosedArea_Counterclockwise_ReturnsPositiveArea()
        {
            var comp = new CompositeCurve3d(
                new LineSegment3d(new Point3d(0, 0, 0), new Point3d(0, 10, 0)),
                new LineSegment3d(new Point3d(0, 10, 0), new Point3d(10, 10, 0)),
                new LineSegment3d(new Point3d(10, 10, 0), new Point3d(10, 0, 0)),
                new LineSegment3d(new Point3d(10, 0, 0), new Point3d(0, 0, 0))
            );
            Assert.Equal(100, comp.ComputeEnclosedArea(), 1e-9);
        }

        [Fact]
        public void IntersectWith_LineAcrossSquare_ReturnsTwoPoints()
        {
            var comp = Square10();
            var line = new LineSegment3d(new Point3d(-5, 5, 0), new Point3d(15, 5, 0));
            var points = new Point3dCollection();
            comp.IntersectWith(line, points);
            Assert.Equal(2, points.Count);
        }

        [Fact]
        public void IntersectWith_LineOutsideSquare_ReturnsNoPoints()
        {
            var comp = Square10();
            var line = new LineSegment3d(new Point3d(-5, -5, 0), new Point3d(15, -5, 0));
            var points = new Point3dCollection();
            comp.IntersectWith(line, points);
            Assert.Empty(points);
        }
    }
}
