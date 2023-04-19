using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace KeepAliveMessaging
{
	internal class Server
	{
		// Dictionary to store the Keep Alive status of each client
		static ConcurrentDictionary<Tuple<IPEndPoint, PhysicalAddress>, DateTime> clientStatus = new();

		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		private static readonly object _lock = new object();

		static async Task Main(string[] args)
		{
			// UDP client to listen for incoming messages
			UdpClient udpServer = new UdpClient(2345);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			Task.Run(async () =>
			{
				Console.WriteLine($"Start receiving incomming message from clients");
				while (true)
				{
					// Receive an incoming message from a client
					UdpReceiveResult result = await udpServer.ReceiveAsync();
					IPEndPoint clientEP = result.RemoteEndPoint;
					byte[] data = result.Buffer;

					// Get the MAC address of the client
					PhysicalAddress clientMAC = GetMacAddress(clientEP);

					// Create a tuple to store the IP endpoint and MAC address of the client
					var clientKey = Tuple.Create(clientEP, clientMAC);

					// Check if the message is a Keep Alive message
					if (data.Length == 1 && data[0] == 0xFF)
					{
						Console.WriteLine($"Received Keep Alive message from client {clientKey.Item1} - MAC:{clientKey.Item2} (Client)");

						// Update the Keep Alive status of the client
						clientStatus[clientKey] = DateTime.Now;
						DateTime value;
						Console.WriteLine($"Keep-Alive status from client {clientKey.Item1} is: {clientStatus[clientKey]}");
					}
				}
			});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			// Set the initial Keep Alive timeout and retry count
			int keepAliveTimeout = 10;
			int retryCount = 2;

			// Start a task to monitor the Keep Alive status of each client
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			Task.Run(async () =>
			{
				while (true)
				{
					foreach (var entry in clientStatus)
					{
						var clientKey = entry.Key;
						DateTime lastKeepAlive = entry.Value;

						// Calculate the time since the last Keep Alive message was received
						TimeSpan timeSinceLastKeepAlive = DateTime.Now - lastKeepAlive;

						// Check if the Keep Alive timeout has been exceeded
						if (timeSinceLastKeepAlive.TotalSeconds > keepAliveTimeout)
						{
							Console.WriteLine($"timeSinceLastKeepAlive: {timeSinceLastKeepAlive.TotalSeconds} -- KeepAliveTimeout {keepAliveTimeout}");
							Console.WriteLine($"Keep-Alive timeout exceeded for client {clientKey}");

							// Send a Keep Alive request to the client
							byte[] data = new byte[] { 0xFF };
							await udpServer.SendAsync(data, data.Length, clientKey.Item1);
							Console.WriteLine($"Sent Keep-Alive message to the not responding client {clientKey}: {data}");

							// Wait for a response from the client
							bool keepAliveReceived = false;
							Console.WriteLine($"Start waiting for a response from the client {clientKey}");
							for (int i = 0; i < retryCount; i++)
							{
								if (udpServer.Available > 0)
								{
									UdpReceiveResult result = await udpServer.ReceiveAsync();
									byte[] responseData = result.Buffer;

									// Check if the response is a Keep Alive response
									if (responseData.Length == 1 && responseData[0] == 0xFF)
									{
										Console.WriteLine($"Received Keep Alive response from {result.RemoteEndPoint}");
										keepAliveReceived = true;
										break;
									}
								}

								await Task.Delay(1000);
							}

							if (!keepAliveReceived)
							{
								Console.WriteLine($"No Keep-Alive response received from client {clientKey}");
								DateTime value;
								var isRemoved = clientStatus.TryRemove(clientKey, out value);
								Console.WriteLine($"Client {clientKey.Item1} disconnected...removed this client ({isRemoved}) ");
							}
						}
					}

					await Task.Delay(1000);
				}
			});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			Console.WriteLine("Server is listening for incoming messages...");

			while (true)
			{
				UdpReceiveResult result;
				try
				{
					result = await udpServer.ReceiveAsync();
				}
				catch (SocketException ex) when (ex.Message.Contains("An existing connection was forcibly closed by the remote host"))
				{
					// Handle the exception gracefully
					Console.WriteLine("An existing connection was forcibly closed by the remote host. Removing client from clientStatus.");
					continue;
				}

				// Receive a message from a client asynchronously
				byte[] data = result.Buffer;

				// Create a tuple to store the IP endpoint and MAC address of the client
				var clientMAC = GetMacAddress(result.RemoteEndPoint);
				var clientKey = Tuple.Create(result.RemoteEndPoint, clientMAC);

				// Update the Keep Alive status of the client
				clientStatus[clientKey] = DateTime.Now;

				// Check if the message is a Keep Alive request
				if (data.Length == 1 && data[0] == 0xFF)
				{
					Console.WriteLine($"Received Keep-Alive request from {result.RemoteEndPoint}");

					// Send a Keep Alive response back to the client asynchronously
					byte[] responseData = new byte[] { 0xFF };
					await udpServer.SendAsync(responseData, responseData.Length, result.RemoteEndPoint);
				}
				else
				{
					string message = Encoding.ASCII.GetString(data);
					Console.WriteLine($"Received {message} from {result.RemoteEndPoint}");

					// NOTE: When change the UDP port, it might be closing the existing socket and creating a new one.
					// This could cause any ongoing communication on the old socket to be terminated, resulting in the `SocketException`
					if (message.StartsWith("port:"))
					{
						int port = int.Parse(message.Split(':')[1].Trim());
						Console.WriteLine($"[updated] Changed UDP port to {port}");
						lock (_lock)
						{
							//udpServer.Client.Bind(new IPEndPoint(IPAddress.Any, port));
							udpServer = new UdpClient(port);
						}
					}
					else if (message.StartsWith("timeout:"))
					{
						keepAliveTimeout = int.Parse(message.Split(':')[1].Trim());
						Console.WriteLine($"[updated] Changed Keep Alive timeout to {keepAliveTimeout} seconds");
					}
					else if (message.StartsWith("retry:"))
					{
						Console.WriteLine($"[updated] Changed retry count to {retryCount}");
						retryCount = int.Parse(message.Split(':')[1].Trim());
					}
					else
					{
						// Send a response back to the client asynchronously
						string response = "Message received!";
						byte[] responseData = Encoding.ASCII.GetBytes(response);
						await udpServer.SendAsync(responseData, responseData.Length, result.RemoteEndPoint);
					}
				}
			}
		}

		public static PhysicalAddress GetMacAddress(IPEndPoint clientEP)
		{
			var macAddress = PhysicalAddress.None;
			IPAddress ipAddress = clientEP.Address;
			var allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (var networkInterface in allNetworkInterfaces)
			{
				var ipProperties = networkInterface.GetIPProperties();
				var unicastAddresses = ipProperties.UnicastAddresses;
				foreach (var unicastAddress in unicastAddresses)
				{
					var isRealHost = unicastAddress.Address.Equals(clientEP);
					var isLoopback = ipAddress.Equals(IPAddress.Loopback);
					if (isRealHost || isLoopback)
					{
						macAddress = networkInterface.GetPhysicalAddress();
						break;
					}
				}
			}
			return macAddress;
		}
	}
}
