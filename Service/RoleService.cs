using BoTools.Model;
using Discord;
using Discord.WebSocket;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        DateTime? _lastDateTime = null;             
        List<SocketGuildUser> _allUsers = new List<SocketGuildUser>();

        private DiscordSocketClient _client;
        private EventService _eventService;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public RoleService(DiscordSocketClient client, EventService eventService)
        {
            _client = client;
            _eventService = eventService;

            if (_birthDays == null)
                _birthDays = Helper.GetBirthDays();

            if (_lastDateTime == null)            
                _lastDateTime = DateTime.Today;

            _client.GuildMembersDownloaded += GuildMembersDownloaded;            
            _client.LatencyUpdated += LatencyUpdated;
        }

        private async Task LatencyUpdated(int oldLatency, int newLatency)
        {            
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
                await SetupRoles();
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
                       
            await CheckBirthday();
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

            if(Helper.IsThursdayToday())
                await _eventService.CreateNextOnePiece(Helper._notifOnePiece);
        }

        private async void NotifGamingDeal()
        {
            #region Vendredi = Epic Store            
            if (Helper.IsFridayToday())
            {
                ISocketMessageChannel mediaChannel = Helper.GetSocketMessageChannel(_client, 1200228737006960650);

                if (mediaChannel != null)
                {                    
                    List<string> urls = await GetEpicGamesStoreImg();
                    List<Embed> embeds = new List<Embed>();
                    int cpt = 0;

                    string message = $"<@&{_gamingDealId}> {Helper._pikachuEmote}\n" +
                        $"N'oublier pas de recup le.s jeu.x gratuit.s de la semaine sur le store **EPIC GAMES** :";                        

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

        public async Task SetupRoles()
        {
            log.Info($"CheckRoles IN");
            if (_IRoleRules == null) _IRoleRules = Helper.GetRoleById(_client, _readTheRulesId);

            if (_allUsers.Count == 0)
            {
                _allUsers = Helper.GetZderLand(_client).Users.ToList();
                _allUsers.RemoveAll(x => x.IsBot);
            }          

            log.Info($"CheckRoles OUT");
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

            var leader = _client.GetUser(Helper._vinceId);
            await leader.SendMessageAsync($"<@{userId}> read the ZderLand's Rules !");
        }
        #endregion
    }
}
