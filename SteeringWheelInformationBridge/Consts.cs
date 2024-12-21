namespace SteeringWheelInformationBridge;

public static class Consts
{
    public static ushort InitialSpeed => 10;
    public static sbyte InitialGear => 2;
    public static byte InitialPosition => 13;
    public static byte InitialRevLightsPercent => 0;
    public static int UpdateInterval => 5;
    public static int BaudRate => 9600;
    public static int DataBits => 8;
    public static int UdpPort => 20777;
    public static int IPEndPointPort => 60240;
    public static int PacketLength1 => 1347;
    public static int PacketLength2 => 843;
    public static int PlayerCarIndexOffset => 22;
    public static int SpeedArrayLength => 2;
    public static int SpeedOffset => 23;
    public static int GearOffset => 23;
    public static int GearIndex => 15;
    public static int RevLightsPercentOffset => 23;
    public static int RevLightsPercentIndex => 19;
    public static int PositionOffset => 23;
    public static int PositionIndex => 32;
    public static int CarDataSize1 => 66;
    public static int CarDataSize2 => 41;
}