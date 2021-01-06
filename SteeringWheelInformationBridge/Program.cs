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
            SerialPortHandler serialPortHandler = new SerialPortHandler(com);

            GameDataReader gameDataReader = new GameDataReader();

            ushort speed = 10;
            sbyte gear = 2;
            byte position = 13;
            byte revLightsPercent = 0;
            int i = 0;

            serialPortHandler.SendDataToUsb(speed, gear, position, revLightsPercent);

            while (true)
            {
                i++;

                gameDataReader.ReadGameData(ref speed, ref gear, ref position, ref revLightsPercent);

                if (i == 5)
                {
                    //if (gear == oldGear && position == oldPosition)
                    //    serialPortStuff.SendSpeedAndRevsToUsb(speed, revLightsPercent);
                    //else
                        serialPortHandler.SendDataToUsb(speed, gear, position, revLightsPercent);
                    i = 0;

                    //oldGear = gear;
                    //oldPosition = position;
                }

                
            }
        }
    }

    class SerialPortHandler
    {
        private SerialPort serialPort;

        public SerialPortHandler(String com)
        {
            serialPort = new SerialPort(com, 9600, Parity.None, 8, StopBits.One);
            serialPort.Open();
        }

        public void SendDataToUsb(int speed, sbyte gear, int position, byte revLightsPercent)
        {
            serialPort.Write("s" + speed.ToString() + "g" + (gear == -1 ? "R" : (gear == 0 ? "N" : gear.ToString())) + "p" + position.ToString() + "r" + revLightsPercent.ToString());
        }

        ~SerialPortHandler()
        {
            serialPort.Close();
        }

    }

    class GameDataReader
    {
        private UdpClient udpClient;
        private IPEndPoint iPEndPoint;
        private byte[] received;

        public GameDataReader()
        {
            udpClient = new UdpClient(20777);
            iPEndPoint = new IPEndPoint(IPAddress.Any, 60240);
        }

        public void ReadGameData(ref ushort speed, ref sbyte gear, ref byte position, ref byte revLightsPercent)
        {
            received = udpClient.Receive(ref iPEndPoint);

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