﻿using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class RoleService
    {        
        private static ulong _readTheRulesId = 847048535799234560;
        private static ulong _vipId = 322490732885835776;
        private static ulong _valideId = 344912149728067584;

        private bool _connexion = true;
        private IRole _IRoleRules = null;                 
        private List<IRole> _IRolesSeparators = new List<IRole>();
        private List<IRole> _IRolesAttribution = new List<IRole>();
        List<SocketGuildUser> _allUsers = new List<SocketGuildUser>();
        private Dictionary<IRole, string> _roleToEmoteGames = new Dictionary<IRole, string>();
        private Dictionary<IRole, string> _roleToEmoteSpecial = new Dictionary<IRole, string>();
        public static readonly List<ulong> _roleAttributionIds = new List<ulong>
        {
            698852663764451381, //apps
            620700703580618762, //games
            613331423000133634, //anime
            536174000439558185, //music            
            613381032569339943, //stoner
            552134779210825739, //OnePiece
            773174545258774568, //visio          
            797968836707352606, //mac
            572073517462323201, //switch            
            689157917521346684, //mine
            843280439698259998, //bf            
            638175689270493205, //cod
            818518545720803341, //gta                        
        };
        public static readonly List<ulong> _roleSeparatorIds = new List<ulong>
        {
            1061919166199775232, //_separatorBonusId
            1061919390607605770, //_separatoIrlId
            1052542257737256980, //_separatorPrivilegesId
            1052521533135917137, //_separatorAccreditationsId                        
        };

        private DiscordSocketClient _client;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public RoleService(DiscordSocketClient client)
        {
            _client = client;
            _client.GuildMembersDownloaded += GuildMembersDownloaded;            
        }


        #region Update At Start
        private async Task GuildMembersDownloaded(SocketGuild arg)
        {
            log.Info($"| GuildMembersDownloaded IN --> firstIN={_connexion}");
            if (_connexion)
            {
                await Helper.SendLatencyAsync(_client);
                await CheckRoles();

                _connexion = false;
            }
            log.Info("| GuildMembersDownloaded out");
        }

        public async Task CheckRoles()
        {
            log.Info($"CheckRoles IN");
            if (_IRoleRules == null) _IRoleRules = Helper.GetRoleById(_client, _readTheRulesId);

            if (_allUsers.Count == 0)
            {
                _allUsers = Helper.GetZderLand(_client).Users.ToList();
                _allUsers.RemoveAll(x => x.IsBot);
            }

            await CheckRules();

            if (_IRolesAttribution.Count == 0 || _IRolesSeparators.Count == 0)
                FillRoles();            

            log.Info($"CheckRoles OUT");
        }

        private void FillRoles()
        {
            _IRolesSeparators = Helper.GetIRolesFromServer(_client, _roleSeparatorIds).ToList();

            //dico for attribution
            _IRolesAttribution = Helper.GetIRolesFromServer(_client, _roleAttributionIds).ToList();
            FillRolesDicos(_IRolesAttribution);            
        }

        internal void FillRolesDicos(List<IRole> rolesAttribution)
        {            
            foreach (var role in rolesAttribution)
            {
                var potentialName = Regex.Replace(role.Name, @"[^\u0000-\u007F]+", "");                                

                if (string.IsNullOrEmpty(potentialName)) // const + sans char
                {
                    switch (role.Name)
                    {
                        case "👽":
                            _roleToEmoteSpecial.Add(role, "👽");
                            break;
                        case "📹":
                            _roleToEmoteSpecial.Add(role, "📹");
                            break;
                        case "🎵":                            
                            _roleToEmoteSpecial.Add(role, "🎵");
                            break;
                    }
                }
                else
                {
                    switch (role.Name)
                    {
                        // CONTAINS
                        case string name when name.Contains("Anime"):
                        //case "Anime 💘":                        
                            _roleToEmoteSpecial.Add(role, "👺");
                            break;
                        //case "👒 One Piece":
                        case string name when name.Contains("One Piece"):
                            _roleToEmoteSpecial.Add(role, "👒");
                            break;

                        // ENDS                       
                        case string name when name.EndsWith("TV"):
                            //case "🎞️ Twitch TV":
                            _roleToEmoteSpecial.Add(role, "🌐");
                            break;
                        case string name when name.EndsWith("Games"):
                            //case "👾 Games":
                            _roleToEmoteSpecial.Add(role, "👾");
                            break;
                        //case "🤖 Apps":
                        case string name when name.EndsWith("Apps"):
                            _roleToEmoteSpecial.Add(role, "🤖");
                            break;
                        //case "💾 Minecraft":
                        case string name when name.EndsWith("Minecraft"):
                            _roleToEmoteGames.Add(role, "🧱");
                            break;
                        //case "💾 Battlefield": 
                        case string name when name.EndsWith("Battlefield"):
                            _roleToEmoteGames.Add(role, "💥");
                            break;
                        //case "💾 Call of Duty":
                        case string name when name.EndsWith("Call of Duty"):
                        case string name2 when name2.EndsWith("COD"):
                            _roleToEmoteGames.Add(role, "🔫");
                            break;
                        //case "💾 Grand Theft Auto":
                        case string name when name.EndsWith("Grand Theft Auto"):
                        case string name2 when name2.EndsWith("GTA"):
                            _roleToEmoteGames.Add(role, "💰");
                            break;
                        //case "🔌 Mac":
                        case string name when name.EndsWith("Mac"):
                            _roleToEmoteGames.Add(role, "🍎");
                            break;
                        //case "🔌 Switch":
                        case string name when name.EndsWith("Switch"):
                            _roleToEmoteGames.Add(role, "🎌");
                            break;
                    }
                }                                          
            }            
        }
       
        private async Task CheckRules()
        {
            log.Info($"CheckRules IN");

            var chrono = new Stopwatch();
            chrono.Start();

            var channelRules = Helper.GetSocketMessageChannel(_client, 846694705177165864); //rôles
            IReadOnlyCollection<IMessage> iMsg = channelRules.GetMessagesAsync(1).FirstAsync().Result;
            IMessage msg = iMsg.First();

            List<IReadOnlyCollection<IUser>> reactListUsers = msg.GetReactionUsersAsync(msg.Reactions.FirstOrDefault().Key, 1000).ToListAsync().Result;

            foreach (var userLst in reactListUsers)
            {
                var okUserslist = userLst.ToList();

                foreach (var okUser in okUserslist)
                {
                    if (okUser.Id != 493020872303443969) // compte qui met les reaction 
                    {
                        var subject = _allUsers.First(x => x.Id == okUser.Id);                        
                        
                        if (!subject.Roles.Contains(_IRoleRules))
                            await subject.AddRoleAsync(_IRoleRules);
                        
                        log.Info($"CheckRules done for {subject.Username}");
                    }
                }
            }

            chrono.Stop();
            log.Info($"CheckRules OUT in {chrono.ElapsedMilliseconds}ms");
        }

        public async Task UpdateListUser()
        {
            await _client.DownloadUsersAsync(_client.Guilds); // DL all user

            _allUsers = Helper.GetZderLand(_client).Users.ToList();
            _allUsers.RemoveAll(x => x.IsBot);
        }
        #endregion


        #region Update Live
        //REMOVED
        internal void RulesReactionRemoved(ulong userId)
        {
            var subject = _allUsers.First(x => x.Id == userId);
            subject.RemoveRoleAsync(_IRoleRules);
        }

        internal void SpecialReactionRemoved(SocketReaction reaction)
        {
            var subject = _allUsers.First(x => x.Id == reaction.User.Value.Id);
            var roleToAssign = _roleToEmoteSpecial.First(x => x.Value == reaction.Emote.Name).Key;
            subject.RemoveRoleAsync(roleToAssign);
        }

        internal void GamesReactionRemoved(SocketReaction reaction)
        {
            var subject = _allUsers.First(x => x.Id == reaction.User.Value.Id);
            var roleToAssign = _roleToEmoteGames.First(x => x.Value == reaction.Emote.Name).Key;
            subject.RemoveRoleAsync(roleToAssign);
        }

        //ADDED
        internal async Task RulesReactionAddedAsync(ulong userId)
        {
            var subject = _allUsers.First(x => x.Id == userId);
            await subject.AddRoleAsync(_IRoleRules);

            foreach(IRole separatorRole in _IRolesSeparators)
                await subject.AddRoleAsync(separatorRole);
        }

        internal void SpecialReactionAdded(SocketReaction reaction)
        {
            var subject = _allUsers.First(x => x.Id == reaction.User.Value.Id);
            var roleToAssign = _roleToEmoteSpecial.First(x => x.Value == reaction.Emote.Name).Key;
            subject.AddRoleAsync(roleToAssign);
        }

        internal void GamesReactionAdded(SocketReaction reaction)
        {
            var subject = _allUsers.First(x => x.Id == reaction.User.Value.Id);
            var roleToAssign = _roleToEmoteGames.First(x => x.Value == reaction.Emote.Name).Key;
            subject.AddRoleAsync(roleToAssign);
        }
        #endregion
    }
}
