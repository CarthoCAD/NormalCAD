using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using Xunit;

namespace NormalCAD.Tests.Core.DatabaseServices
{
    public class PolylineTests
    {
        private static Polyline MakePolyline(bool closed, params Point2d[] vertices)
        {
            var poly = new Polyline(vertices.Length) { Closed = closed };
            for (int i = 0; i < vertices.Length; i++)
                poly.AddVertexAt(i, vertices[i], 0.0, 0.0, 0.0);
            return poly;
        }

        [Fact]
        public void Area_ClosedSquare10x10_Returns100()
        {
            var poly = MakePolyline(true,
                new Point2d(0, 0),
                new Point2d(10, 0),
                new Point2d(10, 10),
                new Point2d(0, 10));

            Assert.Equal(100, poly.Area, 1e-9);
        }

        [Fact]
        public void Area_ClosedRightTriangle_Returns50()
        {
            var poly = MakePolyline(true,
                new Point2d(0, 0),
                new Point2d(10, 0),
                new Point2d(0, 10));

            Assert.Equal(50, poly.Area, 1e-9);
        }

        [Fact]
        public void Area_OpenPolyline_ComputesWithImplicitClosure()
        {
            var poly = MakePolyline(false,
                new Point2d(0, 0),
                new Point2d(10, 0),
                new Point2d(10, 10));

            Assert.Equal(50, poly.Area, 1e-9);
        }

        [Fact]
        public void Area_ClosedCounterClockwise_ReturnsPositive100()
        {
            var poly = MakePolyline(true,
                new Point2d(0, 0),
                new Point2d(0, 10),
                new Point2d(10, 10),
                new Point2d(10, 0));

            Assert.Equal(100, poly.Area, 1e-9);
        }

        [Fact]
        public void Area_SingleSegment_ReturnsZero()
        {
            var poly = MakePolyline(false,
                new Point2d(0, 0),
                new Point2d(10, 10));

            Assert.Equal(0, poly.Area, 1e-9);
        }

        [Fact]
        public void Area_5x5Square_Returns25()
        {
            var poly = MakePolyline(true,
                new Point2d(5, 5),
                new Point2d(10, 5),
                new Point2d(10, 10),
                new Point2d(5, 10));

            Assert.Equal(25, poly.Area, 1e-9);
        }

        [Fact]
        public void Area_LShapedPolygon_ReturnsCorrect()
        {
            var poly = MakePolyline(true,
                new Point2d(0, 0),
                new Point2d(10, 0),
                new Point2d(10, 5),
                new Point2d(5, 5),
                new Point2d(5, 10),
                new Point2d(0, 10));

            Assert.Equal(75, poly.Area, 1e-9);
        }

        [Fact]
        public void Area_NegativeOrientedSquare_ReturnsPositive100()
        {
            var poly = MakePolyline(true,
                new Point2d(0, 10),
                new Point2d(10, 10),
                new Point2d(10, 0),
                new Point2d(0, 0));

            Assert.Equal(100, poly.Area, 1e-9);
        }

        [Fact]
        public void GetDistanceTo_PointOnSegment_ReturnsZero()
        {
            var poly = MakePolyline(false,
                new Point2d(0, 0),
                new Point2d(10, 0),
                new Point2d(10, 10));

            double dist = poly.GetDistanceTo(new Point3d(5, 0, 0));
            Assert.Equal(0, dist, 1e-9);
        }

        [Fact]
        public void GetDistanceTo_PointAbove_ReturnsVerticalDistance()
        {
            var poly = MakePolyline(false,
                new Point2d(0, 0),
                new Point2d(10, 0));

            double dist = poly.GetDistanceTo(new Point3d(5, 3, 0));
            Assert.Equal(3, dist, 1e-9);
        }

        [Fact]
        public void IntersectWith_CrossingLine_ReturnsIntersection()
        {
            var poly = MakePolyline(false,
                new Point2d(0, 0),
                new Point2d(10, 10),
                new Point2d(10, 0));

            var line = new Line(new Point3d(0, 5, 0), new Point3d(15, 5, 0));
            var points = new Point3dCollection();
            poly.IntersectWith(line, Intersect.OnBothOperands, points);
            Assert.NotEmpty(points);
        }
    }
}
