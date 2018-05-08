using System;
using System.Collections.Generic;

namespace StreamExchangeRate_v._2
{
    public abstract class AProvider
    {
        protected string Key
        {
            get;
            set;
        }

        public string[] Symbol
        {
            get;
            protected set;
        }

        public string NameProvider
        {
            get;
            protected set;
        }

        abstract public Uri getUri();

        abstract public void OnMessage(string data);

        protected List<BaseTicker> listTicker = new List<BaseTicker>();

        protected bool EqualsTicker(BaseTicker ticker)
        {
            // return true -  в списку listTicker знайдено обєкт який рівний обєкту  ticker
            // return false - в списку listTicker не знайдено обєкта по необхідному символу (ticker.Symbol), або
            //                відповідний обєкт з listTicker не рівнмй обєкту ticker
            bool returnValue;
            int indexFoundTicker = listTicker.FindIndex((t) => t.Symbol == ticker.Symbol);
            if (indexFoundTicker != -1)
            {
                returnValue = listTicker[indexFoundTicker].Equals(ticker);
                listTicker[indexFoundTicker] = ticker;
                return returnValue;
            }
            else
            {
                listTicker.Add(ticker);
                return false;
            }
        }
    }
}
