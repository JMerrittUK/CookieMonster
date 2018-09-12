# CookieMonster
A Discord bot designed to keep track of the important things in life - cookies!

This was a bot created for use in the Staffordshire University unofficial Game Dev Discord server.

## SET UP INSTRUCTIONS
The bot WILL NOT run without a config.xml file. This holds all of the customizable attributes for the bot, and is stored in the root of this project. Ensure you put a config.xml into the root of the execution folder.

There are three ways to run this bot...

## A | INVITE THE COOKIE MONSTER

Just want to use the bot? Use this link: https://discordapp.com/oauth2/authorize?client_id=366934149791219723&scope=bot&permissions=142400 to get started!

## B | USER FRIENDLY VERSION

Can't or don't want to manually compile the project? Check out the Releases folder, where you can get the latest version. The only thing that needs customization in here is the config.xml, which can be edited with a text editor such as Notepad or Notepad++. 

Please note this is EXACTLY the same as having the bot join your server, which you can do at https://discordapp.com/oauth2/authorize?client_id=366934149791219723&scope=bot&permissions=142400

## C | COMPILATION OF THE PROJECT

You will also need to download the Discord.NET package, a handy C# wrapper available here: https://github.com/RogueException/Discord.Net (you can also use NuGet)

Once this is done, ensure after your first build you then bring in the config.xml into the correct folder. The users data XML check WILL throw an exception, as it is designed to be used with data in place - however it will work fine in this circumstance, and will work as normal. At some point I'll add an additional check to prevent this exception.
