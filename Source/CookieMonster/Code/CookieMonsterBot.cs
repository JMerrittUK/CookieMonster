using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Timers;

using Discord;
using Discord.WebSocket;

namespace CookieMonster
{
    /// <summary>
    /// Welcome to the Cookie Monster bot!
    /// 
    /// This is an example of a Discord bot build on top of the .NET wrapper by RogueException (provided under MIT licence)
    /// 
    /// Want to invite the original bot, fully unmodified?
    /// https://discordapp.com/oauth2/authorize?client_id=366934149791219723&scope=bot&permissions=142400
    /// </summary>
    public class CookieMonsterBot
    {
        // VARIABLES

        #region Static
        public Structs.Config config;
        public Color accentColor;
        #endregion

        #region Public
        /// <summary> Holds all of the data which is saved to disk </summary>
        public UserData.ApplicationData cookieMonsterData = new UserData.ApplicationData();
        #endregion

        #region Private
        /// <summary> The bots client connection to Discord </summary>
        private DiscordSocketClient _client;
        /// <summary> </summary>
        private string dataLocation;

        /// <summary>  </summary>
        private static Timer SaveTimer;
        private static Timer UpdateTimer;
        #endregion


        // METHODS

        #region Public
        public static void Main(string[] args)
            => new CookieMonsterBot().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            dataLocation = Directory.GetCurrentDirectory();

            LoadConfig();

            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            await _client.LoginAsync(TokenType.Bot, config.token);
            await _client.StartAsync();

            LoadData();
            StartTimers();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        #endregion

