using SerializationSystem;

namespace shared.protocol {
    [System.Serializable]
    public class GameResults {
        [Serialized] public bool IsTie;
        [Serialized] public int WinnerID;
        [Serialized] public string WinnerName;
    }
}