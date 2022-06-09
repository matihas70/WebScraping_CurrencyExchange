using scrap;
using MySql.Data.MySqlClient;

Currencies.getCurrencies();

string[] prop = System.IO.File.ReadAllLines(@"D:\C#\Projects\scrap\scrap\Dbase.txt");

string serwer = prop[0][7..],
       port = prop[1][5..],
       database = prop[2][9..],
       user = prop[3][5..],
       password = prop[4][9..];


using (MySqlConnection connection = new MySqlConnection())
{
    connection.ConnectionString = $"Server = {serwer}; Port = {port}; Database = {database}; Uid = {user}; Pwd = {password}";
    connection.Open();
    Console.WriteLine("CONNECTED TO DATA BASE!");

    string sql = "SELECT DISTINCT data_aktualizacji FROM waluty";
    MySqlCommand cmd = new MySqlCommand(sql, connection);
    var reader = cmd.ExecuteReader();

    ConsoleKeyInfo key;
    bool toUpdate = false;

    if (reader.Read()) 
    {
        if ((DateTime)reader["data_aktualizacji"] == DateTime.Today)
        {
            reader.Close();
            Console.WriteLine("Dane są aktualne");  
            Console.ReadKey(true);
        }
        else if (Currencies.PageAcces & (DateTime)reader["data_aktualizacji"] != DateTime.Today)
        {
            DateTime lastUpdate = (DateTime)reader["data_aktualizacji"];
            reader.Close();
            Console.WriteLine($"Dane są nieaktualne. Ostatnia aktualizacja: {lastUpdate.Date}. Zaktualizować?");
            Console.WriteLine("Tak - Enter, Nie - Cokolwiek");

            key = Console.ReadKey(true);

            if(key.Key == ConsoleKey.Enter)
            {
                Currencies.updateDB(connection);
            }
            else
            {
                toUpdate = true;
            }
        }
        else if(Currencies.PageAcces == false & (DateTime)reader["data_aktualizacji"] != DateTime.Today)
        {
            DateTime lastUpdate = (DateTime)reader["data_aktualizacji"];
            reader.Close();
            Console.WriteLine($"Dane są nieaktualne. Ostatnia aktualizacja: {lastUpdate.Date}. Nie można zaktualizować, brak dostępu do strony");
            Console.ReadKey(true);
        }
        Currencies.getFromDB(connection);
        while (true)
        {
            Console.Clear();
            if(toUpdate == false)
            {
                Console.WriteLine("Pokaż całą tablice - 1, Wyświetl wybrany kurs - 2, Zakończ program - Esc");
            }
            else
            {
                Console.WriteLine("Pokaż całą tablice - 1, Wyświetl wybrany kurs - 2, Zaktualizuj dane - Enter, Zakończ program - Esc");
            }
            

            key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.D1)
            {
                Currencies.show();
            }
            else if (key.Key == ConsoleKey.D2)
            {
                while (true)
                {
                    Currencies.singleCourse();

                    Console.Clear();

                    Console.WriteLine("Jeszcze raz? Tak - T, Nie - Coś innego");

                    key = Console.ReadKey(true);

                    if (key.Key != ConsoleKey.T) break;
                }
            }
            else if (toUpdate & key.Key == ConsoleKey.Enter)
            {
                Currencies.updateDB(connection);
                Currencies.getFromDB(connection);
                toUpdate = false;
            }
            else if (key.Key == ConsoleKey.Escape) break;
        }
    }
    else
    {
        Console.WriteLine("Brak danych. Pobrać dane?");
        Console.WriteLine("Tak - Enter, Nie - Cokolwiek");

        key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Enter)
        {
            Currencies.sendToDB(connection);
        }
    }
}