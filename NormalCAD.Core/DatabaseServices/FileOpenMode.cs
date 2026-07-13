namespace NormalCAD.Core.DatabaseServices
{
    public enum FileOpenMode
    {
        OpenForReadAndWrite = 0,
        OpenForReadAndAllShare = 1,
        OpenForReadAndWriteNoShare = 2,
        TryForReadShare = 3
    }
}