        #region Private
        private void LoadConfig()
        {            
            XmlSerializer formatter = new XmlSerializer(typeof(Structs.Config));

            try
            {
                using (FileStream aFile = new FileStream(dataLocation + "/config.xml", FileMode.Open))
                {
                    byte[] buffer = new byte[aFile.Length];
                    aFile.Read(buffer, 0, (int)aFile.Length);

                    using (MemoryStream stream = new MemoryStream(buffer))
                    {
                        config = (Structs.Config)formatter.Deserialize(stream);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            accentColor = new Color(config.color.r, config.color.g, config.color.b);
        }

        private void StartTimers()
        {
            UpdateTimer = new Timer(1000);
            UpdateTimer.Elapsed += Update;
            UpdateTimer.AutoReset = true;
            UpdateTimer.Enabled = true;

            // Create a timer
            SaveTimer = new Timer(5000);
            SaveTimer.Elapsed += SaveData;
            SaveTimer.AutoReset = true;
            SaveTimer.Enabled = true;
        }

        private void Update(Object source, ElapsedEventArgs e)
        {
            foreach(SocketGuild guild in _client.Guilds)
            {
                foreach(SocketUser user in guild.Users)
                {
                    UserData.CookieUser cookieUser = FindCookieUser(user.Username + "#" + user.Discriminator);
                    
                    if (user.Game.HasValue)
                    {
                        if (user.Game.Value.Name == "Spotify")
                        {
                            cookieUser.spotifyTime++;
                            return;
                        }

                        UserData.GameTime targetGameTime = null;

                        try
                        {
                            targetGameTime = cookieUser.allGames.FirstOrDefault(game => game.applicationName == user.Game.Value.Name);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }

                        // If we found the game, we can make the change we want to, if not we create a 
                        if (targetGameTime != null)
                            targetGameTime.timeSpentInSeconds++;
                        else
                            cookieUser.allGames.Add(new UserData.GameTime { applicationName = user.Game.Value.Name, timeSpentInSeconds = 1 });
                    }                    
                }
            }
        }

        private void SaveData(Object source, ElapsedEventArgs e)
        {
            // We create the file stream
            TextWriter output = new StreamWriter(dataLocation + "/UserData.xml");

            // Next we serialize our data. To make this simpler, we contain all data we plan to save in a single serialized list.
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<UserData.CookieUser>));

            // We also store a local version of our data to prevent games overwriting each others attributes during serialization.
            List<UserData.CookieUser> localCopy = cookieMonsterData.users;

            xmlSerializer.Serialize(output, localCopy);

            output.Close();

            Console.WriteLine("Total servers: " + _client.Guilds.Count + " | Total users: " + cookieMonsterData.users.Count);
        }

        #region Message Handling

        private UserData.CookieUser FindCookieUser(string userReference)
        {
            if (cookieMonsterData.users.Count > 0)
            {
                foreach (UserData.CookieUser userInList in cookieMonsterData.users)
                {
                    if (userInList.userName == userReference)
                        return userInList;
                }
            }

            UserData.CookieUser newUser = new UserData.CookieUser
            {
                userName = userReference,
                cookies = 0
            };

            cookieMonsterData.users.Add(newUser);

            return newUser;
        }

        private UserData.CookieChannel FindCookieChannel(ISocketMessageChannel channelReference)
        {
            if (cookieMonsterData.activeChannels.Count > 0)
            {
                foreach (UserData.CookieChannel channelInList in cookieMonsterData.activeChannels)
                {
                    if (channelInList.channel == channelReference)
                        return channelInList;
                }
            }

            UserData.CookieChannel newChannel = new UserData.CookieChannel
            {
                channel = channelReference,
                cookieSpawnChance = config.cookieDropChance,
                commandPrefix = config.commandPrefix
            };

            cookieMonsterData.activeChannels.Add(newChannel);

            return newChannel;
        }

        /// <summary>
        /// If we recieve a message in any chat, this task is called.
        /// </summary>
        /// <param name="message"> Message sent, along with other critical information </param>
        /// <returns> null </returns>
        private async Task MessageReceived(SocketMessage message)
        {
            UserData.CookieUser activeUser = FindCookieUser(message.Author.Username + "#" + message.Author.Discriminator);
            UserData.CookieChannel activeChannel = FindCookieChannel(message.Channel);

            #region Cookie Logic
            // First we make sure we ignore bots, as we don't want them triggering the code. No cookies for the cookie monster!
            if (!message.Author.IsBot)
            {
                string lowercaseMessage = message.Content.ToLower();

                // We check if we should attempt a cookie drop
                if (!activeChannel.cookieDropped == true && lowercaseMessage != "pick")
                {
                    activeChannel.CookieDropCheck();
                }

                // Here we check for commands!
                switch (lowercaseMessage)
                {
                    case "pick":
                        if (activeChannel.cookieDropped)
                        {
                            // We add the message to our 'to delete' messages for this channel.
                            activeChannel.userResponses.Add(message);

                            activeChannel.CookiePickedUp(activeUser);
                        }

                        break;


                    case "-stats":
                        EmbedBuilder stats = new EmbedBuilder();

                        stats.WithTitle(activeUser.userName);

                        stats.WithThumbnailUrl(message.Author.GetAvatarUrl());

                        stats.WithDescription(":cookie: Cookies: " + activeUser.cookies + " \n:musical_note: Spotify Time: " + UserData.Utility.SecondsToString(activeUser.spotifyTime));
                        stats.WithColor(accentColor);

                        // We order our games first, so they display in the correct order.
                        List<UserData.GameTime> games = activeUser.allGames;

                        games = games.OrderByDescending(g => g.timeSpentInSeconds).ToList();

                        // We check to see if we need to display all of the games in this persons storage.
                        if (games.Count > 0)
                        {
                            int gameCount = games.Count;

                            if (gameCount > config.totalGamesToDisplay)
                                gameCount = config.totalGamesToDisplay;

                            if (config.displayAllGames)
                            {
                                foreach (UserData.GameTime game in games)
                                {
                                    stats.AddInlineField(game.applicationName, UserData.Utility.SecondsToString(game.timeSpentInSeconds));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < gameCount; i++)
                                {
                                    UserData.GameTime game = games[i];
                                    stats.AddInlineField(game.applicationName, UserData.Utility.SecondsToString(game.timeSpentInSeconds));
                                }
                            }
                        }

                        stats.WithFooter("Cookie Monster", config.botPictureURL);
                        stats.WithCurrentTimestamp();

                        await message.Channel.SendMessageAsync("", false, stats);
                        break;
                }
            }
            else
            {
                // We search through each of our active cookie channels...
                foreach (UserData.CookieChannel channelToCheck in cookieMonsterData.activeChannels)
                {
                    // First we check to see if this channel has been created as a cookie channel.
                    if (channelToCheck.channel == message.Channel)
                    {
                        // If it has, then we need to check if the bot is the correct one.
                        if (message.Author.Id == config.botID)
                        {
                            channelToCheck.botMessages.Add(message);
                        }
                    }
                }
            }
            #endregion
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        #endregion


        #region Data Handling

        private void LoadData()
        {
            // NEW METHOD

            XmlSerializer formatter = new XmlSerializer(typeof(List<UserData.CookieUser>));

            try
            {
                using (FileStream aFile = new FileStream(dataLocation + "/UserData.xml", FileMode.Open))
                {
                    byte[] buffer = new byte[aFile.Length];
                    aFile.Read(buffer, 0, (int)aFile.Length);

                    using (MemoryStream stream = new MemoryStream(buffer))
                    {
                        cookieMonsterData.users = (List<UserData.CookieUser>)formatter.Deserialize(stream);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #endregion
    }
}
