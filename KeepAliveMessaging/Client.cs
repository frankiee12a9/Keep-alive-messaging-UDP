using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KeepAliveMessaging
{
	internal class Client
	{
		public static async Task Main(string[] args)
		{
			// UDP client to send messages to the server
			UdpClient udpClient = new UdpClient();

			// Endpoint for the server's IP address and port
			IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(Util.loopback), 2345);

			// A threadPool to send Keep Alive messages to the server every 5 seconds
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			Task.Run(async () =>
			{
				while (true)
				{
					Console.WriteLine("Sending Keep Alive request to server");

					// Send a Keep Alive request to the server
					byte[] data = new byte[] { 0xFF };
					await udpClient.SendAsync(data, data.Length, serverEP);

					// Wait for 5 seconds before sending the next Keep Alive request
					await Task.Delay(6000);
				}
			});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			while (true)
			{
				Console.Write("Enter a message to send to the server: ");
				string message = Console.ReadLine();

				Console.WriteLine($"sending {message} to (server: {serverEP})");

				// Send the message to the server
				byte[] data = Encoding.ASCII.GetBytes(message);
				await udpClient.SendAsync(data, data.Length, serverEP);

				// Receive a response from the server
				UdpReceiveResult result = await udpClient.ReceiveAsync();
				byte[] responseData = result.Buffer;

				// Check if the response is a Keep Alive response
				if (responseData.Length == 1 && responseData[0] == 0xFF)
				{
					Console.WriteLine("Received Keep Alive response from server");
				}
				else
				{
					string response = Encoding.ASCII.GetString(responseData);
					Console.WriteLine($"Received response from server: {response}");
				}
			}
		}
	}
}
