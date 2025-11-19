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
    /// <summary> Функция ответа, которая рассылает сообщения пользователям
    private static void Send()
    {
        // Перебираем модели пользователей
        foreach (ViewModelUserSettings User in remoteIPAddress)
        {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();
            // Создаем endPoint по информации об удаленном хосте
            IPEndPoint endPoint = new IPEndPoint(
                // Преобразуем IP адрес
                IPAddress.Parse(User.IPAddress),
                // Преобразуем порт
                int.Parse(User.Port));
            try
            {
                // Преобразуем данные в массив байтов
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelGames.Find(x => x.IdSnake == User.IdSnake)));
                // Отправляем данные
                sender.Send(bytes, bytes.Length, endPoint);
                // Выводим ответ в консоль
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Отправил данные пользователю: {User.IPAddress}:{User.Port}");
            }
            catch (Exception ex)
            {
                // Если возникли какие-либо проблемы, выводим результат об ошибке в консоль
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
            finally
            {
                // Если всё выполнилось превосходно, закрываем UdpClient
                sender.Close();
            }
        }
    }
}
