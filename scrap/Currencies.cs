using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System.Net;

namespace scrap
{
    static public class Currencies
    {
        private const string BaseUrl = "https://www.nbp.pl/home.aspx?f=/kursy/kursya.html";
        private static int num { get; } = 34;
        private static Currency[] course { get; set; }
        public static Currency[] courseFromDB { get; set; }
        private static bool pageAcces = true;
        public static bool PageAcces
        {
            get { return pageAcces; }
        }
        static public void getCurrencies()
        {
            var web = new HtmlWeb();
            try
            {
                var doc = web.Load(BaseUrl);
                var currencies = doc.QuerySelectorAll(".nbptable tbody tr");

                course = new Currency[num];

                int i = 0;
                foreach (var country in currencies)
                {
                    var valuesCur = country.QuerySelectorAll("td");

                    string codeValue = valuesCur[1].InnerText;
                    int length = codeValue.Length;

                    string code = codeValue.Remove(0, length - 3);

                    int value = int.Parse(codeValue.Substring(0, length - 3));

                    string name = valuesCur[0].InnerText;
                    float price = float.Parse(valuesCur[2].InnerText);

                    course[i] = new Currency(name, value, code, price);

                    i++;
                }
            }
            catch (WebException e)
            {
                pageAcces = false;
                Console.WriteLine("Nie można połączyć ze stroną, możliwość odczytu danych z bazy");
            }
           

        }
        static public void show()
        {
            Console.Clear();

            int maxName = 0,
                maxValue = 0,
                maxPrice = 0;

            foreach (var currency in courseFromDB)
            {

                if (currency.name.Length > maxName)
                {
                    maxName = currency.name.Length;
                }

                if (currency.value.ToString().Length > maxValue)
                {
                    maxValue = currency.value.ToString().Length;
                }

                if (currency.price.ToString().Length > maxPrice)
                {
                    maxPrice = currency.price.ToString().Length;
                }
            }

            maxValue += 3; // Bo jeszcze 3 znaki

            for (int a = 0; a <= courseFromDB.Length + 1; a++)
            {


                for (int j = 1; j <= maxName + 3; j++)
                {
                    Console.SetCursorPosition(j, 2 * a);
                    Console.Write("-");
                }

                for (int j = maxName + 5; j <= maxName + 5 + maxValue + 2; j++)
                {
                    Console.SetCursorPosition(j, 2 * a);
                    Console.Write("-");
                }

                for (int j = maxName + 5 + maxValue + 4; j <= maxName + 5 + maxValue + 4 + maxPrice + 2; j++)
                {
                    Console.SetCursorPosition(j, 2 * a);
                    Console.Write("-");
                }

                if (a == courseFromDB.Length + 1) break;

                Console.SetCursorPosition(0, 2 * a + 1);
                Console.Write("|");
                Console.SetCursorPosition(0, 2 * a + 2);
                Console.Write("|");

                Console.SetCursorPosition(maxName + 4, 2 * a + 1);
                Console.Write("|");
                Console.SetCursorPosition(maxName + 4, 2 * a + 2);
                Console.Write("|");

                Console.SetCursorPosition(maxName + 5 + maxValue + 3, 2 * a + 1);
                Console.Write("|");
                Console.SetCursorPosition(maxName + 5 + maxValue + 3, 2 * a + 2);
                Console.Write("|");

                Console.SetCursorPosition(maxName + 5 + maxValue + 4 + maxPrice + 3, 2 * a + 1);
                Console.Write("|");
                Console.SetCursorPosition(maxName + 5 + maxValue + 4 + maxPrice + 3, 2 * a + 2);
                Console.Write("|");


            }
            Console.SetCursorPosition(maxName / 2 + 1, 1);
            Console.Write("Name");
            Console.SetCursorPosition(maxName + 4 + maxValue / 2, 1);
            Console.Write("Code");
            Console.SetCursorPosition(maxName + 5 + maxValue + 3 + maxPrice / 2, 1);
            Console.Write("Price");

            int i = 0;

            foreach (var currency in courseFromDB)
            {
                Console.SetCursorPosition(2, 2 * i + 3);
                Console.Write(currency.name);

                Console.SetCursorPosition(maxName + 6, 2 * i + 3);
                Console.Write($"{currency.value}{currency.code}");

                Console.SetCursorPosition(maxName + 6 + maxValue + 4, 2 * i + 3);
                Console.Write(string.Format("{0:N4}", currency.price));

                i++;
            }
            Console.ReadKey(true);
        }

