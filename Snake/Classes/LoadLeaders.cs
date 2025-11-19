using Common.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Classes
{
    /// <summary> Загрузка результата
    public static void LoadLeaders()
    {
        // Проверяем что есть файл
        if (File.Exists("./leaders.txt"))
        {
            // Открываем файл
            StreamReader SR = new StreamReader("./leaders.txt");
            // читаем первую строку
            string json = SR.ReadLine();
            // Закрываем файл
            SR.Close();
            // Если есть что читать
            if (!string.IsNullOrEmpty(json))
                // Преобразуем строку в объект
                Leaders = JsonConvert.DeserializeObject<List<Leaders>>(json);
            else
                // Возвращаем пустой результат
                Leaders = new List<Leaders>();
        }
        else
            // Возвращаем пустой результат
            Leaders = new List<Leaders>();
    }
}
