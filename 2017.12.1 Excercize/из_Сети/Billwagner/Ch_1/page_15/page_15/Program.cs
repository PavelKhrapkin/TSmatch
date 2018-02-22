using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace page_15
{
    class Program
    {
        static void Main(string[] args)
        {
            var f = GetMagicNumber();
            var total = 100 * f / 6;
            Console.WriteLine($"Declared Type: {total.GetType().Name}, Value: {total}");
        }

        private static Decimal GetMagicNumber()
        {
            return 55;
        }
    }
}
