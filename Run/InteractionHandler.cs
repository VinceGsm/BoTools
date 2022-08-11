using BoTools.Service;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Run
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interaction;
        private readonly IServiceProvider _services;
        private readonly MessageService _messageService;

        public InteractionHandler(DiscordSocketClient client, InteractionService interaction, IServiceProvider services)
        {
            _interaction = interaction;
            _client = client;
            _services = services;

            _messageService ??= (MessageService)_services.GetService(typeof(MessageService));
        }

        public async Task InitializeInteractionAsync()
        {
            // Here we discover all of the command modules in the entry assembly and load them.
            // Starting from Discord.NET 2.0, a service provider is required to be passed into
            // the module registration method to inject the required dependencies
            await _interaction.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);

            _client.Ready += Ready;
            _client.SlashCommandExecuted += SlashCommandExecuted;            
        }

        private Task SlashCommandExecuted(SocketSlashCommand arg)
        {
            switch (arg.Data.Name)
            {
                case "main-roles":
                    HandleMainRolesCommand(arg);
                    break;
                case "rate_one-piece":
                    HandleRateOpGlobalCommand(arg);
                    break;
            }
            return Task.CompletedTask;
        }

        private void HandleRateOpGlobalCommand(SocketSlashCommand arg)
        {
            var user = arg.User;
            var nameFeedback = arg.Data.Options.First().Name;
            string urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1007316947135889418/unknown.png"; //middle by default

            switch (Int32.Parse(nameFeedback.First().ToString()))
            {
                case 1:
                case 2:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1007316947513380914/unknown.png";
                    break;                
                case 5:
                case 6:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1007316948578730015/unknown.png";
                    break;
            }

            var embedBuilder = new EmbedBuilder()
                .WithAuthor(user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithTitle($"Feedback : {nameFeedback.Remove(0,3)}")
                .WithDescription($"{arg.Data.Options.First().Value}")
                .WithColor(Color.Red)
                .WithImageUrl(urlIcon);

            arg.RespondAsync(embed: embedBuilder.Build());
        }

        private Task HandleMainRolesCommand(SocketSlashCommand arg)
        {
            List<string> roles = new List<string>();
            roles.Add("<@&689144324939710527>");    
            roles.Add("<@&322489502562123778>");    
            roles.Add("<@&322490732885835776>");
            roles.Add("<@&344912149728067584>");
            roles.Add("<@&847048535799234560>");

            // We need to extract the user parameter from the command.
            // since we only have one option and it's required, we can just use the first option.
            var user = arg.User;

            // We remove the everyone role and select the mention of each role.
            var roleList = string.Join("\n", roles);            

            var embedBuiler = new EmbedBuilder()
                .WithAuthor(user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithTitle("Rôles principaux de Zderland :")
                .WithDescription(roleList)
                .WithColor(Color.Green)
                .WithImageUrl(Helper.GetZderLandIconUrl());
            
            return arg.RespondAsync(embed: embedBuiler.Build(), ephemeral:true);
        }

        private async Task Ready()
        {
            await _messageService.Ready(); // Legacy Ready() 

            var guild = Helper.GetZderLand(_client);

            var onePieceSlashCommand = new SlashCommandBuilder()
            .WithName("feedback_one-piece") // Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$            
            .WithDescription("Comment as-tu trouver le dernier épisode de One Piece ?") // Descriptions can have a max length of 100.
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("score")
                .WithDescription("La note globale que te dicte ton cœur")
                .WithRequired(true)
                .AddChoice("1. Mauvais !", $"💩💩💩")
                .AddChoice("2. Ennuyant", $"💤💤💤")
                .AddChoice("3. Passable", $"👁👁👁")
                .AddChoice("4. Régale", $"🔥🔥🔥")
                .AddChoice("5. Epoustouflant", $"⭐️⭐️⭐️")
                .AddChoice("6. Légendaire !", $"❤️❤️❤️")
                .WithType(ApplicationCommandOptionType.String) 
            );

            var roleSlashGlobalCommand = new SlashCommandBuilder()
            .WithName("main-roles")
            .WithDescription("Affiche la liste des rôles principaux du server *en message secret*");
            try
            {
                await guild.CreateApplicationCommandAsync(onePieceSlashCommand.Build());
                await _client.CreateGlobalApplicationCommandAsync(roleSlashGlobalCommand.Build()); // up to 1h (use guild test if need test quick)
            }
            catch (HttpException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException.
                // This exception contains the path of the error as well as the error message.
                // You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
            }            
        }
    }
}
