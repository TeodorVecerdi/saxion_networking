using System.Threading;

public class TCPServerSample  {
	/**
	 * This class implements a simple concurrent TCP Echo server.
	 * Read carefully through the comments below.
	 */
	public static void Main (string[] args)
	{
		var server = new Server(25.0f, true);
		server.StartListening();
		while (true) {
			server.AcceptClients();
			server.ProcessClients();
			server.CleanupClients();
			
			//Although technically not required, now that we are no longer blocking, 
			//it is good to cut your CPU some slack
			Thread.Sleep(10);
		}
	}
}


