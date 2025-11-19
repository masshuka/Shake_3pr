using Common.Classes;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Classes
{
    /// <summary> Метод приёма сообщений
    public static void Receiver()
    {
        // Создаем UdpClient для чтения входящих данных
        UdpClient receivingUdpClient = new UdpClient(localPort);
        // Конечная сетевая точка
        IPEndPoint RemoteIPEndPoint = null;
        try
        {
            // Выводим сообщение
            Console.WriteLine("Команды сервера:");
            // Запускаем бесконечный цикл для прослушки приходящих сообщений
            while (true)
            {
                // Ожидание датаграммы
                byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIPEndPoint);
                // Преобразуем и отображаем данные
                string returnData = Encoding.UTF8.GetString(receiveBytes);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Получил команду: " + returnData.ToString());

                // начало игры
                if (returnData.ToString().Contains("/start"))
                {
                    // делим данные на команду и данные Json
                    string[] datamesage = returnData.ToString().Split(' ');
                    // Конвертируем данные в модель
                    ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(datamesage[1]);
                    // Выводим запись в консоль
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Подключился пользователь: {viewModelUserSettings.IPAddress}:{viewModelUserSettings.Port}");
                    // Добавляем данные в коллекцию для того, чтобы отправлять пользователю
                    remoteIPAddress.Add(viewModelUserSettings);
                    // добавляем змею
                    viewModelUserSettings.IdSnake = AddSnake();
                    // связываем змею и игрока
                    viewModelGames[viewModelUserSettings.IdSnake].IdSnake = viewModelUserSettings.IdSnake;
                }
                else
                {
                    // Если команда не является стартом, значит:
                    // управление змеёй
                    string[] datamesage = returnData.ToString().Split(' ');
                    // Конвертируем данные в модель
                    ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(datamesage[1]);
                    // Получаем ID игрока
                    int IdPlayer = -1;
                    // В случае если мёртвый игрок присылает команду
                    // Находим ID игрока, ища его в списке по IP адресу и Порту
                    IdPlayer = remoteIPAddress.FindIndex(x => x.IPAddress == viewModelUserSettings.IPAddress && x.Port == viewModelUserSettings.Port);
                    // Если игрок найден
                    if (IdPlayer != -1)
                    {
                        // Если команда вверх, и если змея не ползёт вниз
                        if (datamesage[0] == "Up" &&
                            viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Down)
                            // Змее игрока указываем команду вверх
                            viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Up;
                        // Если команда вниз и если змея не ползёт вверх
                        else if (datamesage[0] == "Down" &&
                                 viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Up)
                            // Змее игрока указываем команду вниз
                            viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Down;
                        // Если команда влево и змея не ползёт вправо
                        else if (datamesage[0] == "Left" &&
                                 viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Right)
                            // Змее игрока указываем команду влево
                            viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Left;
                        // Если команда вправо и змея не ползёт влево
                        else if (datamesage[0] == "Right" &&
                                 viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Left)
                            // Змее игрока указываем команду вправо
                            viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Right;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Если в ходе работы возникли какие-то ошибки выводим в консоль
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
        }
    }
}
