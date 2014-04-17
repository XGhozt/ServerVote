using System;
using System.Timers;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Newtonsoft.Json;
using System.Web;
using System.Net;



namespace ServerVote
{
    [ApiVersion(1, 15)]
    public class ServerVote : TerrariaPlugin
    {
        public string path = Path.Combine(TShock.SavePath, "ServerVote.txt");
        public override string Name
        {
            get
            {
                return "TServerWebVote";
            }
        }
        public override string Author
        {
            get
            {
                return "Loganizer + XGhozt";
            }
        }
        public override string Description
        {
            get
            {
                return "A plugin to vote to TServerWeb in-game.";
            }
        }
        public override Version Version
        {
            get
            {
                return new Version("1.0");
            }
        }
        public ServerVote(Main game)
            : base(game)
        {
            Order = 1000;
        }
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            }
            base.Dispose(disposing);
        }       
        public void OnInitialize(EventArgs args)
        {
            int ID;
            string message;
            if (!File.Exists(Path.Combine(TShock.SavePath, "ServerVote.txt")))
            {
                string[] text = {"**This is the configuration file, please do not edit.**", "Help page: http://www.tserverweb.com/help/",
                                    "Server ID is on next line. Please DO NOT edit the following line, change it using /tserverweb [ID] in-game",
                                "0"};
                File.WriteAllLines(Path.Combine(TShock.SavePath, "ServerVote.txt"), text);
            }
            else
            {
                if (!GetServerID(out ID, out message))
                    SendError("Configuration", message);
            }
            if (TShock.Config.RestApiEnabled == false)
            {
                SendError("[TServerWeb] REST API", "REST API Not Enabled! ServerVote plugin will not load!");
            }
            else
            {
                Commands.ChatCommands.Add(new Command("server.vote", Vote, "vote"));
                Commands.ChatCommands.Add(new Command("vote.changeid", ChangeID, "tserverweb"));
            }
        }
        public void Vote(CommandArgs e)
        {
            int ID;
            string message;
            try
            {
                if (!GetServerID(out ID, out message))
                {
                    e.Player.SendErrorMessage("[TServerWeb] Vote failed, please contact an admin.");
                    SendError("Configuration", message);
                    return;
                }
                WebClient wc = new WebClient();
                string webadress = ("http://www.tserverweb.com/vote.php?user=" + HttpUtility.UrlPathEncode(e.Player.Name) + "&sid=" + ID);
                Response response = Response.Read(wc.DownloadString(webadress));
                switch (response.response)
                {
                    case "success":
                        e.Player.SendSuccessMessage("[TServerWeb] Vote successful:\n" + response.message);
                        break;
                    case "failure":
                        e.Player.SendErrorMessage("[TServerWeb] Vote failed, please contact an admin.");
                        SendError("Vote", response.message);
                                break;
                }
            }
            catch (Exception ex)
            {
                e.Player.SendErrorMessage("[TServerWeb] Vote failed! This has been logged.");
                SendError("Vote", "Connection failure: " + ex.ToString());
            }
            return;           
        }
        public void ChangeID(CommandArgs e)
        {
            int ID;
            string message;
            if (e.Parameters.Count == 0)
            {
                
                if (!GetServerID(out ID, out message))
                {
                    e.Player.SendErrorMessage("[TServerWeb] Server ID is currently not specified! Please type /tserverweb [number] to set it.");
                    return;
                }
                else
                {
                    if(!GetServerID(out ID, out message))
                    {
                        e.Player.SendErrorMessage("Failed to get server ID from configuration file. Reason: " + message);
                        return;
                    }
                    e.Player.SendInfoMessage("[TServerWeb] Server ID is currently set to " + ID +
                        ". Type /tserverweb [number] to change it.");
                    return;
                }
            }
            if (e.Parameters.Count >= 2)
            {
                e.Player.SendErrorMessage("[TServerWeb] Incorrect syntax! Correct syntax: /tserverweb [number]");
                return;
            }
            int newID;
            if (int.TryParse(e.Parameters[0].ToString(), out newID))
            {
                string oldID = File.ReadAllLines(Path.Combine(TShock.SavePath, "ServerVote.txt"))[3];
                oldID = newID.ToString(); 
                e.Player.SendSuccessMessage("[TServerWeb] Server ID sucessfully changed to " + newID + ".");
                return;
            }
            else
            {
                e.Player.SendErrorMessage("[TServerWeb] Number not specified! Please type /tserverweb [number]");
                return;
            }
        }
        public void SendError(string typeoffailure, string message)
        {
            Log.Error("[TServerWeb] ServerVote Error:" + typeoffailure + "failure. Reason: " + message);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[TServerWeb] ServerVote Error: " + typeoffailure + " failure. Reason: " + message);
            Console.ResetColor();
        }
        public bool GetServerID(out int ID, out string message)
        {
            string stringid = File.ReadAllLines(Path.Combine(TShock.SavePath, "ServerVote.txt"))[3];
            if (int.TryParse(stringid, out ID))
            {
                if (ID == 0)
                {
                    message = "Server ID not specified. Type /tserverweb [ID] to specify it.";
                    return false;
                }
                else
                {
                    message = "";
                    return true;
                }
            }
            else
            {
                message = "Server ID is not a number. Please delete the ServerVote.txt file and restart the server.";
                return false;
            }
        }
    }
}
