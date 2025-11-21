using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Common.Models;
using System.Windows.Media.Animation;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SnakeWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary> Главное окно, используется для общения между страницами
        public static MainWindow mainWindow;

        /// <summary> Модель данных для передачи IP адреса устройства, порта и никнейма
        public ViewModelUserSettings ViewModelUserSettings = new ViewModelUserSettings();

        /// <summary> Модель игрока в который передаются координаты змеи, яблок и точки ...
        public ViewModelGames ViewModelGames = null;

        /// <summary> Удалённый IP адрес для подключения к серверу
        public static IPAddress remoteIPAddress = IPAddress.Parse("127.0.0.1");

        /// <summary> Удалённый порт для подключения к серверу
        public static int remotePort = 5001;

        /// <summary> Основной поток для получения данных о игре
        public Thread tRec;

        /// <summary> UDP клиент для получения данных
        public UdpClient receivingUdpClient;

        /// <summary> Страница HOME
        public Pages.Home Home = new Pages.Home();

        /// <summary> Страница GAME
        public Pages.Game Game = new Pages.Game();

        public MainWindow()
        {
            InitializeComponent();
            // Запоминаем MainWindow в переменную, чтобы обращаться в случае чего
            mainWindow = this;
            // Открываем начальную страницу
            OpenPage(Home);
        }

        /// <summary> Начинаем слушать ответы от сервера
        public void StartReceiver()
        {
            // Создаём поток для прослушивания канала
            tRec = new Thread(new ThreadStart(Receiver));
            // Запускаем поток
            tRec.Start();
        }

        /// <summaryу> Открываем страницу Home
        public void OpenPage(Page PageOpen)
        {
            // Создаём анимацию
            DoubleAnimation startAnimation = new DoubleAnimation();
            // Задаём начальное значение анимации
            startAnimation.From = 1;
            // Задаём конечное значение анимации
            startAnimation.To = 0;
            // Задаём время анимации
            startAnimation.Duration = TimeSpan.FromSeconds(0.6);
            // Подписываемся на выполнение анимации
            startAnimation.Completed += delegate
            {
                // Переключаем страницу
                frame.Navigate(PageOpen);
                // Создаём конечную анимацию
                DoubleAnimation endAnimation = new DoubleAnimation();
                // Задаём начальное значение анимации
                endAnimation.From = 0;
                // Задаём конечное значение анимации
                endAnimation.To = 1;
                // Задаём время анимации
                endAnimation.Duration = TimeSpan.FromSeconds(0.6);
                // Воспроизводим анимацию на framе, анимация прозрачности
                frame.BeginAnimation(OpacityProperty, endAnimation);
            };
            // Воспроизводим анимацию на frameе, анимация прозрачности
            frame.BeginAnimation (OpacityProperty, startAnimation);
        }

        /// <summary> Прослушиваем канал
        public void Receiver()
        {
            // Создаём клиент для прослушивания
            receivingUdpClient = new UdpClient(int.Parse(ViewModelUserSettings.Port));
            // Конечная сетевая точка
            IPEndPoint RemoteIPEndPoint = null;

            try
            {
                // Слушаем постоянно
                while (true)
                {
                    // Ожидание дейтаграммы
                    byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIPEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);

                    // Если у нас не существует данных от сервера (значит мы только начали игру)
                    if (ViewModelGames == null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OpenPage(Game);
                        });
                    }

                    try
                    {
                        // Первое сообщение - твоя змейка
                        ViewModelGames = JsonConvert.DeserializeObject<ViewModelGames>(returnData);

                        // Второе сообщение - все остальные змейки
                        receiveBytes = receivingUdpClient.Receive(ref RemoteIPEndPoint);
                        returnData = Encoding.UTF8.GetString(receiveBytes);

                        List<ViewModelGames> otherSnakes = new List<ViewModelGames>();
                        if (!string.IsNullOrEmpty(returnData) && returnData != "[]")
                        {
                            otherSnakes = JsonConvert.DeserializeObject<List<ViewModelGames>>(returnData);
                        }

                        // Сохраняем все змейки для отрисовки
                        Game.AllSnakes = otherSnakes;
                        Game.MySnake = ViewModelGames;

                        // Если игрок проиграл
                        if (ViewModelGames.SnakesPlayers.GameOver)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                OpenPage(new Pages.EndGame());
                            });
                        }
                        else
                        {
                            // Вызываем создание UI
                            Game.CreateUI();
                        }
                    }
                    catch (JsonSerializationException jsonEx)
                    {
                        Debug.WriteLine($"Ошибка десериализации JSON: {jsonEx.Message}");
                        Debug.WriteLine($"Данные: {returnData}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка обработки данных: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Возникло исключение в Receiver: " + ex.ToString() + "\n " + ex.Message);
            }
        }

        /// <summary> Отправляем команды
        public static void Send(string datagram)
        {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();
            // Создаем endPoint по информации об удаленном хосте
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);
            try
            {
                // Преобразуем данные в массив байтов
                byte[] bytes = Encoding.UTF8.GetBytes(datagram);

                // Отправляем данные
                sender.Send(bytes, bytes.Length, endPoint);
            }
            catch (Exception ex)
            {
                // если что-то пошло не по плану, выводим ошибку в консоль проекта
                Debug.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
            finally
            {
                // Закрыть соединение
                sender.Close();
            }
        }

        /// <summary> Управление змеёй
        private void EventKeyUp(object sender, KeyEventArgs e)
        {
            // Проверяем что у игрока есть IP адрес, порт, данные о змее, и что змея не проиграла
            if (!string.IsNullOrEmpty(ViewModelUserSettings.IPAddress) &&
                !string.IsNullOrEmpty(ViewModelUserSettings.Port) &&
                (ViewModelGames != null && !ViewModelGames.SnakesPlayers.GameOver))
            {
                // Если нажата клавиша вверх
                if (e.Key == Key.Up)
                    // Отправляем на сервер сообщение о том что команда вверх и данные игрока
                    Send($"Up|{JsonConvert.SerializeObject(ViewModelUserSettings)}");
                // Если нажата клавиша вниз
                else if (e.Key == Key.Down)
                    // Отправляем на сервер сообщение о том что команда вниз и данные игрока
                    Send($"Down|{JsonConvert.SerializeObject(ViewModelUserSettings)}");
                // Если нажата клавиша влево
                else if (e.Key == Key.Left)
                    // Отправляем на сервер сообщение о том что команда влево и данные игрока
                    Send($"Left|{JsonConvert.SerializeObject(ViewModelUserSettings)}");
                // Если нажата клавиша вправо
                else if (e.Key == Key.Right)
                    // Отправляем на сервер сообщение о том что команда вправо и данные игрока
                    Send($"Right|{JsonConvert.SerializeObject(ViewModelUserSettings)}");
            }
        }

        /// <summary> Выход из приложения
        private void QuitApplication(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // закрываем соединение
            receivingUdpClient.Close();
            // останавливаем поток
            tRec.Abort();
        }
    }
}