using System;
using System.IO;
using CommandLine;

namespace PacketGenerator
{
    public enum ProgramType
    {
        None = -1,
        Client = 0,
        GameServer = 1,
    }

    public class Options
    {
        [Option('o', "outputPath", Required = true, HelpText = "Output path for generated files.")]
        public string OutputPath { get; set; }

        [Option('t', "programType", Required = true, HelpText = "0=Client, 1=GameServer")]
        public int ProgramType { get; set; }

        [Option('p', "protoPath", Required = true, HelpText = "Path to Protocol.proto file.")]
        public string ProtoPath { get; set; }
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

        static void RunOptions(Options opts)
        {
            s_outPath = opts.OutputPath;
            s_type = (ProgramType)opts.ProgramType;
        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    RunOptions(opts);

                    string protoPath = Path.GetFullPath(opts.ProtoPath); // OS 상관없이 절대 경로
                    if (!File.Exists(protoPath))
                    {
                        Console.WriteLine($"Error: Protocol file not found at {protoPath}");
                        return;
                    }

                    // Protocol.proto 파일 읽어서 패킷 파싱
                    foreach (string line in File.ReadAllLines(protoPath))
                    {
                        string[] names = line.Split(" ");
                        if (names.Length == 0 || !names[0].StartsWith("message"))
                            continue;

                        ParsePacket(names[1]);
                    }

                    // 출력 폴더 생성
                    string outDir = Path.GetFullPath(s_outPath);
                    Directory.CreateDirectory(outDir);

                    if (s_type == ProgramType.Client)
                    {
                        string clientManagerText = string.Format(PacketFormat.managerFormat, clientMsgIdList, clientPacketManager);
                        File.WriteAllText(Path.Combine(outDir, "ClientPacketManager.cs"), clientManagerText);
                    }
                    else if (s_type == ProgramType.GameServer)
                    {
                        string serverManagerText = string.Format(PacketFormat.managerFormat, gameServerMsgIdList, gameServerPacketManager);
                        File.WriteAllText(Path.Combine(outDir, "GameServerPacketManager.cs"), serverManagerText);
                    }
                });
        }

        public static void ParsePacket(string name)
        {
            if (name.StartsWith("S_")) // GameServer -> Client
            {
                clientPacketManager += string.Format(PacketFormat.managerRegisterFormat, name);
                gameServerMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
                clientMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
                s_protocolId++;
            }
            else if (name.StartsWith("C_")) // Client -> GameServer
            {
                gameServerPacketManager += string.Format(PacketFormat.managerRegisterFormat, name);
                clientMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
                gameServerMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
                s_protocolId++;
            }
        }
    }
}
