using Common.Classes;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Classes
{
    /// <summary> Функция добавления змеи
    public static int AddSnake()
    {
        // Создаём змею пользователю
        ViewModelGames viewModelGamesPlayer = new ViewModelGames();
        // Указываем стартовые координаты змеи
        viewModelGamesPlayer.SnakesPlayers = new Snakes()
        {
            // Точки змеи
            Points = new List<Snakes.Point>()
        {
            new Snakes.Point() { X = 30, Y = 10 },
            new Snakes.Point() { X = 20, Y = 10 },
            new Snakes.Point() { X = 10, Y = 10 },
        },
            // Направление змеи
            direction = Snakes.Direction.Start
        };
        // Создаём рандомную точку на карте
        viewModelGamesPlayer.Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));
        // Добавляем змею в общий список всех змей
        viewModelGames.Add(viewModelGamesPlayer);
        // Возвращаем ID змеи чтобы связать игрока и змею
        return viewModelGames.FindIndex(x => x == viewModelGamesPlayer);
    }
}
