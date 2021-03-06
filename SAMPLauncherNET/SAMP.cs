﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinFormsTranslator;

/// <summary>
/// SA:MP launcher .NET namespace
/// </summary>
namespace SAMPLauncherNET
{
    /// <summary>
    /// SA:MP class
    /// </summary>
    public static class SAMP
    {
        /// <summary>
        /// API HTTP URL
        /// </summary>
        public const string APIHTTPURL = "http://lists.sa-mp.com/0.3.7/";

        /// <summary>
        /// Registry key
        /// </summary>
        public const string RegistryKey = "HKEY_CURRENT_USER\\SOFTWARE\\SAMP";

        /// <summary>
        /// Configuration path
        /// </summary>
        public static readonly string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GTA San Andreas User Files\\SAMP";

        /// <summary>
        /// Executable directory
        /// </summary>
        public static readonly string ExeDir = GTASAExe.Substring(0, GTASAExe.Length - 11);

        /// <summary>
        /// SA:MP DLL path
        /// </summary>
        public static readonly string SAMPDLLPath = ExeDir + "\\samp.dll";

        /// <summary>
        /// SA:MP debug path
        /// </summary>
        public static readonly string SAMPDebugPath = ExeDir + "\\samp_debug.exe";

        /// <summary>
        /// Server list API
        /// </summary>
        public const string ServerListAPI = "./api.json";

        /// <summary>
        /// Gallery path
        /// </summary>
        public static readonly string GalleryPath = ConfigPath + "\\screens";

        /// <summary>
        /// Chatlog path
        /// </summary>
        public static readonly string ChatlogPath = ConfigPath + "\\chatlog.txt";

        /// <summary>
        /// Saved positions path
        /// </summary>
        public static readonly string SavedPositionsPath = ConfigPath + "\\savedpositions.txt";

        /// <summary>
        /// SA:MP configuration path
        /// </summary>
        public static readonly string SAMPConfigPath = ConfigPath + "\\sa-mp.cfg";

        /// <summary>
        /// Favourites path
        /// </summary>
        public const string FavouritesPath = ".\\favourites.json";

        /// <summary>
        /// Legacy favourites path
        /// </summary>
        public static readonly string LegacyFavouritesPath = ConfigPath + "\\USERDATA.DAT";

        /// <summary>
        /// Forums URL
        /// </summary>
        public const string ForumsURL = "http://forum.sa-mp.com/index.php";

        /// <summary>
        /// GitHub project URL
        /// </summary>
        public const string GitHubProjectURL = "https://github.com/BigETI/SAMPLauncherNET";

        /// <summary>
        /// API JSON serializer
        /// </summary>
        public static DataContractJsonSerializer apiJSONSerializer = new DataContractJsonSerializer(typeof(APIDataContract[]));

        /// <summary>
        /// Username
        /// </summary>
        public static string Username
        {
            get
            {
                string ret = "";
                try
                {
                    ret = Registry.GetValue(RegistryKey, "PlayerName", "").ToString();
                }
                catch
                {
                    //
                }
                return ret;
            }
            set
            {
                Registry.SetValue(RegistryKey, "PlayerName", value);
            }
        }

        /// <summary>
        /// GTA San Andreas executable
        /// </summary>
        public static string GTASAExe
        {
            get
            {
                string ret = "";
                try
                {
                    ret = Registry.GetValue(RegistryKey, "gta_sa_exe", "").ToString();
                }
                catch
                {
                    //
                }
                return ret;
            }
        }

        /// <summary>
        /// API I/O
        /// </summary>
        public static List<ServerListConnector> APIIO
        {
            get
            {
                List<ServerListConnector> ret = new List<ServerListConnector>();
                try
                {
                    using (FileStream stream = File.Open(ServerListAPI, FileMode.Open))
                    {
                        APIDataContract[] api = (APIDataContract[])(apiJSONSerializer.ReadObject(stream));
                        foreach (APIDataContract apidc in api)
                            ret.Add(new ServerListConnector(apidc));
                    }
                }
                catch
                {
                    ret = RevertAPIs();
                }
                return ret;
            }
            set
            {
                if (value != null)
                {
                    APIDataContract[] api = new APIDataContract[value.Count];
                    for (int i = 0; i < api.Length; i++)
                        api[i] = value[i].APIDataContract;
                    try
                    {
                        foreach (ServerListConnector connector in value)
                        {
                            using (FileStream stream = File.Open(ServerListAPI, FileMode.Create))
                            {
                                apiJSONSerializer.WriteObject(stream, api);
                            }
                        }
                    }
                    catch
                    {
                        //
                    }
                }
            }
        }

