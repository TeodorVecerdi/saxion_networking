using System.Threading;

public class TCPServerSample  {
	/**
	 * This class implements a simple concurrent TCP Echo server.
	 * Read carefully through the comments below.
	 */
	public static void Main (string[] args)
	{
		var server = new Server(5.0f, true);
		server.StartListening();
		while (true) {
			server.AcceptClients(); // accept new clients
			server.ProcessQueue(); // process queued messages
			server.ProcessClients(); // process clients for new messages
			server.CleanupClients(); // remove faulty/disconnected clients
			
			Thread.Sleep(25);
		}
	}
}


