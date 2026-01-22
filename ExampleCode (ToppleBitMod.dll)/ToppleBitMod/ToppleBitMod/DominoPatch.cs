using ToppleBitModding;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.Tilemaps;

namespace ToppleBitMod
{
    [Patch(typeof(Domino))]
    public class DominoPatch
    {
        public static void ApplyChange(Domino __instance, DominoChange change)
        {
            FieldAccess.Set(__instance, "fallState", change.FallState);
            Loader.Log($"[DominoPatch] ApplyChange: {change.Rotation.Direction} ignored!");
            Loader.Log($"[DominoPatch] ApplyChange: Domino is still in {FieldAccess.Get<Rotation>(__instance, "rotation").Direction} !");
        }

        public static void Topple(Domino __instance, Rotation toppleRotation)
        {
            var fallState = FieldAccess.Get<FallState>(__instance, "fallState");

            if (fallState == FallState.Standing)
            {
                var rotation = FieldAccess.Get<Rotation>(__instance, "rotation");
                var toppleHash = FieldAccess.Get<int>(__instance, "toppleHash");
                Rotation relativeRotation = toppleRotation - rotation;
                toppleHash |= Singleton<DominoMetadata>.I.GetHash(relativeRotation);

                FieldAccess.Set(__instance, "toppleHash", toppleHash);
                Singleton<Simulation>.I.AddLateTickableObject(__instance);
            }
            Loader.Log($"[DominoPatch] Topple {toppleRotation.Direction}!");
        }
    }
}
