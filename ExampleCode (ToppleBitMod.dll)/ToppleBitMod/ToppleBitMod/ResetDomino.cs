using System;
using System.Collections.Generic;
using ToppleBitModding;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ToppleBitMod
{
    internal class ResetDomino: Domino
    {
        private Rotation rotationLastTick;

        public override int SaveID => 6;

        protected override Rotation RotationLastTick => rotationLastTick;

        protected override Dictionary<int, Rotation[]> ToppleMap => Singleton<DominoMetadata>.I.OrthogonalDominoFallMap;

        public ResetDomino(Vector2Int position, Rotation rotation, FallState fallState)
            : base(position, rotation, fallState)
        {
        }

        public override void LateTick()
        {
            rotationLastTick = rotation;
            Rotation[] array = ToppleMap[toppleHash];
            Rotation worldRotation = ((array == null) ? rotation : (array[0] + rotation));
            Singleton<Simulation>.I.AddChange(new DominoChange(this, worldRotation, FallState.Falling));
        }

        public override TileBase GetTile()
        {
            return fallState switch
            {
                FallState.Standing => Singleton<TileData>.I.StandingOrthogonalDominoTile,
                FallState.Falling => Singleton<TileData>.I.FallingOrthogonalDominoTile,
                FallState.Fallen => Singleton<TileData>.I.FallingOrthogonalDominoTile,
                _ => null,
            };
        }

        public override void Topple(Rotation toppleRotation)
        {
            if (fallState == FallState.Standing)
            {
                Rotation relativeRotation = toppleRotation - rotation;
                toppleHash |= Singleton<DominoMetadata>.I.GetHash(relativeRotation);

                if (relativeRotation.Value % 2 == 0)
                {
                    Loader.Log("Reset happening");
                    Singleton<Map>.I.ResetMapObjects();
                }
                Singleton<Simulation>.I.AddLateTickableObject(this);
            }

            Loader.Log($"[ResetDomino] Topple {toppleRotation.Direction}!");
        }

        public override MapObject Clone()
        {
            ResetDomino domino = new ResetDomino(position, rotation, fallState)
            {
                toppleHash = toppleHash,
                rotationLastTick = rotationLastTick
            };
            if (domino.fallState == FallState.Falling)
            {
                domino.LateTick();
            }
            return domino;
        }
    }
}
