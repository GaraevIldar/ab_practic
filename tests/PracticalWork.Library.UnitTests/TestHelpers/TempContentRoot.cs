namespace PracticalWork.Library.UnitTests.TestHelpers;

internal sealed class TempContentRoot : IDisposable
{
    public string Path { get; }

    public TempContentRoot()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lib-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(System.IO.Path.Combine(Path, "resources"));
    }

    public void WriteResource(string fileName, string content)
    {
        File.WriteAllText(System.IO.Path.Combine(Path, "resources", fileName), content);
    }

    public void Dispose()
    {
        try { Directory.Delete(Path, recursive: true); } catch { }
    }
}
