namespace shared {
    public abstract class Serializer<T> {
        public static readonly int TypeId = typeof(T).GUID.GetHashCode();

        //!! Change <SerializationHelper> if signature changes
        public abstract void Serialize(T obj, Packet packet);
        public abstract T Deserialize(Packet packet);
    }
}