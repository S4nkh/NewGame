using Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

class Program
{
    static string playersFilePath = "players.json";

    static void Main(string[] args)
    {
        Player player = null;
        bool endGame = false;
        while (player == null)
            player = Enter();
        while (!endGame)
        {
            Console.WriteLine("Меню:");
            Console.WriteLine("1 - Новая игра");
            Console.WriteLine("2 - Войти в другую учетную запись");
            Console.WriteLine("3 - Список лидеров");
            Console.WriteLine("4 - Выйти из игры");
            string menuChoice = Console.ReadLine();
            switch(menuChoice)
            {
                case "1":
                    Game(player);
                    break;
                case "2":
                    player = Enter();
                    break;
                case "3":
                    Leaderboards();
                    break;
                case "4":
                    endGame = true;
                    break;
                default:
                    Console.WriteLine("Неверный пункт меню");
                    break;
            }
        }
        Console.WriteLine("Спасибо за игру!");
        Console.ReadLine();
    }

    public static string GenNum(Random random)
    {
        List<int> nums = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        string number = "";
        for (int i = 0; i < 4; i++)
        {
            int index = random.Next(0, nums.Count);
            number += nums[index];
            nums.RemoveAt(index);
        }
        return number;
    }

    public static int CountPos(string secret, string guess)
    {
        int count = 0;
        for (int i = 0; i < secret.Length; i++)
        {
            if (secret[i] == guess[i])
            {
                count++;
            }
        }
        return count;
    }

    public static int CountNum(string secret, string guess)
    {
        int count = 0;
        bool[] isCounted = new bool[4];
        for (int i = 0; i < guess.Length; i++)
        {
            for (int j = 0; j < secret.Length; j++)
            {
                if (secret[i] == guess[j] && !isCounted[j])
                {
                    count++;
                    isCounted[j] = true;
                    break;
                }
            }
        }
        return count;
    }

    static void Game(Player p)
    {
        Random rnd = new Random();
        string secretNum = GenNum(rnd);
        Console.WriteLine(secretNum);
        Console.WriteLine("Угадайте 4-х значное число");
        int attempts = 0;
        bool guessed = false;
        while (!guessed)
        {
            string guess = Console.ReadLine();
            if (guess.Length != 4 || !guess.All(char.IsDigit))
            {
                Console.WriteLine("Введите 4-х значное число.");
                continue;
            }
            attempts++;
            int corPos = CountPos(secretNum, guess);
            int corNum = CountNum(secretNum, guess);
            if (corPos == 4)
            {
                guessed = true;
                Console.WriteLine($"Вы угадали число {secretNum}.");
                p.CheckScore(attempts);
                List<Player> players = LoadPlayers();
                Player pUpd = players.FirstOrDefault(player => player.Username == p.Username);
                if (pUpd != null)
                {
                    pUpd.BestScore = p.BestScore;
                }
                SavePlayers(players);
            }
            else
            {
                Console.WriteLine($"Угадано: {corNum}, на своих позициях: {corPos}.");
            }
        }
    }
    public static Player Enter()
    {
        Player player = null;
        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("1 - Вход");
            Console.WriteLine("2 - Регистрация");
            Console.WriteLine("3 - Выход");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    player = Login();
                    return player;
                case "2":
                    player = Register();
                    return player;
                case "3":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Неверный пункт меню");
                    break;
            }
        }
        return player;
    }
    public static List<Player> LoadPlayers()
    {
        if (!File.Exists(playersFilePath))
        {
            return new List<Player>();
        }
        string json = File.ReadAllText(playersFilePath);
        return JsonConvert.DeserializeObject<List<Player>>(json);
    }
    public static void SavePlayers(List<Player> players = null)
    {
        if (players == null)
        {
            players = LoadPlayers();
        }

        string json = JsonConvert.SerializeObject(players, Formatting.Indented);
        File.WriteAllText(playersFilePath, json);
    }
    public static Player Login()
    {
        List<Player> pList = LoadPlayers();
        bool reset = false;
        while (!reset)
        {
            if (pList.Count == 0)
            {
                Console.WriteLine("Список игроков пуст");
                return null;
            }
            Console.WriteLine("Введите логин: ");
            string username = Console.ReadLine();
            foreach (Player p in pList)
            {
                if (p.Username == username)
                {
                    bool cancel = false;
                    string password;
                    while (!cancel)
                    {
                        Console.WriteLine("Введите пароль: ");
                        password = Console.ReadLine();
                        if (p.Password == password)
                            return p;
                        else if (password == "exit")
                            return null;
                        else
                        {
                            Console.WriteLine("Неверный пароль, повторите попытку");
                            Console.WriteLine("Чтобы выйти, введите - exit");
                        }
                    }
                }
            }
            if (username == "exit")
                reset = true;
            Console.WriteLine("Логин не найден, повторите попытку");
            Console.WriteLine("Чтобы выйти из меню входа, введите - exit");
        }
        return null;
    }
    public static Player Register()
    {
        bool newU = true;
        List<Player> pList = LoadPlayers();
        Player newP = new Player();
        do
        {
            Console.WriteLine("Введите новый логин: ");
            newP.Username = Console.ReadLine();
            newU = true;
            foreach (Player p in pList)
            {
                if (p.Username == newP.Username)
                {
                    Console.WriteLine("Данный пользователь уже существует");
                    newU = false;
                }
            }
        } while (!newU);
        Console.WriteLine("Введите пароль: ");
        newP.Password = Console.ReadLine();
        newP.BestScore = int.MaxValue;
        pList.Add(newP);
        SavePlayers(pList);
        return newP;
    }
    public static void Leaderboards()
    {
        List<Player> pList = LoadPlayers().OrderBy(p => p.BestScore).ToList();
        Console.WriteLine("Игрок \t\t\t\t Рекорд");
        foreach (Player p in pList) 
        {
            if (p.BestScore != int.MaxValue)
            {
                Console.WriteLine($"{p.Username} \t\t\t\t {p.BestScore}");
            }
        }
    }
}