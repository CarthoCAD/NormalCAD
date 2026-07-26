using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.EditorInput
{
    public struct PromptEntityResult
    {
        public PromptStatus Status { get; }
        public ObjectId ObjectId { get; }
        public Point3d PickedPoint { get; }
        public string StringResult { get; }

        internal PromptEntityResult(PromptStatus status)
        {
            Status = status;
            ObjectId = ObjectId.Null;
            PickedPoint = Point3d.Origin;
            StringResult = string.Empty;
        }

        internal PromptEntityResult(PromptStatus status, ObjectId objectId,
            Point3d pickedPoint, string stringResult)
        {
            Status = status;
            ObjectId = objectId;
            PickedPoint = pickedPoint;
            StringResult = stringResult;
        }

        public static PromptEntityResult Cancelled =>
            new(PromptStatus.Cancel);

        public static PromptEntityResult FromError(string message) =>
            new(PromptStatus.Error, ObjectId.Null, Point3d.Origin, message);
    }
}
