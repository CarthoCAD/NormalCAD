using System;
using System.Collections.Generic;
using System.Linq;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Spatial;
using Xunit;

namespace NormalCAD.Tests.Core.Spatial
{
    public class RTreeTests
    {
        private static RTree CreateTree() => new RTree();

        private static Extents3d Rect(double minX, double minY, double maxX, double maxY)
        {
            return new Extents3d(new Point3d(minX, minY, 0), new Point3d(maxX, maxY, 0));
        }

        private static ObjectId Id(long value)
        {
            return new ObjectId(value, null!);
        }

        [Fact]
        public void Insert_SingleItem_IncrementsCount()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(1));
            Assert.Equal(1, tree.Count);
        }

        [Fact]
        public void Insert_MultipleItems_CorrectCount()
        {
            var tree = CreateTree();
            for (int i = 0; i < 100; i++)
                tree.Insert(Rect(i, i, i + 10, i + 10), Id(i));
            Assert.Equal(100, tree.Count);
        }

        [Fact]
        public void Search_ExactMatch_ReturnsItem()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(42));
            var results = tree.Search(Rect(0, 0, 10, 10));
            Assert.Single(results);
            Assert.Equal(Id(42), results[0]);
        }

        [Fact]
        public void Search_PartialOverlap_ReturnsMatchingItem()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(1));
            tree.Insert(Rect(100, 100, 110, 110), Id(2));

            var results = tree.Search(Rect(5, 5, 15, 15));
            Assert.Single(results);
            Assert.Equal(Id(1), results[0]);
        }

        [Fact]
        public void Search_NoMatch_ReturnsEmpty()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(1));
            var results = tree.Search(Rect(100, 100, 110, 110));
            Assert.Empty(results);
        }

        [Fact]
        public void Search_LargeQuery_ReturnsAllContained()
        {
            var tree = CreateTree();
            for (int i = 0; i < 50; i++)
                tree.Insert(Rect(i, i, i + 1, i + 1), Id(i));

            var results = tree.Search(Rect(-10, -10, 60, 60));
            Assert.Equal(50, results.Count);
        }

        [Fact]
        public void Remove_ExistingItem_DecrementsCount()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(1));
            tree.Insert(Rect(20, 20, 30, 30), Id(2));
            Assert.Equal(2, tree.Count);

            bool removed = tree.Remove(Rect(0, 0, 10, 10), Id(1));
            Assert.True(removed);
            Assert.Equal(1, tree.Count);
        }

        [Fact]
        public void Remove_NonExistingItem_ReturnsFalse()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(1));
            bool removed = tree.Remove(Rect(0, 0, 10, 10), Id(999));
            Assert.False(removed);
            Assert.Equal(1, tree.Count);
        }

        [Fact]
        public void Remove_ThenSearch_DoesNotFindRemovedItem()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(1));
            tree.Insert(Rect(20, 20, 30, 30), Id(2));

            tree.Remove(Rect(0, 0, 10, 10), Id(1));

            var results = tree.Search(Rect(-5, -5, 15, 15));
            Assert.Empty(results);

            results = tree.Search(Rect(15, 15, 35, 35));
            Assert.Single(results);
            Assert.Equal(Id(2), results[0]);
        }

        [Fact]
        public void Clear_EmptiesTree()
        {
            var tree = CreateTree();
            for (int i = 0; i < 100; i++)
                tree.Insert(Rect(i, i, i + 10, i + 10), Id(i));

            Assert.Equal(100, tree.Count);
            tree.Clear();
            Assert.Equal(0, tree.Count);

            var results = tree.Search(Rect(-1000, -1000, 1000, 1000));
            Assert.Empty(results);
        }

        [Fact]
        public void Insert_AfterClear_WorksCorrectly()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(1));
            tree.Clear();
            tree.Insert(Rect(20, 20, 30, 30), Id(2));

            Assert.Equal(1, tree.Count);
            var results = tree.Search(Rect(0, 0, 40, 40));
            Assert.Single(results);
            Assert.Equal(Id(2), results[0]);
        }

        [Fact]
        public void Search_ManyOverlappingItems_ReturnsAllMatches()
        {
            var tree = CreateTree();
            var testData = new Dictionary<long, Extents3d>();
            var random = new Random(42);

            for (int i = 0; i < 500; i++)
            {
                double x = random.NextDouble() * 1000;
                double y = random.NextDouble() * 1000;
                double w = random.NextDouble() * 50 + 1;
                double h = random.NextDouble() * 50 + 1;
                var r = Rect(x, y, x + w, y + h);
                tree.Insert(r, Id(i));
                testData[i] = r;
            }

            Assert.Equal(500, tree.Count);

            var allResults = tree.Search(Rect(0, 0, 1000, 1000));
            Assert.Equal(500, allResults.Count);

            var smallResults = tree.Search(Rect(400, 400, 600, 600));
            int expected = testData.Count(kvp =>
                kvp.Value.MinPoint.X <= 600 && kvp.Value.MaxPoint.X >= 400 &&
                kvp.Value.MinPoint.Y <= 600 && kvp.Value.MaxPoint.Y >= 400);
            Assert.Equal(expected, smallResults.Count);
        }

        [Fact]
        public void Search_ZeroAreaQuery_Works()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(1));
            tree.Insert(Rect(5, 5, 15, 15), Id(2));
            tree.Insert(Rect(100, 100, 110, 110), Id(3));

            var results = tree.Search(Rect(5, 5, 5, 5));
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void Remove_FromEmptyTree_ReturnsFalse()
        {
            var tree = CreateTree();
            bool removed = tree.Remove(Rect(0, 0, 10, 10), Id(1));
            Assert.False(removed);
        }

        [Fact]
        public void Insert_ZeroAreaExtents_Works()
        {
            var tree = CreateTree();
            tree.Insert(Rect(5, 5, 5, 5), Id(1));
            Assert.Equal(1, tree.Count);

            var results = tree.Search(Rect(0, 0, 10, 10));
            Assert.Single(results);
        }

        [Fact]
        public void Insert_ManyItemsThenRemoveHalf_Works()
        {
            var tree = CreateTree();
            for (int i = 0; i < 100; i++)
                tree.Insert(Rect(i * 10, i * 10, i * 10 + 8, i * 10 + 8), Id(i));

            Assert.Equal(100, tree.Count);

            for (int i = 0; i < 100; i += 2)
                tree.Remove(Rect(i * 10, i * 10, i * 10 + 8, i * 10 + 8), Id(i));

            Assert.Equal(50, tree.Count);

            for (int i = 0; i < 100; i++)
            {
                var results = tree.Search(Rect(i * 10, i * 10, i * 10 + 8, i * 10 + 8));
                if (i % 2 == 0)
                    Assert.Empty(results);
                else
                    Assert.Single(results);
            }
        }

        [Fact]
        public void Insert_NegativeCoordinates_Works()
        {
            var tree = CreateTree();
            tree.Insert(Rect(-100, -100, -50, -50), Id(1));
            tree.Insert(Rect(-50, -50, 0, 0), Id(2));
            tree.Insert(Rect(0, 0, 50, 50), Id(3));

            Assert.Equal(3, tree.Count);

            var results = tree.Search(Rect(-75, -75, -25, -25));
            Assert.Equal(2, results.Count);

            results = tree.Search(Rect(10, 10, 20, 20));
            Assert.Single(results);
        }

        [Fact]
        public void Insert_DuplicateExtents_DifferentIds_AllStored()
        {
            var tree = CreateTree();
            tree.Insert(Rect(0, 0, 10, 10), Id(1));
            tree.Insert(Rect(0, 0, 10, 10), Id(2));
            tree.Insert(Rect(0, 0, 10, 10), Id(3));

            Assert.Equal(3, tree.Count);

            var results = tree.Search(Rect(0, 0, 10, 10));
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public void Search_ReturnsDistinctIds()
        {
            var tree = CreateTree();
            for (int i = 0; i < 200; i++)
                tree.Insert(Rect(i, i, i + 10, i + 10), Id(i));

            var results = tree.Search(Rect(0, 0, 200, 200));
            Assert.Equal(200, results.Count);
            Assert.Equal(200, results.Distinct().Count());
        }
    }
}
