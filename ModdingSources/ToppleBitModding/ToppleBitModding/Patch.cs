using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;

namespace ToppleBitModding
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PatchAttribute : Attribute
    {
        public Type TargetType { get; }
        public int Order { get; }
        public PatchAttribute(Type targetType, int order) {
            TargetType = targetType;
            Order = order;
        }

        public PatchAttribute(Type targetType)
        {
            TargetType = targetType;
            Order = 0;
        }

    }

    public static class PatchEngine
    {
        private static List<PatchAttribute> PatchedTypes = new List<PatchAttribute>();

        public static void PatchAll()
        {
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;

                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch
                    {
                        // Mono-safe fallback: skip broken assemblies
                        Loader.Log($"[PatchEngine] Skipping types from {assembly.FullName}");
                        continue;
                    }

                    foreach (var type in types)
                    {
                        var attr = type.GetCustomAttribute<PatchAttribute>();
                        if (attr == null) continue;

                        PatchedTypes.Add(attr);
                        var targetType = attr.TargetType;
                        PatchType(type, targetType);
                    }
                }
                ForceUnityReload();

                Loader.Log("[PatchEngine] All patches applied!");
            }
            catch (Exception ex)
            {
                Loader.Log("[PatchEngine] Error: " + ex.Message + "\nat: " + ex.StackTrace.ToString());
            }
        }

        public static void ForceUnityReload()
        {
            UnityEngine.Object[] allObjects = Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));

            foreach (PatchAttribute PatchedType in PatchedTypes.OrderBy((p) => p.Order))
            {
                foreach (var obj in allObjects)
                {
                    var mb = obj as MonoBehaviour;
                    if (mb == null) continue;

                    if (!PatchedType.TargetType.IsAssignableFrom(mb.GetType())) continue;
                    
                    // Call Awake via reflection
                    var awake = PatchedType.TargetType.GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    try
                    {
                        awake?.Invoke(mb, null);
                    }
                    catch (Exception ex)
                    {
                        Loader.Log($"[PatchEngine] Error while running Awake on {PatchedType.TargetType}:\n{ex.Message} at {ex.StackTrace}");
                    }
                }
            }
        }

        private static MethodInfo ResolveTargetMethod(MethodInfo patch, Type target)
        {
            var patchParams = patch.GetParameters()
                .Where(p =>
                    p.Name != "__instance" &&
                    !typeof(Delegate).IsAssignableFrom(p.ParameterType))
                .Select(p => p.ParameterType)
                .ToArray();

            return target.GetMethods(BindingFlags.Public |
                                     BindingFlags.NonPublic |
                                     BindingFlags.Instance |
                                     BindingFlags.Static)
                .FirstOrDefault(m =>
                {
                    if (m.Name != patch.Name) return false;
                    var mp = m.GetParameters().Select(p => p.ParameterType).ToArray();
                    return mp.SequenceEqual(patchParams);
                });
        }

        private static void PatchType(Type patchType, Type targetType)
        {
            try
            {
                var methods = patchType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (var patchMethod in methods)
                {
                    var targetMethod = ResolveTargetMethod(patchMethod, targetType);
                    if (targetMethod == null) continue;

                    Detour(targetMethod, patchMethod);
                    Loader.Log($"[PatchEngine] Patched {targetType.Name}.{targetMethod.Name}");
                }
            }
            catch (Exception ex)
            {
                Loader.Log($"[PatchEngine] Failed to patch: {ex.Message}\nat: {ex.StackTrace}");
            }
        }


        public static unsafe void Detour(MethodInfo original, MethodInfo replacement)
        {
            IntPtr oriPtr = MonoNative.GetNativePtr(original);
            IntPtr repPtr = MonoNative.GetNativePtr(replacement);

            ProtectRWX(oriPtr, 16);

            if (IntPtr.Size == 8)
            {
                // x64: mov rax, addr; jmp rax
                byte* p = (byte*)oriPtr;
                p[0] = 0x48;
                p[1] = 0xB8;
                *(ulong*)(p + 2) = (ulong)repPtr.ToInt64();
                p[10] = 0xFF;
                p[11] = 0xE0;
            }
            else
            {
                // x86: jmp rel32
                byte* p = (byte*)oriPtr;
                p[0] = 0xE9;
                *(int*)(p + 1) = (int)(repPtr.ToInt64() - oriPtr.ToInt64() - 5);
            }
        }

        private static void ProtectRWX(IntPtr addr, int size)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                VirtualProtect(addr, (UIntPtr)size, 0x40, out _);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect);
    }
}