using BoTools.Model;
using Discord;
using Discord.WebSocket;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class RoleService
    {        
        private static ulong _readTheRulesId = 847048535799234560;
        private static ulong _birthdayId = 1052530092082995201;
        private static ulong _gamingDealId = 1092072288226115685;

        private bool _connexion = true;        
        private IRole _IRoleBirthday = null;
        private IRole _IRoleRules = null;             
        Dictionary<string, DateTime> _birthDays = null;
        DateTime? _onGoingBirthday = null;
        DateTime? _lastDateTime = null;
        private List<IRole> _IRolesSeparators = new List<IRole>();        
        List<SocketGuildUser> _allUsers = new List<SocketGuildUser>();

        public static readonly List<ulong> _roleSeparatorIds = new List<ulong>
        {
            1061919166199775232, //_separatorBonusId
            1061919390607605770, //_separatoIrlId
            1052542257737256980 //_separatorDroitsId
            //1052521533135917137, _separatorAccreditationsId     //remove bc conflict admin rights modos
        };

        private DiscordSocketClient _client;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public RoleService(DiscordSocketClient client)
        {
            _client = client;

            if (_birthDays == null)
                _birthDays = Helper.GetBirthDays();

            if (_lastDateTime == null)            
                _lastDateTime = DateTime.Today;

            _client.GuildMembersDownloaded += GuildMembersDownloaded;            
            _client.LatencyUpdated += LatencyUpdated;
        }

        private async Task LatencyUpdated(int oldLatency, int newLatency)
        {
            //log.Info($"LatencyUpdated from {oldLatency} to {newLatency}ms");
            if (_lastDateTime != DateTime.Today)
            {
                _lastDateTime = DateTime.Today;
                await NotifRoles();
                await CheckBirthdate();
            }
        }

        private async Task GuildMembersDownloaded(SocketGuild arg)
        {
            log.Info($"| GuildMembersDownloaded IN --> firstIN={_connexion}");
            if (_connexion)
            {                
                await CheckRoles();
                await CheckBirthdate();
                await NotifRoles();                
                await CleanVocal();
                log.Debug($"Latency : {_client.Latency} ms");
                _connexion = false;
            }
            log.Info("| GuildMembersDownloaded out");
        }

        private async Task CheckBirthdate()
        {            
            if (_IRoleBirthday == null) _IRoleBirthday = Helper.GetRoleById(_client, _birthdayId);

            if (_onGoingBirthday == null) //pas anniv en cours                         
                await CheckBirthday();
            else
            {
                if (_onGoingBirthday != DateTime.Today) //anniv en cours != ajd ?
                    await CheckBirthday();
            }
        }

        public async Task CheckBirthday()
        {
            string msgStart = $"@here {Helper._pikachuEmote} \n" +
                        $"Vince me souffle dans l'oreille que c'est l'anniversaire de";

            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, Helper._idGeneralChannel);

            if (_birthDays != null && channel != null)
            {
                bool isSomeoneBD = _birthDays.ContainsValue(DateTime.Today);

                if (isSomeoneBD)
                {
                    string idTagTarget = _birthDays.First(x => x.Value == DateTime.Today).Key;

                    string message = msgStart + $" <@{idTagTarget}> aujourd'hui !\n" +
                        $"{Helper._coeurEmote} sur toi";

                    _onGoingBirthday = DateTime.Today;
                    var userTarget = Helper.GetZderLand(_client).Users.First(x => x.Id == Convert.ToUInt64(idTagTarget.Remove(0, 1)));
                    userTarget.AddRoleAsync(_IRoleBirthday);

                    var res = (IMessage)channel.SendMessageAsync(message).Result;

                    var bravo = Emote.Parse(Helper._bravoEmote);
                    Emoji cake = new Emoji("\uD83C\uDF82");
                    Emoji face = new Emoji("\uD83E\uDD73");
                    await res.AddReactionAsync(cake);
                    await res.AddReactionAsync(face);
                    await res.AddReactionAsync(bravo);
                }
            }
            else
                log.Error("no birthday list or channel");
        }

        private async Task CleanVocal()
        {
            var voiceChannels = _client.Guilds.First().VoiceChannels;

            var squadTmpVoice = voiceChannels.FirstOrDefault(x => x.Name.EndsWith("Squad bis"));
            if (squadTmpVoice != null)
            {
                var squadTmpVoiceId = squadTmpVoice.Id;
                await voiceChannels.First(x => x.Id == squadTmpVoiceId).DeleteAsync();
            }

            var reuTmpVoice = voiceChannels.FirstOrDefault(x => x.Name.EndsWith("⏱"));
            if (reuTmpVoice != null)
            {
                var reuTmpVoiceId = reuTmpVoice.Id;
                await voiceChannels.First(x => x.Id == reuTmpVoiceId).DeleteAsync();
            }
        }

        private async Task NotifRoles()
        {
            NotifGamingDeal();
        }

        private async void NotifGamingDeal()
        {
            #region Vendredi = Epic Store            
            if (Helper.IsFridayToday()){
                ISocketMessageChannel mediaChannel = Helper.GetSocketMessageChannel(_client, 494958624922271745);

                if (mediaChannel != null)
                {                    
                    List<string> urls = await GetEpicGamesStoreImg();
                    List<Embed> embeds = new List<Embed>();
                    int cpt = 0;

                    string message = $"<@&{_gamingDealId}> {Helper._verifiedEmote}\n" +
                        $"N'oublier pas de recup les jeux gratuits de la semaine sur le store EPIC GAMES :";                        

                    foreach (var url in urls)
                    {
                        Embed embed = new EmbedBuilder() { ImageUrl = urls[cpt] }.Build();
                        embeds.Add(embed);
                        cpt++;
                    }

                    await mediaChannel.SendMessageAsync(text: message, embeds: embeds.ToArray(), isTTS: true);                    
                }
            }
            #endregion
        }

        private async Task<List<string>> GetEpicGamesStoreImg()
        {
            List<string> res = new List<string>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://store-site-backend-static.ak.epicgames.com/");

                HttpResponseMessage response = await client.GetAsync("freeGamesPromotions?locale=fr-FR&country=FR&allowCountries=FR");

                if (response.IsSuccessStatusCode)
                {                    
                    string json = await response.Content.ReadAsStringAsync();

                    //LINQ to JSON
                    JObject jObj = JObject.Parse(json);
                    var scopedJson = jObj["data"]["Catalog"]["searchStore"]["elements"].Select(x => x.ToString()).ToList();                    

                    foreach (var extract in scopedJson)
                    {
                        EpicFreeGames game = JsonConvert.DeserializeObject<EpicFreeGames>(extract);

                        if (game.promotions?.PromotionalOffers?.Count >= 1)
                        {
                            if (game.promotions.PromotionalOffers[0].PromotionalOffers[0].DiscountSetting.DiscountPercentage == 0)                            
                                res.Add(game.keyImages[0].url);                            
                        } 
                    }                    
                }
                else                
                    log.Error("Failed to retrieve free games from Epic Games API.");                
            }
            return res;
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

            if (_IRolesSeparators.Count == 0)
                FillRoles();            

            log.Info($"CheckRoles OUT");
        }

        private void FillRoles()
        {
            _IRolesSeparators = Helper.GetIRolesFromServer(_client, _roleSeparatorIds).ToList();           
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


        #region Update Live
        //REMOVED
        internal void RulesReactionRemoved(ulong userId)
        {
            var subject = _allUsers.First(x => x.Id == userId);
            subject.RemoveRoleAsync(_IRoleRules);
        }

        //ADDED
        internal async Task RulesReactionAddedAsync(ulong userId)
        {
            var subject = _allUsers.First(x => x.Id == userId);
            await subject.AddRoleAsync(_IRoleRules);

            foreach(IRole separatorRole in _IRolesSeparators)
                await subject.AddRoleAsync(separatorRole);
        }
        #endregion
    }
}
