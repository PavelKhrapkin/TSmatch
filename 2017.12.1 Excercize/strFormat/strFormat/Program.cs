using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace strFormat
{
    class Program
    {
        static void Main(string[] args)
        {
            txt("try {0}, {1}, {2}, {3}, {4}", 0, 1, 2, 3, 4);
        }
        public static void txt(string msgcode, params object[] p)
        {
            string str = string.Format(msgcode, p);
            Console.WriteLine(str);
        }
    }
}
