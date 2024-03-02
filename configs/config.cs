using System.IO;

namespace Launcher;

public class Config
{
    public void CheckConfig()
    {
        if (!Directory.Exists("C:\\Launcher"))
        {
            Directory.CreateDirectory("C:\\Launcher");
            if (!Directory.Exists("C:\\Launcher\\Arizona")) Directory.CreateDirectory("C:\\Launcher\\Arizona");
        }
    }
}