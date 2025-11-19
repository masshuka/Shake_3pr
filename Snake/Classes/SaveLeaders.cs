using Common.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Classes
{
    /// <summary> Сохранение результата
    public static void SaveLeaders()
    {
        // Преобразуем данные игроков в JSON
        string json = JsonConvert.SerializeObject(Leaders);
        // Записываем в файл
        StreamWriter SM = new StreamWriter("./Leaders.txt");
        // Пишем строку
        SM.WriteLine(json);
        // Закрываем файл
        SM.Close();
    }
}
