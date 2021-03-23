using System;

namespace Server {
    public class User {
        private static int nextId = 0;

        internal DateTime LastHeartbeat;
        internal readonly int Id;
        internal int SkinId;
        internal float PositionX;
        internal float PositionZ;
        
        internal User() {
            // Calculate random position
            var distance = Rand.Float * Constants.SpawnRange;
            var angle = (Rand.Float * (Constants.MaxSpawnAngle - Constants.MinSpawnAngle) + Constants.MinSpawnAngle) * Constants.Deg2Rad;
            var cAngle = (float)Math.Cos(angle);
            var sAngle = (float)Math.Sin(angle);
            
            Id = nextId++;
            SkinId = Rand.Range(Constants.SkinCount);
            PositionX = cAngle * distance;
            PositionZ = sAngle * distance;
        }

        internal void UpdatePosition(float x, float z) {
            PositionX = x;
            PositionZ = z;
        }

        internal void Reskin() {
            int newSkin;
            do {
                newSkin = Rand.Range(Constants.SkinCount);
            } while (newSkin == SkinId);
            SkinId = newSkin;
        }
    }
}