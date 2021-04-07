using System.Collections.Generic;

namespace shared.protocol {
    public class Command {
        public string CommandName { get; }
        public List<string> Parameters { get; }

        public Command(string command, List<string> parameters) {
            CommandName = command;
            Parameters = parameters;
        }
    }
}