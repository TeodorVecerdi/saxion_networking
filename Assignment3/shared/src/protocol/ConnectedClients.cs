using System.Collections.Generic;

namespace shared.protocol {
    public class ConnectedClients {
        public IReadOnlyList<UserModel> Users { get; }

        public ConnectedClients(IEnumerable<UserModel> users) {
            Users = new List<UserModel>(users).AsReadOnly();
        }
    }
}