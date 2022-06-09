using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scrap
{
    public class Currency
    {
        public Currency(string _name, int _value, string _code, float _price)
        {
            name = _name;
            value = _value;
            code = _code;
            price = _price;
        }
        public string name { get;}
        public int value { get;}
        public string code { get;}
        public float price { get;}

    }
}
