using System.Collections.Generic;

namespace BoTools.Model
{
    public class EpicFreeGames
    {
        public Promotions? promotions { get; set; }
        public List<KeyImages> keyImages { get; set; }
    }

    public class KeyImages
    {
        public string type { get; set; }
        public string url { get; set; }
    }

    public class Promotions
    {        
        public List<PromotionalOffer?> PromotionalOffers { get; set; }
        public List<PromotionalOffer> UpcomingPromotionalOffers { get; set; }
    }

    public class PromotionalOffer
    {
        public List<PromotionalOfferDetails> PromotionalOffers { get; set; }
    }

    public class PromotionalOfferDetails
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public DiscountSetting DiscountSetting { get; set; }
    }

    public class DiscountSetting
    {
        public string DiscountType { get; set; }
        public int DiscountPercentage { get; set; }
    }

}
