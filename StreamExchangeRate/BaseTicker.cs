using System;
namespace StreamExchangeRate
{
    public class BaseTicker
    {
        public string Symbol { get; set; }
        public decimal AskPrice { get; set; }
        public decimal BidPrice { get; set; }
        public decimal TotalTradedVolume { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            BaseTicker ticker = (BaseTicker)obj;
            return (AskPrice == ticker.AskPrice) && (BidPrice == ticker.BidPrice) && (TotalTradedVolume == ticker.TotalTradedVolume);
        }
        public override int GetHashCode()
        {
            return this.AskPrice.GetHashCode();
        }
    }
}
