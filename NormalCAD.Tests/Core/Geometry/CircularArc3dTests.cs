using System;
using NormalCAD.Core.Geometry;
using Xunit;

namespace NormalCAD.Tests.Core.Geometry
{
    public class CircularArc3dTests
    {
        [Fact]
        public void Length_FullCircle_Returns2PiR()
        {
            var circle = CircularArc3d.FullCircle(new Point3d(0, 0, 0), 5);
            Assert.Equal(10 * Math.PI, circle.Length, 1e-9);
        }

        [Fact]
        public void Length_HalfCircle_ReturnsPiR()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, 0, Math.PI);
            Assert.Equal(5 * Math.PI, arc.Length, 1e-9);
        }

        [Fact]
        public void Length_QuarterCircle_ReturnsHalfPiR()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, 0, Math.PI / 2);
            Assert.Equal(2.5 * Math.PI, arc.Length, 1e-9);
        }

        [Fact]
        public void GetPointAtDist_Start_ReturnsStartPoint()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, 0, Math.PI);
            var pt = arc.GetPointAtDist(0);
            Assert.Equal(5, pt.X, 1e-9);
            Assert.Equal(0, pt.Y, 1e-9);
        }

        [Fact]
        public void GetPointAtDist_End_ReturnsEndPoint()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, 0, Math.PI);
            var pt = arc.GetPointAtDist(5 * Math.PI);
            Assert.Equal(-5, pt.X, 1e-9);
            Assert.Equal(0, pt.Y, 1e-6);
        }

        [Fact]
        public void GetPointAtDist_Middle_Returns90DegPoint()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, 0, Math.PI);
            var pt = arc.GetPointAtDist(2.5 * Math.PI);
            Assert.Equal(0, pt.X, 1e-6);
            Assert.Equal(5, pt.Y, 1e-6);
        }

        [Fact]
        public void GetClosestPointTo_InsideCircleFull_ProjectsToRadius()
        {
            var circle = CircularArc3d.FullCircle(new Point3d(0, 0, 0), 5);
            var pt = new Point3d(10, 0, 0);
            var closest = circle.GetClosestPointTo(pt);
            Assert.Equal(5, closest.X, 1e-9);
            Assert.Equal(0, closest.Y, 1e-9);
        }

        [Fact]
        public void GetClosestPointTo_OutsideArc_ReturnsEndpoint()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, 0, Math.PI / 2);
            var pt = new Point3d(15, 0, 0);
            var closest = arc.GetClosestPointTo(pt);
            Assert.Equal(5, closest.X, 1e-9);
            Assert.Equal(0, closest.Y, 1e-9);
        }

        [Fact]
        public void GetDistanceTo_OnFullCircle_ReturnsDiffFromRadius()
        {
            var circle = CircularArc3d.FullCircle(new Point3d(0, 0, 0), 5);
            var pt = new Point3d(10, 0, 0);
            Assert.Equal(5, circle.GetDistanceTo(pt), 1e-9);
        }

        [Fact]
        public void IsPointOnArc_FullCircle_AnyPointOnRadius_ReturnsTrue()
        {
            var circle = CircularArc3d.FullCircle(new Point3d(0, 0, 0), 5);
            var pt = new Point3d(5, 0, 0);
            Assert.True(circle.IsPointOnArc(pt));
        }

        [Fact]
        public void IsPointOnArc_LimitedArc_InsideRange_ReturnsTrue()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, 0, Math.PI / 2);
            var pt = new Point3d(3, 4, 0);
            Assert.True(arc.IsPointOnArc(pt));
        }

        [Fact]
        public void IsPointOnArc_LimitedArc_OutsideRange_ReturnsFalse()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, 0, Math.PI / 2);
            var pt = new Point3d(-5, 0, 0);
            Assert.False(arc.IsPointOnArc(pt));
        }

        [Fact]
        public void GetAreaContribution_FullCircle_ReturnsPiRSquared()
        {
            var circle = CircularArc3d.FullCircle(new Point3d(0, 0, 0), 5);
            double area = circle.GetAreaContribution();
            Assert.Equal(25 * Math.PI, area, 1e-9);
        }

        [Fact]
        public void GetAreaContribution_HalfCircle_ReturnsHalfCircleArea()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, 0, Math.PI);
            double contrib = arc.GetAreaContribution();
            Assert.Equal(12.5 * Math.PI, contrib, 1e-9);
        }

        [Fact]
        public void GetAreaContribution_NegativeSweep_ReturnsNegativeArea()
        {
            var arc = new CircularArc3d(new Point3d(0, 0, 0), 5, Math.PI, 0);
            double contrib = arc.GetAreaContribution();
            Assert.True(contrib < 0);
        }

        [Fact]
        public void IntersectWith_CrossingLineSegment_ReturnsPoints()
        {
            var circle = CircularArc3d.FullCircle(new Point3d(0, 0, 0), 5);
            var line = new LineSegment3d(new Point3d(0, -10, 0), new Point3d(0, 10, 0));
            var points = new Point3dCollection();
            circle.IntersectWith(line, points);
            Assert.Equal(2, points.Count);
        }

        [Fact]
        public void IntersectWith_TangentLine_ReturnsOnePoint()
        {
            var circle = CircularArc3d.FullCircle(new Point3d(0, 0, 0), 5);
            var line = new LineSegment3d(new Point3d(-10, 5, 0), new Point3d(10, 5, 0));
            var points = new Point3dCollection();
            circle.IntersectWith(line, points);
            Assert.Single(points);
        }
    }
}
