using System.Collections.Generic;
using ToppleBitModding;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ToppleBitMod
{
    [Patch(typeof(Map), 1)]
    public class MapOverride
    {
        public static void Awake(Map __instance)
        {
            List<MapObject> objmap = FieldAccess.Get<List<MapObject>>(__instance, "sampleMapObjects");
            objmap.Add(new ResetDomino(Vector2Int.zero, Rotation.RIGHT, FallState.Standing));
            objmap.Add(new ResetDomino(Vector2Int.zero, Rotation.RIGHT, FallState.Falling));
            objmap.Add(new ResetDomino(Vector2Int.zero, Rotation.RIGHT, FallState.Fallen));
            Loader.Log("Added Reset Domino");
            __instance.ResetMapObjects();
            FieldAccess.Get<Dictionary<Vector2Int, MapObject>>(__instance, "mapObjects").Clear();


            foreach (Vector3Int position in FieldAccess.Get<Tilemap>(__instance, "tilemap").cellBounds.allPositionsWithin)
            {
                TileBase tile = FieldAccess.Get<Tilemap>(__instance, "tilemap").GetTile(position);
                if (!(tile == null))
                {
                    MapObject mapObject = objmap.Find((MapObject mapObject2) => mapObject2.GetTile() == tile).Clone();
                    Vector2Int position2D = (mapObject.Position = (Vector2Int)position);
                    Matrix4x4 matrix = FieldAccess.Get<Tilemap>(__instance, "tilemap").GetTransformMatrix(position);
                    mapObject.Rotation = Rotation.FromMatrix(matrix);
                    FieldAccess.Get<Dictionary<Vector2Int, MapObject>>(__instance, "mapObjects").Add(position2D, mapObject);
                }
            }
        }
    }
}
