using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XMLserialization
{
    class Program
    {
        static void Main(string[] args)
        {
            List<PersonalDetails> staff = new List<PersonalDetails>();
            Address mow = new Address();
            mow.City = "Moscow";
            mow.StreetName = "Сокольники";
            mow.HouseNo = 25;
            Address perm = new Address();
            perm.City = "Пермь";
            perm.StreetName = "Krivaya";
            perm.HouseNo = 15;
            PersonalDetails Ivan = new PersonalDetails();
            Ivan.Name = "Иван";
            Ivan.Age = 24;
            Ivan.address = mow;
            var Petr = new PersonalDetails();
            Petr.Name = "Петр";
            Petr.Age = 40;
            Petr.address = perm;
            staff.Add(Ivan);
            staff.Add(Petr);
            Serialize(staff);
        }

        private static void Serialize(List<PersonalDetails> list)
        {
            XmlSerializer serialyzer = new XmlSerializer(typeof(List<PersonalDetails>));
            using (TextWriter writer = new StreamWriter(@"C:\Users\Pavel_Khrapkin\Desktop\$DebugDir\Excercize\Xml.xml"))
            {
                serialyzer.Serialize(writer, list);
            }
        }
    }

    public class PersonalDetails
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Address address;
    }
    public class Address
    {
        public int HouseNo { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
    }
}
