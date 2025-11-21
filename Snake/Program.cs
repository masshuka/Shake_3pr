using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Classes;
using Common.Models;
using Newtonsoft.Json;
using System.IO;

namespace Snake
{
    class Program
    {
        /// <summary> Коллекция рекордов
        public static List<Leaders> Leaders = new List<Leaders>();

        /// <summary> Коллекция ViewModelUserSetting, содержащая IP адрес игрока, порт, ...
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();

        /// <summary> Коллекция ViewModelGames, содержащая точки змеи, точку на карте
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();

        /// <summary> Локальный порт, который прослушивается для ответов
        private static int localPort = 5001;

        /// <summary> Максимальная скорость движения змейки
        public static int MaxSpeed = 15;

        static void Main(string[] args)
        {
            try
            {
                Thread tRec = new Thread(new ThreadStart(Receiver));
                tRec.Start();
                Thread tTime = new Thread(new ThreadStart(Timer));
                tTime.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }

        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIPAddress)
            {
                UdpClient sender = new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(User.IPAddress), int.Parse(User.Port));
                try
                {
                    // Первое сообщение - твоя змейка
                    var mySnake = viewModelGames.Find(x => x.IdSnake == User.IdSnake);
                    if (mySnake != null)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mySnake));
                        sender.Send(bytes, bytes.Length, endPoint);
                    }

                    // Второе сообщение - все остальные змейки (может быть пустым)
                    var otherSnakes = viewModelGames.FindAll(x => x.IdSnake != User.IdSnake);
                    // Всегда отправляем список, даже если он пустой
                    byte[] otherBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(otherSnakes));
                    sender.Send(otherBytes, otherBytes.Length, endPoint);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Отправил данные пользователю: {User.IPAddress}:{User.Port} " +
                                    $"(моя змейка: {mySnake != null}, другие: {otherSnakes.Count})");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
                }
                finally
                {
                    sender.Close();
                }
            }
        }

        public static void Receiver()
        {
            UdpClient receivingUdpClient = new UdpClient(localPort);
            IPEndPoint RemoteIPEndPoint = null;
            try
            {
                Console.WriteLine("Команды сервера:");
                while (true)
                {
                    byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIPEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получил команду: " + returnData.ToString());

                    if (returnData.ToString().Contains("/start"))
                    {
                        string[] datamesage = returnData.ToString().Split(' ');
                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(datamesage[1]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Подключился пользователь: {viewModelUserSettings.IPAddress}:{viewModelUserSettings.Port}");
                        remoteIPAddress.Add(viewModelUserSettings);
                        viewModelUserSettings.IdSnake = AddSnake();
                        viewModelGames[viewModelUserSettings.IdSnake].IdSnake = viewModelUserSettings.IdSnake;
                    }
                    else
                    {
                        // Обработка команд управления
                        string[] datamesage = returnData.ToString().Split('|');
                        if (datamesage.Length >= 2)
                        {
                            string command = datamesage[0];
                            ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(datamesage[1]);

                            int IdPlayer = remoteIPAddress.FindIndex(x => x.IPAddress == viewModelUserSettings.IPAddress && x.Port == viewModelUserSettings.Port);

                            if (IdPlayer != -1)
                            {
                                int snakeId = remoteIPAddress[IdPlayer].IdSnake;
                                var snake = viewModelGames.Find(x => x.IdSnake == snakeId).SnakesPlayers;

                                // Обработка направлений
                                switch (command)
                                {
                                    case "Up":
                                        if (snake.direction != Snakes.Direction.Down) // Нельзя развернуться на 180°
                                            snake.direction = Snakes.Direction.Up;
                                        break;
                                    case "Down":
                                        if (snake.direction != Snakes.Direction.Up)
                                            snake.direction = Snakes.Direction.Down;
                                        break;
                                    case "Left":
                                        if (snake.direction != Snakes.Direction.Right)
                                            snake.direction = Snakes.Direction.Left;
                                        break;
                                    case "Right":
                                        if (snake.direction != Snakes.Direction.Left)
                                            snake.direction = Snakes.Direction.Right;
                                        break;
                                }
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"Игрок {viewModelUserSettings.Name} двигается: {snake.direction}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }

        public static int AddSnake()
        {
            ViewModelGames viewModelGamesPlayer = new ViewModelGames();
            viewModelGamesPlayer.SnakesPlayers = new Snakes()
            {
                Points = new List<Snakes.Point>()
                {
                    new Snakes.Point() { X = 30, Y = 10 },
                    new Snakes.Point() { X = 20, Y = 10 },
                    new Snakes.Point() { X = 10, Y = 10 },
                },
                direction = Snakes.Direction.Start
            };
            viewModelGamesPlayer.Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));
            viewModelGames.Add(viewModelGamesPlayer);
            return viewModelGames.FindIndex(x => x == viewModelGamesPlayer);
        }

        /// <summary> Таймер с игрой (тут происходит перемещение змей и обработка столкновений ...)
        public static void Timer()
        {
            
            while (true)
            {
                // останавливаем на 100 миллисекунд
                Thread.Sleep(100);

                // Получаем змей которых необходимо удалить
                List<ViewModelGames> RemoteSnakes = Program.viewModelGames.FindAll(x => x.SnakesPlayers.GameOver);
                // Если кол-во змей более 0
                if (RemoteSnakes.Count > 0)
                {
                    // Перебираем удалённых змей
                    foreach (ViewModelGames DeadSnake in RemoteSnakes)
                    {
                        // Говорим что отключаем игрока
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Отключил пользователя: {Program.remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).IPAddress}" + $":{remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).Port}");
                        // Удаляем пользователя
                        Program.remoteIPAddress.RemoveAll(x => x.IdSnake == DeadSnake.IdSnake);
                    }
                    // Удаляем змей которых необходимо удалить
                    Program.viewModelGames.RemoveAll(x => x.SnakesPlayers.GameOver);
                }

                // Перебираем подключенных игроков
                foreach (ViewModelUserSettings User in Program.remoteIPAddress)
                {
                    // Находим змею игрока
                    Snakes Snake = Program.viewModelGames.Find(x => x.IdSnake == User.IdSnake).SnakesPlayers;

                    // Если змея ещё не начала движение, пропускаем
                    if (Snake.direction == Snakes.Direction.Start)
                        continue;

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
                            if (Speed > Program.MaxSpeed) Speed = Program.MaxSpeed;

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
                    // проверяем змею на столкновение с препятствием
                    if (Snake.Points[0].X <= 0 || Snake.Points[0].X >= 793)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Игрок {User.Name} врезался в стену по X: {Snake.Points[0].X}");
                        Snake.GameOver = true;
                    }
                    else if (Snake.Points[0].Y <= 0 || Snake.Points[0].Y >= 420)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Игрок {User.Name} врезался в стену по Y: {Snake.Points[0].Y}");
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
                    if (Snake.Points[0].X >= Program.viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X - 15 &&
                        Snake.Points[0].X <= Program.viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X + 15)
                    {
                        // Проверяем что если первая точка змеи игрока находится в координатах яблока по вертикали
                        if (Snake.Points[0].Y >= Program.viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y - 15 &&
                            Snake.Points[0].Y <= Program.viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y + 15)
                        {
                            // создаём новое яблоко
                            Program.viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));
                            // Добавляем змее новую точку на координатах последней
                            Snake.Points.Add(new Snakes.Point()
                            {
                                X = Snake.Points[Snake.Points.Count - 1].X,
                                Y = Snake.Points[Snake.Points.Count - 1].Y
                            });
                            // загружаем таблицу
                            LoadLeaders();
                            // добавляем нас в таблицу
                            Program.Leaders.Add(new Leaders()
                            {
                                Name = User.Name,
                                Points = Snake.Points.Count - 3
                            });
                            // сортируем таблицу по двум значениям сначала по кол-ву точек затем по наименованию
                            Program.Leaders = Leaders.OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();
                            // Ищем себя в списке и записываем в модель змеи
                            Program.viewModelGames.Find(x => x.IdSnake == User.IdSnake).Top = Leaders.FindIndex(x => x.Points == Snake.Points.Count - 3 && x.Name == User.Name) + 1;
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

        public static void SaveLeaders()
        {
            try
            {
                // Убедимся что директория существует
                string directory = Path.GetDirectoryName("./leaders.txt");
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(Leaders, Formatting.Indented);
                File.WriteAllText("./leaders.txt", json);
                Console.WriteLine($"Сохранено {Leaders.Count} записей в таблицу лидеров");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка сохранения таблицы лидеров: " + ex.Message);
            }
        }

        public static void LoadLeaders()
        {
            try
            {
                if (File.Exists("./leaders.txt"))
                {
                    string json = File.ReadAllText("./leaders.txt");

                    // Проверяем что файл не пустой и содержит валидный JSON
                    if (!string.IsNullOrWhiteSpace(json) && json.Trim() != "[]")
                    {
                        Leaders = JsonConvert.DeserializeObject<List<Leaders>>(json);
                        Console.WriteLine($"Загружено {Leaders.Count} записей из таблицы лидеров");
                    }
                    else
                    {
                        Leaders = new List<Leaders>();
                        Console.WriteLine("Файл leaders.txt пустой, создан новый список лидеров");
                    }
                }
                else
                {
                    Leaders = new List<Leaders>();
                    Console.WriteLine("Файл leaders.txt не существует, создан новый список лидеров");
                }
            }
            catch (JsonSerializationException jsonEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка загрузки таблицы лидеров (неверный формат JSON): {jsonEx.Message}");
                Leaders = new List<Leaders>();

                // Создаем backup поврежденного файла
                try
                {
                    if (File.Exists("./leaders.txt"))
                    {
                        File.Move("./leaders.txt", "./leaders_corrupted_backup.txt");
                        Console.WriteLine("Создан backup поврежденного файла: leaders_corrupted_backup.txt");
                    }
                }
                catch (Exception backupEx)
                {
                    Console.WriteLine($"Не удалось создать backup: {backupEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка загрузки таблицы лидеров: {ex.Message}");
                Leaders = new List<Leaders>();
            }
        }
    }
}