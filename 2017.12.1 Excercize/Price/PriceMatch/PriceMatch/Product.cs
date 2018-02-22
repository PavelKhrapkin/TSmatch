/* ------------------------------------------------------------------
 * Product - product description, usually- price-list line 9.02.2018
 * 
 * --- History: ---
 * Dec-2018 - re-make for ReadPriceList Excercize
 * 25.01.18 - GetL() - return product Length
 *  9.02.18 - refactoring Good renamed to Product
 * 
 * --- Methods: -----
 * Product(), (name,price..) - constructors
 * GetL()      - product length get from Fields and RuleText
 */
using System;
using System.Collections.Generic;

namespace PriceMatch
{
    [Serializable]
    public class Product
    {
        public string name;
        public double price;
        public List<object> Fields = new List<object>();
        protected Boot boot;
        protected ProductSet ps;
        protected string id;

        public Product() { }

        public Product(string _name, double _price)
        {
            name = _name;
            price = _price;
        }
        public Product(string name, object[] p)
        {
            this.name = name;
            foreach (var par in p) Fields.Add(par);
        }

        public Product(string _name, double _price, List<object> Fields, ProductSet ps) : this(_name, _price)
        {
            this.Fields = Fields;
            if (ps == null) throw new ArgumentNullException();
        }

        public void Init(Boot boot, ProductSet ps, string id = "")
        {
            this.boot = boot;
            this.ps = ps;
            if (string.IsNullOrEmpty(id))
                id = boot.Lib.IdGen(Declaration.IdTypeProd);
            this.id = id;
        }

        public Product Set(string name, double price, dynamic Fields)
        {
            this.name = name;
            this.price = price;
            this.Fields = Fields;
            return this;
        }
        public Product Set(Product product)
            => Set(product.name, product.price, product.Fields);

        public double GetL()
        {
            double result = 0;
            if (string.IsNullOrEmpty(name) || price <= 0) return -1;
            var ind = boot.Sect.GetFieldNumbers(ps.RuleText, Declaration.STypeLNG);
            if (ind.Count == 0)
            {
                string str = boot.Sect.Get(ps.RuleText, Declaration.STypeLNG);
                if (!double.TryParse(str, out result))
                    throw new Exception("GetL wrong length");
            }
            else result = (double)Fields[ind[0]];
            return result;
        }
    } // end class 
} // end namespace