using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ToppleBitModding
{
    public class Loader
    {
        static string path = "./logs.txt";
        static bool isPatched = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (isPatched)
            {
                return;
            }
            if (File.Exists(path))
            {
                File.Copy(path, path + ".old", true);
                File.Delete(path);
            }

            string modsPath = Path.Combine(
                Path.GetDirectoryName(path),
                "Mods"
            );

            LoadMods(modsPath);

            PatchEngine.PatchAll();
            Loader.Log("[ToppleBitModding] Loaded successfully!");
            isPatched = true;
        }

        public static bool Log(string message)
        {
            try
            {
                string content = File.Exists(path) ? File.ReadAllText(path) : "";
                File.WriteAllText(path, content + message + "\n");
                return true;
            } catch  {
                return false;
            }
        }

        public static void LoadMods(string modsPath)
        {
            if (!Directory.Exists(modsPath))
            {
                Directory.CreateDirectory(modsPath);
                Loader.Log("[ToppleBitModding] Mods folder created");
                return;
            }

            foreach (var dll in Directory.GetFiles(modsPath, "*.dll"))
            {
                try
                {
                    Assembly.LoadFrom(dll);
                    Loader.Log($"[ToppleBitModding] Loaded mod: {Path.GetFileName(dll)}");
                }
                catch (Exception ex)
                {
                    Loader.Log($"[ToppleBitModding] Failed to load mod {dll}: {ex.Message}");
                }
            }
        }

    }
}
