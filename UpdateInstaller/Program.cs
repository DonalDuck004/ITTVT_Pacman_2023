using System.Diagnostics;
using System.Reflection;

try
{
    Process.GetProcessById(int.Parse(args[0])).WaitForExit();
}
catch (ArgumentException)
{

}
catch (IndexOutOfRangeException)
{

}

string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
if (Path.GetFileName(path) != "OldInstaller")
{
    var files = Directory.GetFiles(path).ToArray();
    var target = Path.Combine(path, "OldInstaller");
    if (!Directory.Exists(target))
        Directory.CreateDirectory(target);

    foreach (var file in files)
        File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);

    Process.Start(new ProcessStartInfo(Directory.GetFiles(target).Where(x => x.EndsWith(".exe")).First()!)
    {
        UseShellExecute = true
    });
}
else
{
    var update_path = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(path)!)!, "Update");

    foreach (var item in Directory.GetFiles(update_path))
    {
        if (!item.EndsWith("exe"))
            continue;

        Process.Start(new ProcessStartInfo(item)
        {
            UseShellExecute = true,
        });
    }
}


