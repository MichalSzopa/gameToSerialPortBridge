using System;
using System.IO.Ports;

namespace SteeringWheelInformationBridge;

public class SerialPortHandler : IDisposable
{
    private readonly SerialPort serialPort;

    public SerialPortHandler(string serialPortName)
    {
        serialPort = new SerialPort(serialPortName, Consts.BaudRate, Parity.None, Consts.DataBits, StopBits.One);
        serialPort.Open();
    }

    public void SendDataToUsb(int speed, sbyte gear, int position, byte revLightsPercent)
    {
        serialPort.Write("s" + speed.ToString() +  // speed
                         "g" + (gear == -1 ? "R" : (gear == 0 ? "N" : gear.ToString())) + // gear
                         "p" + position.ToString() + // position
                         "r" + revLightsPercent.ToString()); // rev lights
    }

    public void Dispose()
    {
        serialPort.Close();
        serialPort.Dispose();
    }

    ~SerialPortHandler()
    {
        Dispose();
    }
}
