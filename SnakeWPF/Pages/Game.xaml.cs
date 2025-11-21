using Common.Classes;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace SnakeWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        /// <summary> Кадр отрисовки
        public int StepCadr = 0;

        /// <summary> Все остальные змейки
        public List<ViewModelGames> AllSnakes { get; set; }

        /// <summary> Моя змейка
        public ViewModelGames MySnake { get; set; }

        public Game()
        {
            InitializeComponent();
        }

        public void CreateUI()
        {
            // Выполняем вне потока
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // Если кадр 0 то кадр 1
                    if (StepCadr == 0) StepCadr = 1;
                    // Если кадр 1 то 0
                    else StepCadr = 0;

                    // Чистим интерфейс
                    canvas.Children.Clear();

                    // Отрисовываем ВСЕХ змеек из списка AllSnakes
                    if (AllSnakes != null && AllSnakes.Count > 0)
                    {
                        foreach (var snakeGame in AllSnakes)
                        {
                            if (snakeGame?.SnakesPlayers?.Points != null && snakeGame.SnakesPlayers.Points.Count > 0)
                            {
                                DrawSnake(snakeGame.SnakesPlayers, false); // false - чужая змейка
                            }
                        }
                    }

                    // Отрисовываем свою змейку
                    if (MySnake != null && MySnake.SnakesPlayers?.Points != null && MySnake.SnakesPlayers.Points.Count > 0)
                    {
                        DrawSnake(MySnake.SnakesPlayers, true); // true - моя змейка

                        // Отрисовка яблока для своей змейки
                        if (MySnake.Points != null)
                        {
                            DrawApple(MySnake.Points);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка в CreateUI: {ex.Message}");
                }
            });
        }

        private void DrawSnake(Snakes snake, bool isMySnake)
        {
            if (snake?.Points == null || snake.Points.Count == 0)
            {
                Debug.WriteLine("Пустая змейка, пропускаем отрисовку");
                return;
            }

            // Перебираем точки змеи
            for (int iPoint = snake.Points.Count - 1; iPoint >= 0; iPoint--)
            {
                // Получаем точку
                Snakes.Point SnakePoint = snake.Points[iPoint];

                // Смещение точек змеи (твоя старая логика)
                if (iPoint != 0)
                {
                    // Получаем следующую точку змеи
                    Snakes.Point NextSnakePoint = snake.Points[iPoint - 1];
                    // Если точка находится по горизонтали
                    if (SnakePoint.X > NextSnakePoint.X || SnakePoint.X < NextSnakePoint.X)
                    {
                        // если точка чётная
                        if (iPoint % 2 == 0)
                        {
                            // если кадр чётный
                            if (StepCadr % 2 == 0)
                                // смещаем в одну сторону
                                SnakePoint.Y -= 1;
                            else
                                // смещаем в другую сторону
                                SnakePoint.Y += 1;
                        }
                        else
                        {
                            // если кадр не чётный
                            if (StepCadr % 2 == 0)
                                // смещаем в одну сторону
                                SnakePoint.Y += 1;
                            else
                                // смещаем в другую сторону
                                SnakePoint.Y -= 1;
                        }
                    }
                    // Если точка находится по вертикали
                    else if (SnakePoint.Y > NextSnakePoint.Y || SnakePoint.Y < NextSnakePoint.Y)
                    {
                        // если точка чётная
                        if (iPoint % 2 == 0)
                        {
                            // если кадр чётный
                            if (StepCadr % 2 == 0)
                                // смещаем в одну сторону
                                SnakePoint.X -= 1;
                            else
                                // смещаем в другую сторону
                                SnakePoint.X += 1;
                        }
                        else
                        {
                            // если кадр не чётный
                            if (StepCadr % 2 == 0)
                                // смещаем в одну сторону
                                SnakePoint.X += 1;
                            else
                                // смещаем в другую сторону
                                SnakePoint.X -= 1;
                        }
                    }
                }

                // Цвет для точки
                Brush Color;
                if (isMySnake)
                {
                    // Если моя змейка - зеленые цвета
                    if (iPoint == 0)
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 127, 14)); // Темно-зеленый
                    else
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 198, 19)); // Светло-зеленый
                }
                else
                {
                    // Если чужая змейка - синие цвета
                    if (iPoint == 0)
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 14, 127)); // Темно-синий
                    else
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 19, 198)); // Светло-синий
                }

                // Рисуем точку
                Ellipse ellipse = new Ellipse()
                {
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(SnakePoint.X - 10, SnakePoint.Y - 10, 0, 0),
                    Fill = Color,
                    Stroke = Brushes.Black
                };
                canvas.Children.Add(ellipse);
            }
        }

        private void DrawApple(Snakes.Point applePoint)
        {
            // Отрисовка яблока
            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/ImageApple.png", UriKind.Absolute));

            Ellipse points = new Ellipse()
            {
                Width = 40,
                Height = 40,
                Margin = new Thickness(applePoint.X - 20, applePoint.Y - 20, 0, 0),
                Fill = myBrush
            };
            canvas.Children.Add(points);
        }
    }
}