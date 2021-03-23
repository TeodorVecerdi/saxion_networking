namespace shared.serialization {
    public abstract class Serializer<T> {
        //!! Change <SerializationHelper> if signature changes
        public abstract void Serialize(T obj, Packet packet);
        public abstract T Deserialize(Packet packet);
    }
}