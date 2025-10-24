using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace InventorySystem2.Models;

public class Robot
{
    private readonly string _host;
    private readonly int _port;

    public Robot(string host = "localhost", int port = 30002)
    {
        _host = host;
        _port = port;

        // Sørg for at URScript bruger punktum som decimal
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
    }

    public void SendProgram(string program, uint itemId = 0)
    {
        try
        {
            using var client = new TcpClient(_host, _port);
            var fullProgram = $"def pick_item_{itemId}():\n{program}\nend\n";
            var bytes = Encoding.ASCII.GetBytes(fullProgram);
            client.GetStream().Write(bytes, 0, bytes.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Robotfejl: {ex.Message}");
        }
    }
}

