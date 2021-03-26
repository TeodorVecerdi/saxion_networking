namespace shared.serialization {
    public interface ISerializationCallback {
        void OnBeforeSerialize();
        void OnAfterDeserialize();
    }
}