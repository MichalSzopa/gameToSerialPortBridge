using SteeringWheelInformationBridge;

// serial port initialization and validation
string? com;
Console.Write(" Which port is the controller connected to?(COMx): ");
com = Console.ReadLine();

if (string.IsNullOrWhiteSpace(com) || !com.StartsWith("COM") || com.Length < 4)
{
    Console.WriteLine("Invalid port");
    return;
}

Console.WriteLine("Reader is running, type \"exit\" to close the program");

// classes and variables initialization
var serialPortHandler = new SerialPortHandler(com);
var gameDataReader = new GameDataReader();

ushort speed = Consts.InitialSpeed;
sbyte gear = Consts.InitialGear;
byte position = Consts.InitialPosition;
byte revLightsPercent = Consts.InitialRevLightsPercent;
int i = 0;

// initial data send
serialPortHandler.SendDataToUsb(speed, gear, position, revLightsPercent);

// exit program condition
bool exitRequested = false;
Task.Run(() =>
{
    while (true)
    {
        string? input = Console.ReadLine();
        if (input?.ToLower() == "exit")
        {
            exitRequested = true;
            break;
        }
    }
});
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    exitRequested = true;
};

// main loop
while (!exitRequested)
{
    i++;
    gameDataReader.ReadGameData(ref speed, ref gear, ref position, ref revLightsPercent);

    if (i == Consts.UpdateInterval)
    {
        serialPortHandler.SendDataToUsb(speed, gear, position, revLightsPercent);
        i = 0;
    }

    Thread.Sleep(10); // Add a small delay to reduce CPU usage
}

serialPortHandler.Dispose();
Console.WriteLine("Program exited");
