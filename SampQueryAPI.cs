/*
 * Well I didn't like the 'simplicity' in the original SA-MP query for C#.
 * This is intended for those seeking a nice and simple way to query a server.
 * 
 * Anyways, I was bored. Have fun coding! :D
 * 
 * Coded by Lorenc (zeelorenc)
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Launcher;

internal class SampQuery
{
    private readonly bool _bDebug;

    private readonly Dictionary<string, string> _dData = new();
    private readonly ushort _iPort;
    private readonly Socket _svrConnect;

    private readonly string _szIp;

    private readonly DateTime _transmitMs;
    private DateTime _receiveMs;
    private IPEndPoint _serverEndPoint;
    private IPAddress _ServerIP;

    public SampQuery(string ip, ushort port, char packetType, bool consoleDebug = false)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        try
        {
            _ServerIP = new IPAddress(IPAddress.Parse(ip).GetAddressBytes());
            _serverEndPoint = new IPEndPoint(_ServerIP, port);

            _svrConnect = new Socket(_serverEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            _svrConnect.SendTimeout = 5000;
            _svrConnect.ReceiveTimeout = 5000;
            _szIp = ip;
            _iPort = port;
            _bDebug = consoleDebug;

            if (_bDebug) Console.Write("Connecting to " + ip + ":" + port + Environment.NewLine);

            try
            {
                using (stream)
                {
                    using (writer)
                    {
                        var szSplitIp = _szIp.Split('.');

                        writer.Write("SAMP".ToCharArray());

                        writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIp[0])));
                        writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIp[1])));
                        writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIp[2])));
                        writer.Write(Convert.ToByte(Convert.ToInt16(szSplitIp[3])));

                        writer.Write(_iPort);
                        writer.Write(packetType);

                        if (_bDebug)
                            Console.Write("Transmitting Packet '" + packetType + "'" + Environment.NewLine);

                        _transmitMs = DateTime.Now; // To get ping (ms to reach back & forth to the svr)
                    }
                }

                _svrConnect.SendTo(stream.ToArray(), _serverEndPoint);
            }
            catch (Exception e)
            {
                if (_bDebug) Console.Write("Failed to receive packet:", e);
            }
        }
        catch (Exception e)
        {
            if (_bDebug) Console.Write("Failed to connect to IP:", e);
        }
    }

    public Dictionary<string, string> Read(bool flushdata = true)
    {
        try
        {
            _ServerIP = new IPAddress(IPAddress.Parse(_szIp).GetAddressBytes());
            _serverEndPoint = new IPEndPoint(_ServerIP, _iPort);

            EndPoint rawPoint = _serverEndPoint;

            var szReceive = new byte[2048];
            _svrConnect.ReceiveFrom(szReceive, ref rawPoint);

            _svrConnect.Close();

            _receiveMs = DateTime.Now;

            if (flushdata)
                _dData.Clear();

            var ping = _receiveMs.Subtract(_transmitMs).Milliseconds.ToString();

            var stream = new MemoryStream(szReceive);
            var read = new BinaryReader(stream);

            using (stream)
            {
                using (read)
                {
                    read.ReadBytes(10);

                    switch (read.ReadChar())
                    {
                        case 'i':
                            _dData.Add("password", Convert.ToString(read.ReadByte()));
                            _dData.Add("players", Convert.ToString(read.ReadInt16()));
                            _dData.Add("maxplayers", Convert.ToString(read.ReadInt16()));
                            _dData.Add("hostname", new string(read.ReadChars(read.ReadInt32())));
                            _dData.Add("gamemode", new string(read.ReadChars(read.ReadInt32())));
                            _dData.Add("mapname", new string(read.ReadChars(read.ReadInt32())));
                            break;

                        case 'r':
                            for (int i = 0, iRules = read.ReadInt16(); i < iRules; i++)
                                _dData.Add(new string(read.ReadChars(read.ReadByte())),
                                    new string(read.ReadChars(read.ReadByte())));
                            break;

                        case 'c':
                            for (int i = 0, iPlayers = read.ReadInt16(); i < iPlayers; i++)
                                _dData.Add(new string(read.ReadChars(read.ReadByte())),
                                    Convert.ToString(read.ReadInt32()));
                            break;

                        case 'd':
                            for (int i = 0, iTotalPlayers = read.ReadInt16(); i < iTotalPlayers; i++)
                            {
                                var id = Convert.ToString(read.ReadByte());
                                _dData.Add(id + ".name", new string(read.ReadChars(read.ReadByte())));
                                _dData.Add(id + ".score", Convert.ToString(read.ReadInt32()));
                                _dData.Add(id + ".ping", Convert.ToString(read.ReadInt32()));
                            }

                            break;

                        case 'p':
                            _dData.Add("ping", ping);
                            break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (_bDebug) Console.Write("There's been a problem reading the data", e);
        }

        return _dData;
    }
}