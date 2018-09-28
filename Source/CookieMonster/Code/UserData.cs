using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookieMonster.UserData
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ApplicationData
    {
        public List<CookieChannel> activeChannels = new List<CookieChannel>();
        public List<CookieUser> users = new List<CookieUser>();
    }

    /// <summary>
    /// We create a seperate class for cookies so that they have open access.
    /// </summary>
    [Serializable]
    public class CookieChannel
    {
        public List<SocketMessage> botMessages = new List<SocketMessage>();
        public List<SocketMessage> userResponses = new List<SocketMessage>();

        public ISocketMessageChannel channel;

        public bool cookieDropped;

        // Adjust cookie spawn chance for this channel.
        public int cookieSpawnChance = 3;
        public string commandPrefix;

        /// <summary>
        /// 
        /// </summary>
        public async void CookieDropCheck()
        {
            Random rnd = new Random();

            int cookieChance = rnd.Next(1, 100);

            if (cookieChance <= cookieSpawnChance)
            {
                if (cookieDropped == false)
                {
                    cookieDropped = true;

                    try
                    {
                        await channel.SendMessageAsync("Cookie Monster dropped a :cookie:!");
                    }
                    catch
                    {
                        Console.WriteLine("COULD NOT SEND MESSAGE");
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookieWinner"></param>
        public async void CookiePickedUp(CookieUser cookieWinner)
        {
            cookieDropped = false;

            try
            {
                await channel.SendMessageAsync("@" + cookieWinner.userName + " just got a cookie!");
            }
            catch
            {
                Console.WriteLine("COULD NOT SEND MESSAGE");
            }

            cookieWinner.cookies++;


            await Task.Delay(5000);

            foreach (SocketMessage msg in userResponses)
            {
                if (msg.Content.ToLower() == "pick" ||msg.Content.ToLower() == commandPrefix + "stats")
                {
                    try
                    {
                        await msg.DeleteAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                else
                    return;
            }

            foreach (SocketMessage msg in botMessages)
            {
                // We check to see if this is a cookie message. WE don't want to delete stat related ones.
                if (msg.Content.Contains(" just got a cookie!"))
                {
                    try
                    {
                        await msg.DeleteAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

    [Serializable]
    public class CookieUser
    {
        public string userName;
        public int cookies;
        public long spotifyTime;

        public List<GameTime> allGames = new List<GameTime>();
    }

    [Serializable]
    public class GameTime
    {
        public string applicationName = "No Name";
        public long timeSpentInSeconds = 0;
    }

    public static class Utility
    {
        public static string SecondsToString(long timeSpentInSeconds)
        {
            long hours = timeSpentInSeconds / 3600;
            long mins = (timeSpentInSeconds % 3600) / 60;

            long seconds = timeSpentInSeconds % 60;

            return string.Format("{0:D2}h {1:D2}m {2:D2}s", hours, mins, seconds);
        }
    }
}