        static public void sendToDB(MySqlConnection dataBase)
        {
            string sql = "INSERT INTO waluty (nazwa, wartosc, kod, kurs, data_aktualizacji) VALUES";

            string day = DateTime.Today.ToString().Substring(0, 2),
                   month = DateTime.Today.ToString().Substring(3, 2),
                   year = DateTime.Today.ToString().Substring(6, 4),
                   dateToSql = $"{year}-{month}-{day}";

            int intiger,
                rest;

            for (int i = 0; i < course.Length; i++)
            {
                intiger = (int)course[i].price;
                rest = (int)((course[i].price - intiger)*10000);

                sql = String.Concat(sql, $"(\"{course[i].name}\", {course[i].value}, \"{course[i].code}\", {intiger}.{rest}, '{dateToSql}')");

                if(i == course.Length - 1)
                {
                    sql = String.Concat(sql, ";");
                    break;
                }
                sql = String.Concat(sql, ",");

            }
            MySqlCommand command = new MySqlCommand(sql, dataBase);
            Console.WriteLine(sql);
            var reader = command.ExecuteReader();
            reader.Close();
        }
        public static void updateDB(MySqlConnection dataBase)
        {
            string sql = "";

            string day = DateTime.Today.ToString().Substring(0, 2),
                   month = DateTime.Today.ToString().Substring(3, 2),
                   year = DateTime.Today.ToString().Substring(6, 4),
                   dateToSql = $"{year}-{month}-{day}";

            int intiger,
                rest;

            for (int i = 0; i < course.Length; i++)
            {
                intiger = (int)course[i].price;
                rest = (int)((course[i].price - intiger) * 10000);

                sql = String.Concat(sql, $"UPDATE waluty SET kurs = {intiger}.{rest}, data_aktualizacji = '{dateToSql}' WHERE id = {i + 1}; ");
            }

            MySqlCommand cmd = new MySqlCommand(sql, dataBase);
            var reader = cmd.ExecuteReader();
            reader.Close();
        }
        public static void getFromDB(MySqlConnection dataBase)
        {
            courseFromDB = new Currency[num];

            string sql = "SELECT * FROM waluty";

            MySqlCommand cmd = new MySqlCommand(sql, dataBase);
            using (var reader = cmd.ExecuteReader())
            {
                string name;
                int value;
                string code;
                float price;

                int i = 0;
                while (reader.Read())
                {
                    name = Convert.ToString(reader["nazwa"]);
                    value = int.Parse(Convert.ToString(reader["wartosc"]));
                    code = Convert.ToString(reader["kod"]);
                    price = float.Parse(Convert.ToString(reader["kurs"]));

                    courseFromDB[i] = new Currency(name, value, code, price);

                    i++;
                }
            }            
        }

        public static void singleCourse()
        {
            
            Console.WriteLine("Podaj kod waluty");
            while (true)
            {
                string kod = Console.ReadLine().ToUpper();

                Currency chosen = courseFromDB.FirstOrDefault(c => c.code == kod);

                if (chosen != null)
                {
                    Console.WriteLine($"{chosen.name} code: {chosen.value}{chosen.code}, exchange ratio: {chosen.price}");
                    break;
                }
                else
                {
                    Console.WriteLine("Podaj poprawny kod");
                }
            }
            
            Console.ReadKey(true);
        }
    }
}