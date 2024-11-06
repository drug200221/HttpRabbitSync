
class Program
{
    static async Task Main(string[] args)
    {
        string? protocol = "";
        string? host = "";
        string? port = "";
        string? login = "";
        string? password = "";
        
        (protocol, host, port, login, password) = await SetAuth();
        
        string url = $"{protocol}://{host}:{port}/event?login={login}&password={password}&filter=427f1cc3-2c2f-4f50-8865-56ae99c3610d&responsetype=json";

        while (true)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Connection.Add("keep-alive");

                try
                {
                    using (HttpResponseMessage response =
                           await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var reader = new System.IO.StreamReader(contentStream))
                            {
                                while (!reader.EndOfStream)
                                {
                                    string? line = await reader.ReadLineAsync();
                                    Console.WriteLine(line);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
                    Console.ReadLine();
                }
            }
        }
    }

    private static async Task<(string? protocol, string? host, string? port, string? login, string? password)> SetAuth()
    {
        string? protocol;
        string? host;
        string? port;
        string? login;
        string? password;
        
        while (true)
        {
            Console.Write("Подключение по http|https (0|1): ");
            int k = Console.ReadKey().KeyChar;
            Console.WriteLine();
            if (k == 48)
            {
                protocol = "http";
                Console.WriteLine(protocol);
                break;
            } 
            if (k == 49)
            {
                protocol = "https";
                Console.WriteLine(protocol);
                break;
            }
        }
        
        Console.Write("Укажите хост: ");
        host = Console.ReadLine();
        while (host == "")
        {
            Console.Write("Хост не указан. Укажите хост: ");
            host = Console.ReadLine();
        }
        Console.Write("Укажите порт: ");
        port = Console.ReadLine();
        while (port == "")
        {
            Console.Write("Порт не указан. Укажите порт: ");
            port = Console.ReadLine();
        }

        using (HttpClient client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(5);
            Console.WriteLine("Ожидаю 5 сек.");
            try
            {
                using (var response =
                       await client.GetAsync($"{protocol}://{host}:{port}", HttpCompletionOption.ResponseHeadersRead))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Есть контакт!");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Ошибка: Запрос превысил время ожидания. Возможно не верно указан хост и/или порт.");
            }
        }

        Console.Write("Укажите логин: ");
        login = Console.ReadLine();
        while (login == "")
        {
            Console.Write("Логин не указан. Укажите логин: ");
            login = Console.ReadLine();
        }
        Console.Write("Укажите пароль: ");
        password = Console.ReadLine();
        
        return (protocol, host, port, login, password);
    }
}