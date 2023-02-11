using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using Windows.Networking.Sockets;

namespace Pear
{
    public static class PearDefinitions
    {
        public const int DiscoverPort = 17002;
        public static readonly byte[] Header = new byte[] { 0xF0, 0x9F, 0x8D, 0x90 };
    }

    public class PearId
    {

    }

    public class PearDevice
    {
        public PearId ID { get; set; } = null!;
        public int ProtocolVersion { get; set; }
        public string Name { get; set; } = null!;

        public IEnumerable<byte> ToDackBytes()
        {
            var name = Encoding.UTF8.GetBytes(Name);
            var staticHeader = new byte[]
            {
                0xF0, 0x9F, 0x8D, 0x90, // pear header
                0x01, // version
                0x01, // message type (DACK)
                1, 2, 3, 4, // device id [INCOMPLETE]
                (byte)name.Length, // name length

            };
            return staticHeader.Concat(name);
        }
    }

    public class Peard
    {

        private static readonly Lazy<Peard> _this = new Lazy<Peard>(() => new Peard());
        public static Peard Instance {
            get { return _this.Value; }
        }

        public static void Start(string name)
        {
            Debug.WriteLine($"[peard] starting pear services on device {name}");
            // trigger constructor
            _ = Peard.Instance;
            Peard.Instance._self.Name = name;

            Debug.WriteLine("[peard] starting discovery server");
            Peard.Instance.InitializeUdp();
        }

        private PearDevice _self;

        private UdpClient _udp;
        //private TcpListener _tcp;

        private Peard()
        {
            _self = new PearDevice()
            {
                ID = new PearId(),
                ProtocolVersion = 0,
                Name = "Unknown Device"
            };

            _udp = new UdpClient(new IPEndPoint(IPAddress.Any, PearDefinitions.DiscoverPort));
            _udp.EnableBroadcast = true;
            Debug.WriteLine($"[peard] pear services OK");
        }


        private void InitializeUdp()
        {
            var task = Task.Run(() =>
            {
                while (true)
                {
                    var from = new IPEndPoint(0, 0);
                    var recvBuffer = _udp.Receive(ref from);
                    Debug.WriteLine($"[peard][udp] incoming broadcast from {from}");
                    Debug.WriteLine($"[peard][udp] dump payload: {BitConverter.ToString(recvBuffer)}");
                    if (recvBuffer[0] != PearDefinitions.Header[0] ||
                        recvBuffer[1] != PearDefinitions.Header[1] ||
                        recvBuffer[2] != PearDefinitions.Header[2] ||
                        recvBuffer[3] != PearDefinitions.Header[3])
                        continue; // ignore stuff not meant for pear
                    var version = recvBuffer[4];
                    var type = recvBuffer[5]; // we can ignore this for now as we dont have any other udp types
                    Debug.WriteLine($"[peard][udp] received DISC (version {version})");
                    // only one version exists right now so thats all i care about

                    // next up is the device ID. im going to just use 64 bytes
                    // well, im using just 4 for testing for now
                    var id = recvBuffer[6..9];

                    // and i do not care about the rest of it right now
                    // signing? identities? pfft who needs em

                    var client = new TcpClient();
                    client.Connect(from);
                    var stream = client.GetStream();
                    var data = _self.ToDackBytes().ToArray();
                    stream.Write(data, 0, data.Length);
                    Debug.WriteLine("[peard][tcp] wrote DACK");
                    client.Close();
                }
            });
        }

        private void DISC_Recv()
        {

        }
    }
}
