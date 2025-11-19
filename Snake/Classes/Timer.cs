using Common.Classes;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snake.Classes
{
    /// <summary> Таймер с игрой (тут происходит перемещение змей и обработка столкновений ...)
    public static void Timer()
    {
        while (true)
        {
            // останавливаем на 100 миллисекунд
            Thread.Sleep(100);

            // Получаем змей которых необходимо удалить
            List<ViewModelGames> RemoteSnakes = viewModelGames.FindAll(x => x.SnakesPlayers.GameOver);
            // Если кол-во змей более 0
            if (RemoteSnakes.Count > 0)
            {
                // Перебираем удалённых змей
                foreach (ViewModelGames DeadSnake in RemoteSnakes)
                {
                    // Говорим что отключаем игрока
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Отключил пользователя: {remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).IPAddress}" + $":{remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).Port}");
                    // Удаляем пользователя
                    remoteIPAddress.RemoveAll(x => x.IdSnake == DeadSnake.IdSnake);
                }
                // Удаляем змей которых необходимо удалить
                viewModelGames.RemoveAll(x => x.SnakesPlayers.GameOver);
            }

            // Перебираем подключенных игроков
            foreach (ViewModelUserSettings User in remoteIPAddress)
            {
                // Находим змею игрока
                Snakes Snake = viewModelGames.Find(x => x.IdSnake == User.IdSnake).SnakesPlayers;
                // Проходим точки змеи через цикл от конца в начало
                for (int i = Snake.Points.Count - 1; i >= 0; i--)
                {
                    // Если у нас не первая точка
                    if (i != 0)
                    {
                        // Перемещаем точку на место предыдущей
                        Snake.Points[i] = Snake.Points[i - 1];
                    }
                    else
                    {
                        // Получаем скорость змеи (Поскольку радиус точки 10, начальная скорость 10 пунктов)
                        int Speed = 10 + (int)Math.Round(Snake.Points.Count / 20f);
                        // Если скорость змеи более максимальной скорости
                        if (Speed > MaxSpeed) Speed = MaxSpeed;

                        // Если направление змеи вправо
                        if (Snake.direction == Snakes.Direction.Right)
                        {
                            // Двигаем змею вправо
                            Snake.Points[i] = new Snakes.Point() { X = Snake.Points[i].X + Speed, Y = Snake.Points[i].Y };
                        }
                        // Если направление вниз
                        else if (Snake.direction == Snakes.Direction.Down)
                        {
                            // Двигаем вниз
                            Snake.Points[i] = new Snakes.Point() { X = Snake.Points[i].X, Y = Snake.Points[i].Y + Speed };
                        }
                        // Если направление вверх
                        else if (Snake.direction == Snakes.Direction.Up)
                        {
                            // Двигаем вверх
                            Snake.Points[i] = new Snakes.Point() { X = Snake.Points[i].X, Y = Snake.Points[i].Y - Speed };
                        }
                        // Если направление влево
                        else if (Snake.direction == Snakes.Direction.Left)
                        {
                            // Двигаем влево
                            Snake.Points[i] = new Snakes.Point() { X = Snake.Points[i].X - Speed, Y = Snake.Points[i].Y };
                        }
                    }
                }

                // проверяем змею на столкновение с препятствием
                // Если первая точка змеи вышла за координаты экрана по горизонтали
                if (Snake.Points[0].X <= 0 || Snake.Points[0].X >= 793)
                {
                    // Говорим что игра окончена
                    Snake.GameOver = true;
                }
                else if (Snake.Points[0].Y <= 0 || Snake.Points[0].Y >= 420)
                {
                    // Говорим что игра окончена
                    Snake.GameOver = true;
                }

                // проверяем что мы не столкнулись сами с собой
                if (Snake.direction != Snakes.Direction.Start)
                {
                    // Прогоняем все точки кроме первой
                    for (int i = 1; i < Snake.Points.Count; i++)
                    {
                        // Если первая точка находится в координатах последующей по горизонтали
                        if (Snake.Points[0].X >= Snake.Points[i].X - 1 && Snake.Points[0].X <= Snake.Points[i].X + 1)
                        {
                            // Если первая точка находится в координатах по вертикали
                            if (Snake.Points[0].Y >= Snake.Points[i].Y - 1 && Snake.Points[0].Y <= Snake.Points[i].Y + 1)
                            {
                                // Говорим что игра окончена
                                Snake.GameOver = true;
                                // останавливаем цикл
                                break;
                            }
                        }
                    }
                }

                // Проверяем что если первая точка змеи игрока находится в координатах яблока по горизонтали
                if (Snake.Points[0].X >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X - 15 &&
                    Snake.Points[0].X <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X + 15)
                {
                    // Проверяем что если первая точка змеи игрока находится в координатах яблока по вертикали
                    if (Snake.Points[0].Y >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y - 15 &&
                        Snake.Points[0].Y <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y + 15)
                    {
                        // создаём новое яблоко
                        viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));
                        // Добавляем змее новую точку на координатах последней
                        Snake.Points.Add(new Snakes.Point()
                        {
                            X = Snake.Points[Snake.Points.Count - 1].X,
                            Y = Snake.Points[Snake.Points.Count - 1].Y
                        });
                        // загружаем таблицу
                        LoadLeaders();
                        // добавляем нас в таблицу
                        Leaders.Add(new Leaders()
                        {
                            Name = User.Name,
                            Points = Snake.Points.Count - 3
                        });
                        // сортируем таблицу по двум значениям сначала по кол-ву точек затем по наименованию
                        Leaders = Leaders.OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();
                        // Ищем себя в списке и записываем в модель змеи
                        viewModelGames.Find(x => x.IdSnake == User.IdSnake).Top = Leaders.FindIndex(x => x.Points == Snake.Points.Count - 3 && x.Name == User.Name) + 1;
                    }
                }
                // Если игра для змеи закончена
                if (Snake.GameOver)
                {
                    // Загружаем таблицу
                    LoadLeaders();
                    // Добавляем нас в таблицу
                    Leaders.Add(new Leaders()
                    {
                        // Указываем никнейм
                        Name = User.Name,
                        // Указываем кол-во яблок которое собрал пользователь
                        Points = Snake.Points.Count - 3
                    });
                    // Сохраняем результаты
                    SaveLeaders();
                }
            }
            // Рассылаем пользователям ответ
            Send();

        }
    }
}
