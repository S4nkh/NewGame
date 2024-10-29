using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Player
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int BestScore { get; set; }
        public Player() 
        {
        }
        public void CheckScore(int newScore)
        {
            Console.WriteLine($"Ваш результат: {newScore}");
            if (newScore < this.BestScore) 
            {
                Console.WriteLine("Новый рекорд!");
                this.BestScore = newScore;
            }
            else 
            {
                Console.WriteLine($"Ваш лучший результат: {this.BestScore}");
            }
        }
    }
}
