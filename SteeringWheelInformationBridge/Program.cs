using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;

namespace SteeringWheelInformationBridge
{
    class Program
    {
        static void Main(string[] args)
        {
            String com;
            Console.Write(" Which port is the controller connected to?(COMx): ");
            com = Console.ReadLine();
            SerialPortStuff serialPortStuff = new SerialPortStuff(com);

            GameInfoStuff gameInfoStuff = new GameInfoStuff();

            
            //sbyte oldGear = 1;
            //byte oldPosition = 14;

            ushort speed = 10;
            sbyte gear = 2;
            byte position = 13;
            byte revLightsPercent = 0;
            int i = 0;

            serialPortStuff.SendDataToUsb(speed, gear, position, revLightsPercent);

            while (true)
            {
                i++;

                gameInfoStuff.ReadCarInfo(ref speed, ref gear, ref position, ref revLightsPercent);

                if (i == 5)
                {
                    //if (gear == oldGear && position == oldPosition)
                    //    serialPortStuff.SendSpeedAndRevsToUsb(speed, revLightsPercent);
                    //else
                        serialPortStuff.SendDataToUsb(speed, gear, position, revLightsPercent);
                    i = 0;

                    //oldGear = gear;
                    //oldPosition = position;
                }

                
            }
        }
    }

    class SerialPortStuff
    {
        private SerialPort serialPort;

        public SerialPortStuff(String com)
        {
            serialPort = new SerialPort(com, 9600, Parity.None, 8, StopBits.One);
            serialPort.Open();
        }

        public void SendDataToUsb(int speed, sbyte gear, int position, byte revLightsPercent)
        {
            serialPort.Write("s" + speed.ToString() + "g" + (gear == -1 ? "R" : (gear == 0 ? "N" : gear.ToString())) + "p" + position.ToString() + "r" + revLightsPercent.ToString());
        }

        public void SendSpeedAndRevsToUsb(int speed, byte revLightsPercent)
        {
            serialPort.Write("s" + speed.ToString() + "r" + revLightsPercent.ToString());
        }

        ~SerialPortStuff()
        {
            serialPort.Close();
        }

    }

    class GameInfoStuff
    {
        private UdpClient Client;
        private IPEndPoint RemoteIP;
        private byte[] received;

        public GameInfoStuff()
        {
            Client = new UdpClient(20777);
            RemoteIP = new IPEndPoint(IPAddress.Any, 60240);
        }

        public void ReadCarInfo(ref ushort speed, ref sbyte gear, ref byte position, ref byte revLightsPercent)
        {
            received = Client.Receive(ref RemoteIP);

            if (received.Length == 1347)
            {
                // player car index
                byte playerCarIndex = received[22];


                // speed
                byte[] array = new byte[2];
                for(int i=0; i<2;i++)
                {
                    array[i] = received[23 + (playerCarIndex * 66) + i];
                }
                speed = BitConverter.ToUInt16(array, 0);


                // gear
                gear = (sbyte)received[23 + (playerCarIndex * 66) + 15];


                // revLightsPercent
                revLightsPercent = received[23 + (playerCarIndex * 66) + 19];
            }
            else if (received.Length == 843)
            {
                // player car index
                byte playerCarIndex = received[22];


                // position
                position = received[23 + (playerCarIndex * 41) + 32];
            }
        }
    }
}

