using System;
using shared;

namespace Server {
    public class User {
        private static int nextId = 0;

        internal DateTime LastHeartbeat;
        internal readonly int Id;
        internal int SkinId;
        internal float PositionX;
        internal float PositionY;
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
            PositionY = 0;
            PositionZ = sAngle * distance;
            LastHeartbeat = DateTime.Now;
        }

        internal void Reskin() {
            int newSkin;
            do {
                newSkin = Rand.Range(Constants.SkinCount);
            } while (newSkin == SkinId);
            SkinId = newSkin;
        }

        internal UserModel ToUserModel() {
            return new UserModel(Id, SkinId, PositionX, PositionY, PositionZ);
        }

        public void UpdatePosition(float x, float y, float z) {
            PositionX = x;
            PositionY = y;
            PositionZ = z;
        }
    }
}