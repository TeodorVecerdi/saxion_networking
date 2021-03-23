using System;

namespace Server {
    public class User {
        // Random settings
        private const float spawnRange = 18.0f;
        private const float minSpawnAngle = 0.0f;
        private const float maxSpawnAngle = 180.0f;
        
        private static int nextId = 0;

        internal DateTime LastHeartbeat;
        internal readonly int Id;
        internal int SkinId;
        internal float PositionX;
        internal float PositionZ;
        
        internal User() {
            // Calculate random position
            var distance = Rand.Float * spawnRange;
            var angle = (Rand.Float * (maxSpawnAngle - minSpawnAngle) + minSpawnAngle) * Constants.Deg2Rad;
            var cAngle = (float)Math.Cos(angle);
            var sAngle = (float)Math.Sin(angle);
            
            Id = nextId++;
            SkinId = Rand.Range(4);
            PositionX = cAngle * distance;
            PositionZ = sAngle * distance;
        }

        internal void UpdatePosition(float x, float z) {
            PositionX = x;
            PositionZ = z;
        }
    }
}