using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Args;
using ESocket.Pack;
using Windows.Networking;

namespace ESocket.Controller
{
	class DebugTransmitter : ITransmitter
	{
		private static Pack.Package pack;

		public uint DownloadSpeed
		{
			get
			{
				System.Diagnostics.Debug.WriteLine("Get download speed : 16384 bytes per second");
				return 16384;
			}
		}

		public HostName LocalHostName
		{
			get
			{
				return new HostName("192.168.3.1");
			}
		}

		public string LocalServiceName
		{
			get
			{
				return "65535";
			}
		}

		public HostName RemoteHostName
		{
			get
			{
				return new HostName("192.168.3.1");
			}
		}

		public string RemoteServiceName
		{
			get
			{
				return "32768";
			}
		}

		public ulong TotalDownload
		{
			get
			{
				System.Diagnostics.Debug.WriteLine("Get download count : 65536 bytes");
				return 65536;
			}
		}

		public uint TotalRunningTime
		{
			get
			{
				return 10240;
			}
		}

		public ulong TotalUpload
		{
			get
			{
				return 0;
			}
		}

		public uint UploadSpeed
		{
			get
			{
				return 256;
			}
		}

		public event EventHandler<ConnectionTimeoutEventArgs> OnConnectionTimeout;
		public event EventHandler<PackageReceivedEventArgs> OnPackageReceived;
		public event EventHandler<SocketExceptionEventArgs> OnSocketException;

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public Package RecvPackage()
		{
			return new Package(0, 10, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
		}

		public void SendPackage(Package pack)
		{
			//System.Diagnostics.Debug.WriteLine("New Package Send , Sequence : {0} , Size : {1} , DataLength : {2} , Data : {3} .", pack.Sequence, pack.Size, pack.Data.Length, pack.Data);
			DebugTransmitter.pack = pack;
		}

		public async Task StartAutoRecvAsync()
		{
			await Task.Run(() =>
			{
				while (true)
				{
					Task.Delay(10).Wait();
					if(pack != null)
					{
						OnPackageReceived?.Invoke(this, new PackageReceivedEventArgs(DateTime.Now, pack, this.RemoteHostName, this.RemoteServiceName, this.LocalServiceName));
						pack = null;
					}
				}
			});
		}

		public void StopAutoRecv()
		{
			throw new NotImplementedException();
		}
	}
}