/*
    [Serializable]
    public struct PacketHeader // 23 B
    {
        public ushort m_packetFormat;         // 2019   bytes: 0,1
        public byte m_gameMajorVersion;     // Game major version - "X.00"   bytes: 2
        public byte m_gameMinorVersion;     // Game minor version - "1.XX"   bytes: 3
        public byte m_packetVersion;        // Version of this packet type, all start from 1   bytes: 4
        public byte m_packetId;             // Identifier for the packet type, see below   bytes: 5
        public ulong m_sessionUID;           // Unique identifier for the session   bytes: 6,7,8,9,10,11,12,13
        public float m_sessionTime;          // Session timestamp   bytes: 14,15,16,17
        public uint m_frameIdentifier;      // Identifier for the frame the data was retrieved on   bytes: 18,19,20,21
        public byte m_playerCarIndex;       // Index of player's car in the array   bytes: 22

        public PacketHeader(ushort packetFormat, byte gameMajorVersion, byte gameMinorVersion, byte packetVersion, byte packetId, ulong sessionUID, float sessionTime, uint frameIdentifier, byte playerCarIndex)
        {
            m_packetFormat = packetFormat;
            m_gameMajorVersion = gameMajorVersion;
            m_gameMinorVersion = gameMinorVersion;
            m_packetVersion = packetVersion;
            m_packetId = packetId;
            m_sessionUID = sessionUID;
            m_sessionTime = sessionTime;
            m_frameIdentifier = frameIdentifier;
            m_playerCarIndex = playerCarIndex;
        }
    };

    [Serializable]
    public struct CarTelemetryData // 2 + 12 + 2 + 2 + 2 + 24 + 2 + 16 + 4 = 66 B
    {
        public ushort m_speed;                    // Speed of car in kilometres per hour   bytes: 0,1
        public float m_throttle;                 // Amount of throttle applied (0.0 to 1.0)   bytes: 2,3,4,5
        public float m_steer;                    // Steering (-1.0 (full lock left) to 1.0 (full lock right))   bytes: 6,7,8,9
        public float m_brake;                    // Amount of brake applied (0.0 to 1.0)   bytes: 10,11,12,13
        public byte m_clutch;                   // Amount of clutch applied (0 to 100)   bytes: 14
        public sbyte m_gear;                     // Gear selected (1-8, N=0, R=-1)   bytes: 15
        public ushort m_engineRPM;                // Engine RPM   bytes: 16,17
        public byte m_drs;                      // 0 = off, 1 = on   bytes: 18
        public byte m_revLightsPercent;         // Rev lights indicator (percentage)   bytes: 19
        public ushort[] m_brakesTemperature;     // Brakes temperature (celsius)   bytes: 20-27
        public ushort[] m_tyresSurfaceTemperature; // Tyres surface temperature (celsius)   bytes: 28-35
        public ushort[] m_tyresInnerTemperature; // Tyres inner temperature (celsius)   bytes: 36-43
        public ushort m_engineTemperature;        // Engine temperature (celsius)   bytes: 44,45
        public float[] m_tyresPressure;         // Tyres pressure (PSI)   bytes: 46-61
        public byte[] m_surfaceType;           // Driving surface, see appendices   bytes: 62,63,64,65

        public CarTelemetryData(ushort speed, float throttle, float steer, float brake, byte clutch, sbyte gear, ushort engineRPM, byte drs, byte revLightsPercent, ushort[] brakesTemperature, ushort[] tyresSurfaceTemperature, ushort[] tyresInnerTemperature, ushort engineTemperature, float[] tyresPressure, byte[] surfaceType)
        {
            m_speed = speed;
            m_throttle = throttle;
            m_steer = steer;
            m_brake = brake;
            m_clutch = clutch;
            m_gear = gear;
            m_engineRPM = engineRPM;
            m_drs = drs;
            m_revLightsPercent = revLightsPercent;
            m_brakesTemperature = brakesTemperature;
            m_tyresSurfaceTemperature = tyresSurfaceTemperature;
            m_tyresInnerTemperature = tyresInnerTemperature;
            m_engineTemperature = engineTemperature;
            m_tyresPressure = tyresPressure;
            m_surfaceType = surfaceType;
        }
    };

    [Serializable]
    public struct PacketCarTelemetryData // 23 + 20*66 + 4 = 1347
    {
        public PacketHeader m_header;        // Header   bytes:  0-22

        public CarTelemetryData[] m_carTelemetryData;   //   bytes:   23-1342

        public uint m_buttonStatus;        // Bit flags specifying which buttons are being pressed   bytes: 1343-1346

        public PacketCarTelemetryData(PacketHeader header, CarTelemetryData[] carTelemetryData, uint buttonStatus)
        {
            m_header = header;
            m_carTelemetryData = carTelemetryData;
            m_buttonStatus = buttonStatus;
        }
        // currently - see appendices


    };

struct LapData 41 B
{
    float       m_lastLapTime;	       	// Last lap time in seconds  0-3
    float       m_currentLapTime;	// Current time around the lap in seconds 4-7
    float       m_bestLapTime;		// Best lap time of the session in seconds 8-11
    float       m_sector1Time;		// Sector 1 time in seconds 12-15
    float       m_sector2Time;		// Sector 2 time in seconds 16-19
    float       m_lapDistance;		// Distance vehicle is around current lap in metres – could 20-23
					// be negative if line hasn’t been crossed yet
    float       m_totalDistance;		// Total distance travelled in session in metres – could 24-27
					// be negative if line hasn’t been crossed yet
    float       m_safetyCarDelta;        // Delta in seconds for safety car 28-31
    uint8       m_carPosition;   	// Car race position 32
    uint8       m_currentLapNum;		// Current lap number 33
    uint8       m_pitStatus;            	// 0 = none, 1 = pitting, 2 = in pit area 34
    uint8       m_sector;               	// 0 = sector1, 1 = sector2, 2 = sector3 35
    uint8       m_currentLapInvalid;    	// Current lap invalid - 0 = valid, 1 = invalid 36
    uint8       m_penalties;            	// Accumulated time penalties in seconds to be added 37
    uint8       m_gridPosition;         	// Grid position the vehicle started the race in 38
    uint8       m_driverStatus;         	// Status of driver - 0 = in garage, 1 = flying lap 39
// 2 = in lap, 3 = out lap, 4 = on track
    uint8       m_resultStatus;          // Result status - 0 = invalid, 1 = inactive, 2 = active 40
// 3 = finished, 4 = disqualified, 5 = not classified
// 6 = retired
};


struct PacketLapData 
{
    PacketHeader    m_header;              // Header 0-22

    LapData         m_lapData[20];         // Lap data for all cars on track 23-842
};

    */
