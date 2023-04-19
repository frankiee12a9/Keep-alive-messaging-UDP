using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KeepAliveMessaging
{
	internal class Util
	{
		public static string loopback { get; } = "127.0.0.1";

		//public static PhysicalAddress GetMacAddressHelper(IPEndPoint clientEP)
		//{
		//	var macAddress = PhysicalAddress.None;
		//	IPAddress ipAddress = clientEP.Address;
		//	var allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		//	foreach (var networkInterface in allNetworkInterfaces)
		//	{
		//		var ipProperties = networkInterface.GetIPProperties();
		//		var unicastAddresses = ipProperties.UnicastAddresses;
		//		foreach (var unicastAddress in unicastAddresses)
		//		{
		//			var isRealHost = unicastAddress.Address.Equals(clientEP);
		//			var isLoopback = ipAddress.Equals(IPAddress.Loopback);
		//			//Console.WriteLine($"unicastAddress.Address: {unicastAddress.Address} - clientEP.Address: {ipAddress} - Loopback: {IPAddress.Loopback}");
		//			if (isRealHost || isLoopback)
		//			{
		//				macAddress = networkInterface.GetPhysicalAddress();
		//				break;
		//			}
		//		}
		//	}
		//	return macAddress;
		//}
	}
}
