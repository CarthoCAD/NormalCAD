using System;
using System.Collections.Generic;
using System.Linq;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.Spatial
{
    internal struct BBox
    {
        public double MinX, MinY, MaxX, MaxY;

        public double Area => Math.Max(0, (MaxX - MinX) * (MaxY - MinY));

        public double Margin => 2.0 * ((MaxX - MinX) + (MaxY - MinY));

        public double CenterX => (MinX + MaxX) * 0.5;
        public double CenterY => (MinY + MaxY) * 0.5;

        public static BBox FromExtents3d(Extents3d e) => new BBox
        {
            MinX = e.MinPoint.X,
            MinY = e.MinPoint.Y,
            MaxX = e.MaxPoint.X,
            MaxY = e.MaxPoint.Y,
        };

        public readonly bool Intersects(in BBox other) =>
            MinX <= other.MaxX && MaxX >= other.MinX &&
            MinY <= other.MaxY && MaxY >= other.MinY;

        public readonly BBox Extend(in BBox other) => new BBox
        {
            MinX = Math.Min(MinX, other.MinX),
            MinY = Math.Min(MinY, other.MinY),
            MaxX = Math.Max(MaxX, other.MaxX),
            MaxY = Math.Max(MaxY, other.MaxY),
        };

        public readonly double EnlargementArea(in BBox other)
        {
            double minX = Math.Min(MinX, other.MinX);
            double minY = Math.Min(MinY, other.MinY);
            double maxX = Math.Max(MaxX, other.MaxX);
            double maxY = Math.Max(MaxY, other.MaxY);
            double newW = maxX - minX;
            double newH = maxY - minY;
            double newArea = newW > 0 && newH > 0 ? newW * newH : 0;

            double w = MaxX - MinX;
            double h = MaxY - MinY;
            double curArea = w > 0 && h > 0 ? w * h : 0;

            return newArea - curArea;
        }

        public readonly double Overlap(in BBox other)
        {
            double ox = Math.Max(0, Math.Min(MaxX, other.MaxX) - Math.Max(MinX, other.MinX));
            double oy = Math.Max(0, Math.Min(MaxY, other.MaxY) - Math.Max(MinY, other.MinY));
            return ox * oy;
        }
    }

    internal struct Entry
    {
        public BBox MBR;
        public ObjectId Data;
        public Node? Child;

        public Entry(BBox mbr, ObjectId data)
        {
            MBR = mbr;
            Data = data;
            Child = null;
        }

        public Entry(BBox mbr, Node child)
        {
            MBR = mbr;
            Data = ObjectId.Null;
            Child = child;
        }
    }

    internal class Node
    {
        public bool IsLeaf;
        public readonly List<Entry> Entries = new();
        public BBox MBR;

        public int Count => Entries.Count;

        public void RecalculateMBR()
        {
            if (Entries.Count == 0)
                return;
            MBR = Entries[0].MBR;
            for (int i = 1; i < Entries.Count; i++)
                MBR = MBR.Extend(Entries[i].MBR);
        }

        public void AddEntry(Entry entry)
        {
            Entries.Add(entry);
            RecalculateMBR();
        }

        public void RemoveAt(int index)
        {
            Entries.RemoveAt(index);
            if (Entries.Count > 0)
                RecalculateMBR();
        }
    }

    public class RTree
    {
        private const int MaxEntries = 12;
        private const int MinEntries = 4;
        private const int ReinsertCount = 4;

        private Node? _root;
        private int _count;
        private int _height;

        public int Count => _count;

        public void Insert(Extents3d mbr, ObjectId data)
        {
            BBox box = BBox.FromExtents3d(mbr);

            if (_root == null)
            {
                _root = new Node { IsLeaf = true };
                _root.AddEntry(new Entry(box, data));
                _count = 1;
                _height = 1;
                return;
            }

            int treeHeight = _height;
            var reinsertQueue = new List<(BBox Box, ObjectId Data)>();
            var reinsertLevels = new HashSet<int>();

            Node? split = DoInsert(_root, box, data, _height, treeHeight, reinsertLevels, reinsertQueue);
            _count++;

            if (split != null)
            {
                var newRoot = new Node { IsLeaf = false };
                newRoot.AddEntry(new Entry(_root.MBR, _root));
                newRoot.AddEntry(new Entry(split.MBR, split));
                _root = newRoot;
                _height++;
            }

            foreach (var (reBox, reData) in reinsertQueue)
            {
                _count--;
                ProcessReinsert(reBox, reData);
            }
        }

        private void ProcessReinsert(BBox box, ObjectId data)
        {
            if (_root == null)
            {
                _root = new Node { IsLeaf = true };
                _root.AddEntry(new Entry(box, data));
                _count = 1;
                _height = 1;
                return;
            }

            Node? split = DirectInsert(_root, box, data, _height);
            _count++;

            if (split != null)
            {
                var newRoot = new Node { IsLeaf = false };
                newRoot.AddEntry(new Entry(_root.MBR, _root));
                newRoot.AddEntry(new Entry(split.MBR, split));
                _root = newRoot;
                _height++;
            }
        }

        private static Node? DirectInsert(Node node, BBox box, ObjectId data, int level)
        {
            if (level == 1)
            {
                node.AddEntry(new Entry(box, data));
            }
            else
            {
                int childIdx = ChooseSubtree(node, box, level);
                Node child = node.Entries[childIdx].Child!;

                Node? splitChild = DirectInsert(child, box, data, level - 1);

                if (splitChild != null)
                {
                    node.AddEntry(new Entry(splitChild.MBR, splitChild));
                }
                else
                {
                    var entry = node.Entries[childIdx];
                    entry.MBR = child.MBR;
                    node.Entries[childIdx] = entry;
                }

                node.RecalculateMBR();
            }

            if (node.Count > MaxEntries)
                return Split(node);

            return null;
        }

        private static Node? DoInsert(
            Node node, BBox box, ObjectId data, int level, int treeHeight,
            HashSet<int> reinsertLevels, List<(BBox, ObjectId)> reinsertQueue)
        {
            if (level == 1)
            {
                node.AddEntry(new Entry(box, data));
            }
            else
            {
                int childIdx = ChooseSubtree(node, box, level);
                Node child = node.Entries[childIdx].Child!;

                Node? splitChild = DoInsert(child, box, data, level - 1, treeHeight, reinsertLevels, reinsertQueue);

                if (splitChild != null)
                {
                    node.AddEntry(new Entry(splitChild.MBR, splitChild));
                }
                else
                {
                    var entry = node.Entries[childIdx];
                    entry.MBR = child.MBR;
                    node.Entries[childIdx] = entry;
                }

                node.RecalculateMBR();
            }

            if (node.Count > MaxEntries)
            {
                if (level < treeHeight && !reinsertLevels.Contains(level))
                {
                    reinsertLevels.Add(level);
                    Reinsert(node, reinsertQueue);
                    return null;
                }
                else
                {
                    return Split(node);
                }
            }

            return null;
        }

        private static void Reinsert(Node node, List<(BBox, ObjectId)> queue)
        {
            double cx = node.MBR.CenterX;
            double cy = node.MBR.CenterY;

            var sorted = node.Entries
                .Select((e, i) => (Entry: e, Index: i,
                    DistSq: (e.MBR.CenterX - cx) * (e.MBR.CenterX - cx) +
                            (e.MBR.CenterY - cy) * (e.MBR.CenterY - cy)))
                .OrderByDescending(x => x.DistSq)
                .ToList();

            int count = Math.Min(ReinsertCount, sorted.Count);
            var toRemoveIndices = new List<int>();

            for (int i = 0; i < count; i++)
            {
                var entry = sorted[i].Entry;
                toRemoveIndices.Add(sorted[i].Index);

                if (node.IsLeaf)
                {
                    queue.Add((entry.MBR, entry.Data));
                }
                else
                {
                    CollectLeafEntries(entry.Child!, queue);
                }
            }

            toRemoveIndices.Sort((a, b) => b.CompareTo(a));
            foreach (int idx in toRemoveIndices)
                node.RemoveAt(idx);
        }

        private static void CollectLeafEntries(Node node, List<(BBox, ObjectId)> queue)
        {
            if (node.IsLeaf)
            {
                foreach (var e in node.Entries)
                    queue.Add((e.MBR, e.Data));
            }
            else
            {
                foreach (var e in node.Entries)
                    CollectLeafEntries(e.Child!, queue);
            }
        }

        private static int ChooseSubtree(Node node, BBox box, int level)
        {
            int bestIdx = 0;
            double bestScore = double.MaxValue;
            double bestArea = double.MaxValue;

            if (level == 2)
            {
                for (int i = 0; i < node.Count; i++)
                {
                    var entry = node.Entries[i];
                    double overlapEnlargement = ComputeOverlapEnlargement(node, i, box);
                    double areaEnlargement = entry.MBR.EnlargementArea(box);

                    if (overlapEnlargement < bestScore ||
                        (Math.Abs(overlapEnlargement - bestScore) < 1e-12 && areaEnlargement < bestArea))
                    {
                        bestScore = overlapEnlargement;
                        bestArea = areaEnlargement;
                        bestIdx = i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < node.Count; i++)
                {
                    var entry = node.Entries[i];
                    double enlargement = entry.MBR.EnlargementArea(box);
                    double area = entry.MBR.Area;

                    if (enlargement < bestScore ||
                        (Math.Abs(enlargement - bestScore) < 1e-12 && area < bestArea))
                    {
                        bestScore = enlargement;
                        bestArea = area;
                        bestIdx = i;
                    }
                }
            }

            return bestIdx;
        }

        private static double ComputeOverlapEnlargement(Node node, int entryIdx, BBox box)
        {
            var entry = node.Entries[entryIdx];
            BBox enlarged = entry.MBR.Extend(box);

            double overlapBefore = 0;
            double overlapAfter = 0;

            for (int i = 0; i < node.Count; i++)
            {
                if (i == entryIdx) continue;
                var other = node.Entries[i];
                overlapBefore += entry.MBR.Overlap(other.MBR);
                overlapAfter += enlarged.Overlap(other.MBR);
            }

            return overlapAfter - overlapBefore;
        }

        private static Node Split(Node node)
        {
            int axis = ChooseSplitAxis(node);
            int splitIdx = ChooseSplitIndex(node, axis);

            var entries = new List<Entry>(node.Entries);

            if (axis == 0)
                entries.Sort((a, b) => a.MBR.MinX.CompareTo(b.MBR.MinX));
            else
                entries.Sort((a, b) => a.MBR.MinY.CompareTo(b.MBR.MinY));

            var newNode = new Node { IsLeaf = node.IsLeaf };
            for (int i = splitIdx; i < entries.Count; i++)
                newNode.AddEntry(entries[i]);

            node.Entries.Clear();
            for (int i = 0; i < splitIdx; i++)
                node.AddEntry(entries[i]);

            return newNode;
        }

        private static int ChooseSplitAxis(Node node)
        {
            double minSum = double.MaxValue;
            int bestAxis = 0;

            for (int axis = 0; axis < 2; axis++)
            {
                var byLower = new List<Entry>(node.Entries);
                if (axis == 0)
                    byLower.Sort((a, b) => a.MBR.MinX.CompareTo(b.MBR.MinX));
                else
                    byLower.Sort((a, b) => a.MBR.MinY.CompareTo(b.MBR.MinY));

                double sumMargin = ComputeSumMargin(byLower);

                var byUpper = new List<Entry>(node.Entries);
                if (axis == 0)
                    byUpper.Sort((a, b) => a.MBR.MaxX.CompareTo(b.MBR.MaxX));
                else
                    byUpper.Sort((a, b) => a.MBR.MaxY.CompareTo(b.MBR.MaxY));

                sumMargin = Math.Min(sumMargin, ComputeSumMargin(byUpper));

                if (sumMargin < minSum)
                {
                    minSum = sumMargin;
                    bestAxis = axis;
                }
            }

            return bestAxis;
        }

        private static double ComputeSumMargin(List<Entry> sorted)
        {
            int total = sorted.Count;
            double sum = 0;

            for (int k = MinEntries; k <= total - MinEntries; k++)
            {
                BBox mbr1 = sorted[0].MBR;
                for (int i = 1; i < k; i++)
                    mbr1 = mbr1.Extend(sorted[i].MBR);

                BBox mbr2 = sorted[k].MBR;
                for (int i = k + 1; i < total; i++)
                    mbr2 = mbr2.Extend(sorted[i].MBR);

                sum += mbr1.Margin + mbr2.Margin;
            }

            return sum;
        }

        private static int ChooseSplitIndex(Node node, int axis)
        {
            var entries = new List<Entry>(node.Entries);

            if (axis == 0)
                entries.Sort((a, b) => a.MBR.MinX.CompareTo(b.MBR.MinX));
            else
                entries.Sort((a, b) => a.MBR.MinY.CompareTo(b.MBR.MinY));

            int total = entries.Count;
            int bestIdx = MinEntries;
            double minOverlap = double.MaxValue;
            double minArea = double.MaxValue;

            for (int k = MinEntries; k <= total - MinEntries; k++)
            {
                BBox mbr1 = entries[0].MBR;
                for (int i = 1; i < k; i++)
                    mbr1 = mbr1.Extend(entries[i].MBR);

                BBox mbr2 = entries[k].MBR;
                for (int i = k + 1; i < total; i++)
                    mbr2 = mbr2.Extend(entries[i].MBR);

                double overlap = mbr1.Overlap(mbr2);
                double area = mbr1.Area + mbr2.Area;

                if (overlap < minOverlap ||
                    (Math.Abs(overlap - minOverlap) < 1e-12 && area < minArea))
                {
                    minOverlap = overlap;
                    minArea = area;
                    bestIdx = k;
                }
            }

            return bestIdx;
        }

        public bool Remove(Extents3d mbr, ObjectId data)
        {
            if (_root == null)
                return false;

            BBox box = BBox.FromExtents3d(mbr);
            bool found = RemoveEntry(_root, box, data);

            if (!found)
                return false;

            _count--;

            if (_root.Count == 0)
            {
                _root = null;
                _height = 0;
            }
            else if (!_root.IsLeaf && _root.Count == 1)
            {
                _root = _root.Entries[0].Child;
                _height--;
            }

            return true;
        }

        private static bool RemoveEntry(Node node, BBox box, ObjectId data)
        {
            if (node.IsLeaf)
            {
                for (int i = 0; i < node.Count; i++)
                {
                    if (node.Entries[i].Data == data)
                    {
                        node.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }

            for (int i = 0; i < node.Count; i++)
            {
                if (!node.Entries[i].MBR.Intersects(box))
                    continue;

                Node child = node.Entries[i].Child!;

                if (RemoveEntry(child, box, data))
                {
                    if (child.Count == 0)
                    {
                        node.RemoveAt(i);
                    }
                    else
                    {
                        child.RecalculateMBR();
                        var entry = node.Entries[i];
                        entry.MBR = child.MBR;
                        node.Entries[i] = entry;
                        node.RecalculateMBR();
                    }
                    return true;
                }
            }

            return false;
        }

        public List<ObjectId> Search(Extents3d query)
        {
            var results = new List<ObjectId>();
            if (_root != null)
            {
                BBox box = BBox.FromExtents3d(query);
                SearchNode(_root, box, results);
            }
            return results;
        }

        private static void SearchNode(Node node, BBox box, List<ObjectId> results)
        {
            if (node.IsLeaf)
            {
                for (int i = 0; i < node.Count; i++)
                {
                    if (node.Entries[i].MBR.Intersects(box))
                        results.Add(node.Entries[i].Data);
                }
            }
            else
            {
                for (int i = 0; i < node.Count; i++)
                {
                    if (node.Entries[i].MBR.Intersects(box))
                        SearchNode(node.Entries[i].Child!, box, results);
                }
            }
        }

        public void Clear()
        {
            _root = null;
            _count = 0;
            _height = 0;
        }
    }
}
