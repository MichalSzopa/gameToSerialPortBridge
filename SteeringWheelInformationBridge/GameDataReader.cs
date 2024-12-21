using System.Net;
using System.Net.Sockets;

namespace SteeringWheelInformationBridge;

public class GameDataReader
{
    private readonly UdpClient udpClient;
    private IPEndPoint iPEndPoint;
    private byte[]? received;

    public GameDataReader()
    {
        udpClient = new UdpClient(Consts.UdpPort);
        iPEndPoint = new IPEndPoint(IPAddress.Any, Consts.IPEndPointPort);
    }

    public void ReadGameData(ref ushort speed, ref sbyte gear, ref byte position, ref byte revLightsPercent)
    {
        received = udpClient.Receive(ref iPEndPoint);

        if (received.Length == Consts.PacketLength1)
        {
            // player car index
            byte playerCarIndex = received[Consts.PlayerCarIndexOffset];

            // speed
            byte[] array = new byte[Consts.SpeedArrayLength];
            for (int i = 0; i < Consts.SpeedArrayLength; i++)
            {
                array[i] = received[Consts.SpeedOffset + (playerCarIndex * Consts.CarDataSize1) + i];
            }
            speed = BitConverter.ToUInt16(array, 0);

            // gear
            gear = (sbyte)received[Consts.GearOffset + (playerCarIndex * Consts.CarDataSize1) + Consts.GearIndex];

            // revLightsPercent
            revLightsPercent = received[Consts.RevLightsPercentOffset + (playerCarIndex * Consts.CarDataSize1) + Consts.RevLightsPercentIndex];
        }
        else if (received.Length == Consts.PacketLength2)
        {
            // player car index
            byte playerCarIndex = received[Consts.PlayerCarIndexOffset];

            // player position
            position = received[Consts.PositionOffset + (playerCarIndex * Consts.CarDataSize2) + Consts.PositionIndex];
        }
    }
}
