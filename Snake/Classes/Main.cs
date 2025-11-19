using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snake.Classes
{
    static void Main(string[] args)
    {
        try
        {
            // Создаем поток для прослушивания сообщений от клиентов
            Thread tRec = new Thread(new ThreadStart(Receiver));
            // Запускаем поток прослушивания
            tRec.Start();
            // Создаём таймер для управления игрой
            Thread tTime = new Thread(Timer);
            // Запускаем таймер для управления игрой
            tTime.Start();
        }
        catch (Exception ex)
        {
            // Если что-то пошло не так, выводим сообщение о том что возникла ошибка
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
        }
    }
}