        /// <summary>
        /// Chatlog
        /// </summary>
        public static string Chatlog
        {
            get
            {
                string ret = "";
                try
                {
                    using (FileStream fs = File.Open(ChatlogPath, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                        {
                            ret = sr.ReadToEnd();
                        }
                    }
                }
                catch
                {
                    //
                }
                return ret;
            }
        }

        /// <summary>
        /// Saved positions
        /// </summary>
        public static string SavedPositions
        {
            get
            {
                string ret = "";
                try
                {
                    using (FileStream fs = File.Open(SavedPositionsPath, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                        {
                            ret = sr.ReadToEnd();
                        }
                    }
                }
                catch
                {
                    //
                }
                return ret;
            }
        }

        /// <summary>
        /// SA:MP configuration
        /// </summary>
        public static SAMPConfig SAMPConfig
        {
            get
            {
                return new SAMPConfig(SAMPConfigPath);
            }
        }

        /// <summary>
        /// Revert APIs
        /// </summary>
        /// <returns></returns>
        public static List<ServerListConnector> RevertAPIs()
        {
            List<ServerListConnector> ret = new List<ServerListConnector>();
            ret.Add(new ServerListConnector("{$SHOW_FAVOURITES$}", EServerListType.Favourites, FavouritesPath));
            ret.Add(new ServerListConnector("{$SHOW_LEGACY_FAVOURITES$}", EServerListType.LegacyFavourites, LegacyFavouritesPath));
            ret.Add(new ServerListConnector("{$SHOW_LEGACY_HOSTED_LIST$}", EServerListType.LegacySAMP, APIHTTPURL + "hosted"));
            ret.Add(new ServerListConnector("{$SHOW_LEGACY_MASTER_LIST$}", EServerListType.LegacySAMP, APIHTTPURL + "servers"));
            APIIO = ret;
            return ret;
        }

        /// <summary>
        /// Launch SA:MP
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="username">Username</param>
        /// <param name="serverPassword">Server password</param>
        /// <param name="rconPassword">RCON password</param>
        /// <param name="debug">Debug mode</param>
        /// <param name="quitWhenDone">Quit when done</param>
        /// <param name="f">Form to close</param>
        public static void LaunchSAMP(Server server, string username, string serverPassword, string rconPassword, bool debug, bool quitWhenDone, Form f)
        {
            if ((server != null) || debug)
            {
                if (debug || server.IsValid)
                {
                    IntPtr mh = Kernel32.GetModuleHandle("kernel32.dll");
                    if (mh != IntPtr.Zero)
                    {
                        IntPtr pa = Kernel32.GetProcAddress(mh, "LoadLibraryW");
                        if (pa != IntPtr.Zero)
                        {
                            Kernel32.PROCESS_INFORMATION process_info;
                            Kernel32.STARTUPINFO startup_info = new Kernel32.STARTUPINFO();
                            if (Kernel32.CreateProcess(GTASAExe, debug ? "-d" : "-c " + ((rconPassword == null) ? "" : rconPassword) + " -h " + server.IPv4AddressString + " -p " + server.Port + " -n " + username + ((serverPassword == null) ? "" : (" -z " + serverPassword)), IntPtr.Zero, IntPtr.Zero, false, /* DETACHED_PROCESS */ 0x8 | /* CREATE_SUSPENDED */ 0x4, IntPtr.Zero, ExeDir, ref startup_info, out process_info))
                            {
                                IntPtr ptr = Kernel32.VirtualAllocEx(process_info.hProcess, IntPtr.Zero, (uint)(SAMPDLLPath.Length + 1) * 2U, Kernel32.AllocationType.Reserve | Kernel32.AllocationType.Commit, Kernel32.MemoryProtection.ReadWrite);
                                if (ptr != IntPtr.Zero)
                                {
                                    int nobw = 0;
                                    byte[] p = Encoding.Unicode.GetBytes(SAMPDLLPath);
                                    byte[] nt = Encoding.Unicode.GetBytes("\0");
                                    if (Kernel32.WriteProcessMemory(process_info.hProcess, ptr, p, (uint)(p.Length), out nobw) && Kernel32.WriteProcessMemory(process_info.hProcess, new IntPtr(ptr.ToInt64() + p.LongLength), nt, (uint)(nt.Length), out nobw))
                                    {
                                        uint tid = 0U;
                                        IntPtr rt = Kernel32.CreateRemoteThread(process_info.hProcess, IntPtr.Zero, 0U, pa, ptr, /* CREATE_SUSPENDED */ 0x4, out tid);
                                        if (rt != IntPtr.Zero)
                                        {
                                            Kernel32.ResumeThread(rt);
                                            unchecked
                                            {
                                                Kernel32.WaitForSingleObject(rt, (uint)(Timeout.Infinite));
                                            }
                                        }
                                    }
                                    Kernel32.VirtualFreeEx(process_info.hProcess, ptr, 0, Kernel32.AllocationType.Release);
                                }
                                Kernel32.ResumeThread(process_info.hThread);
                                Kernel32.CloseHandle(process_info.hProcess);
                                if (quitWhenDone)
                                    f.Close();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Launch SA:MP debug mode
        /// </summary>
        /// <param name="quitWhenDone">Quit when done</param>
        /// <param name="f">Form to close</param>
        public static void LaunchSAMPDebugMode(bool quitWhenDone, Form f)
        {
            try
            {
                LaunchSAMP(null, "", null, null, true, quitWhenDone, f);
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Launch singleplayer mode
        /// </summary>
        /// <param name="quitWhenDone">Quit when done</param>
        /// <param name="f">Form tom close</param>
        public static void LaunchSingleplayerMode(bool quitWhenDone, Form f)
        {
            try
            {
                Process.Start(GTASAExe);
                if (quitWhenDone)
                    f.Close();
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Show gallery
        /// </summary>
        public static void ShowGallery()
        {
            try
            {
                Process.Start("explorer.exe", GalleryPath);
            }
            catch
            {
                //
            }
        }
    
        /// <summary>
        /// Gallery images
        /// </summary>
        public static Dictionary<string, Image> GalleryImages
        {
            get
            {
                Dictionary<string, Image> ret = new Dictionary<string, Image>();
                string[] files = Directory.GetFiles(GalleryPath, "*.png");
                foreach (string file in files)
                {
                    try
                    {
                        ret.Add(file, Utils.GetThumbnail(Image.FromFile(file)));
                    }
                    catch
                    {
                        //
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// Open forums
        /// </summary>
        public static void OpenForums()
        {
            try
            {
                Process.Start(ForumsURL);
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Open GitHub project
        /// </summary>
        public static void OpenGitHubProject()
        {
            try
            {
                Process.Start(GitHubProjectURL);
            }
            catch
            {
                //
            }
        }
    }
}
