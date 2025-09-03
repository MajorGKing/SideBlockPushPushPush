using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace PacketGenerator
{
    public enum ProgramType
    {
        None = -1,
        Client = 0,
        GameServer = 1
    }

    class Program
    {
        static string clientPacketManager = "";
        static string gameServerPacketManager = "";
        static string clientMsgIdList = "";
        static string gameServerMsgIdList = "";
        static int s_protocolId = 1;
        static string s_outPath = "";
        static ProgramType s_type = ProgramType.None;

        static void Main(string[] args)
        {
            string outputPath;
            int programType;
            string protoPath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // 윈도우에서는 무조건 6개로 들어옴
                if (args.Length < 6)
                {
                    Console.WriteLine("Usage: -o <outputPath> -t <programType> -p <protoPath>");
                    Environment.Exit(1);
                }

                outputPath = args[1];
                programType = int.Parse(args[3]);
                protoPath = args[5];
            }
            else
            {
                // 맥/리눅스는 셸에서 이미 잘라줌
                var argDict = ParseArgsUnixStyle(args);

                if (!argDict.ContainsKey("-o") || !argDict.ContainsKey("-t") || !argDict.ContainsKey("-p"))
                {
                    Console.WriteLine("Usage: -o <outputPath> -t <programType> -p <protoPath>");
                    Environment.Exit(1);
                }

                outputPath = argDict["-o"];
                programType = int.Parse(argDict["-t"]);
                protoPath = argDict["-p"];
            }

            s_outPath = Path.GetFullPath(outputPath);
            s_type = (ProgramType)programType;

            Console.WriteLine($"[INFO] OutputPath: {s_outPath}");
            Console.WriteLine($"[INFO] ProgramType: {s_type}");
            Console.WriteLine($"[INFO] ProtoPath: {Path.GetFullPath(protoPath)}");

            if (!Directory.Exists(s_outPath))
                Directory.CreateDirectory(s_outPath);

            if (!File.Exists(protoPath))
            {
                Console.WriteLine($"Error: Protocol file not found at {protoPath}");
                Environment.Exit(1);
            }

            foreach (string line in File.ReadAllLines(protoPath))
            {
                if (!line.StartsWith("message ")) continue;
                string name = line.Split(' ')[1];
                ParsePacket(name);
            }

            if (s_type == ProgramType.Client)
            {
                File.WriteAllText(Path.Combine(s_outPath, "ClientPacketManager.cs"),
                    string.Format(PacketFormat.managerFormat, clientMsgIdList, clientPacketManager));
            }
            else if (s_type == ProgramType.GameServer)
            {
                File.WriteAllText(Path.Combine(s_outPath, "GameServerPacketManager.cs"),
                    string.Format(PacketFormat.managerFormat, gameServerMsgIdList, gameServerPacketManager));
            }

            Console.WriteLine("Packet generation completed.");
        }

        static Dictionary<string, string> ParseArgsUnixStyle(string[] args)
        {
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].StartsWith("-"))
                    dict[args[i]] = args[i + 1];
            }
            return dict;
        }

        static void ParsePacket(string name)
        {
            if (name.StartsWith("S_"))
            {
                clientPacketManager += string.Format(PacketFormat.managerRegisterFormat, name);
                clientMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
                gameServerMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
                s_protocolId++;
            }
            else if (name.StartsWith("C_"))
            {
                gameServerPacketManager += string.Format(PacketFormat.managerRegisterFormat, name);
                gameServerMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
                clientMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
                s_protocolId++;
            }
        }
    }
}
