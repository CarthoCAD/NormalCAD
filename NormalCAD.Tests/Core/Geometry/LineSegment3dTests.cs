using System;
using NormalCAD.Core.Geometry;
using Xunit;

namespace NormalCAD.Tests.Core.Geometry
{
    public class LineSegment3dTests
    {
        [Fact]
        public void Length_HorizontalSegment_ReturnsDeltaX()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            Assert.Equal(10, seg.Length, 1e-9);
        }

        [Fact]
        public void Length_DiagonalSegment_ReturnsCorrect()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(3, 4, 0));
            Assert.Equal(5, seg.Length, 1e-9);
        }

        [Fact]
        public void GetPointAtDist_Start_ReturnsP0()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = seg.GetPointAtDist(0);
            Assert.Equal(0, pt.X, 1e-9);
            Assert.Equal(0, pt.Y, 1e-9);
        }

        [Fact]
        public void GetPointAtDist_End_ReturnsP1()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = seg.GetPointAtDist(10);
            Assert.Equal(10, pt.X, 1e-9);
            Assert.Equal(0, pt.Y, 1e-9);
        }

        [Fact]
        public void GetPointAtDist_Middle_ReturnsMidpoint()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = seg.GetPointAtDist(5);
            Assert.Equal(5, pt.X, 1e-9);
            Assert.Equal(0, pt.Y, 1e-9);
        }

        [Fact]
        public void GetPointAtDist_BeyondLength_ClampsToEnd()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = seg.GetPointAtDist(20);
            Assert.Equal(10, pt.X, 1e-9);
        }

        [Fact]
        public void GetPointAtDist_Negative_ClampsToStart()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = seg.GetPointAtDist(-5);
            Assert.Equal(0, pt.X, 1e-9);
        }

        [Fact]
        public void GetClosestPointTo_OnSegment_ReturnsSamePoint()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = new Point3d(5, 0, 0);
            var closest = seg.GetClosestPointTo(pt);
            Assert.Equal(5, closest.X, 1e-9);
            Assert.Equal(0, closest.Y, 1e-9);
        }

        [Fact]
        public void GetClosestPointTo_AboveSegment_ProjectsVertically()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = new Point3d(5, 10, 0);
            var closest = seg.GetClosestPointTo(pt);
            Assert.Equal(5, closest.X, 1e-9);
            Assert.Equal(0, closest.Y, 1e-9);
        }

        [Fact]
        public void GetClosestPointTo_BeyondEnd_ReturnsEnd()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = new Point3d(15, 5, 0);
            var closest = seg.GetClosestPointTo(pt);
            Assert.Equal(10, closest.X, 1e-9);
        }

        [Fact]
        public void GetDistanceTo_OnSegment_ReturnsZero()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = new Point3d(5, 0, 0);
            Assert.Equal(0, seg.GetDistanceTo(pt), 1e-9);
        }

        [Fact]
        public void GetDistanceTo_AboveSegment_ReturnsVerticalDistance()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var pt = new Point3d(5, 3, 0);
            Assert.Equal(3, seg.GetDistanceTo(pt), 1e-9);
        }

        [Fact]
        public void GetDistAtPoint_Middle_ReturnsHalfLength()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var dist = seg.GetDistAtPoint(new Point3d(5, 0, 0));
            Assert.Equal(5, dist, 1e-9);
        }

        [Fact]
        public void IntersectWith_ParallelLines_ReturnsNoPoints()
        {
            var seg1 = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            var seg2 = new LineSegment3d(new Point3d(0, 5, 0), new Point3d(10, 5, 0));
            var points = new Point3dCollection();
            seg1.IntersectWith(seg2, points);
            Assert.Empty(points);
        }

        [Fact]
        public void IntersectWith_CrossingLines_ReturnsIntersection()
        {
            var seg1 = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 10, 0));
            var seg2 = new LineSegment3d(new Point3d(10, 0, 0), new Point3d(0, 10, 0));
            var points = new Point3dCollection();
            seg1.IntersectWith(seg2, points);
            Assert.Single(points);
            Assert.Equal(5, points[0].X, 1e-9);
            Assert.Equal(5, points[0].Y, 1e-9);
        }

        [Fact]
        public void IntersectWith_NonCrossingLines_ReturnsNoPoints()
        {
            var seg1 = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(5, 5, 0));
            var seg2 = new LineSegment3d(new Point3d(10, 10, 0), new Point3d(15, 15, 0));
            var points = new Point3dCollection();
            seg1.IntersectWith(seg2, points);
            Assert.Empty(points);
        }

        [Fact]
        public void GetAreaContribution_ReturnsShoelaceTerm()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            double contrib = seg.GetAreaContribution();
            Assert.Equal(0, contrib, 1e-9);
        }

        [Fact]
        public void GetAreaContribution_SquareEdge_Correct()
        {
            var seg = new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0));
            double contrib = seg.GetAreaContribution();
            Assert.Equal(0.5 * (0 * 0 - 10 * 0), contrib, 1e-9);
        }
    }
}
