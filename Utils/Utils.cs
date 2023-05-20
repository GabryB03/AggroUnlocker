using System.Diagnostics;
using System.IO;
using System.Threading;

public class Utils
{
    public static void UnlockFile(string path)
    {
        TakeOwnership(path);

        try
        {
            File.SetAttributes(path, FileAttributes.Normal);
        }
        catch
        {

        }

        try
        {
            DeadLock.Classes.ListViewLocker locker = new DeadLock.Classes.ListViewLocker(path, 0);
            locker.Unlock();
        }
        catch
        {

        }

        try
        {
            File.SetAttributes(path, FileAttributes.Normal);
        }
        catch
        {

        }

        while (File.Exists(path))
        {
            Thread.Sleep(1000);

            try
            {
                File.Delete(path);
            }
            catch
            {

            }
        }
    }

    public static void UnlockDirectory(string path)
    {
        TakeOwnership(path);

        try
        {
            DirectoryInfo info = new DirectoryInfo(path);
            info.Attributes = FileAttributes.Directory;
        }
        catch
        {

        }

        try
        {
            DeadLock.Classes.ListViewLocker locker = new DeadLock.Classes.ListViewLocker(path, 0);
            locker.Unlock();
        }
        catch
        {

        }

        try
        {
            DirectoryInfo info = new DirectoryInfo(path);
            info.Attributes = FileAttributes.Directory;
        }
        catch
        {

        }

        while (Directory.Exists(path))
        {
            Thread.Sleep(1000);

            try
            {
                Directory.Delete(path);
            }
            catch
            {

            }
        }
    }

    private static void TakeOwnership(string filePath)
    {
        try
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", $"/c takeown /f \"{filePath}\"");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process processStarted = Process.Start(processInfo);
            processStarted.Start();
            processStarted.WaitForExit();
        }
        catch
        {

        }

        try
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", $"/c icacls \"{filePath}\" /grant \"%username%\":F /c");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process processStarted = Process.Start(processInfo);
            processStarted.Start();
            processStarted.WaitForExit();
        }
        catch
        {

        }
    }
}