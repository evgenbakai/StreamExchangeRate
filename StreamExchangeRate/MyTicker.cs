using System;

namespace StreamExchangeRate
{
    class MyTicker
    {
        public string Symbol { get; set; }
        public decimal AskPrice { get; set; }
        public decimal BidPrice { get; set; }
        public decimal TotalTradedVolume { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            MyTicker t = (MyTicker)obj;
            return (AskPrice == t.AskPrice) && (BidPrice == t.BidPrice) && (TotalTradedVolume == t.TotalTradedVolume);
        }
        public override int GetHashCode()
        {
            return this.AskPrice.GetHashCode();
        }
    }
}
