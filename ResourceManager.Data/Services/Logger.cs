using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager.Data.Services
{
    public class Logger : ILoggerService
    {
        /// <summary>
        /// Metoda, logująca wyjątek do pliku o podanej ścieżce
        /// </summary>
        /// <param name="ex">Wyjątek, który ma zostać zalogowany do pliku</param>
        /// <param name="path">Nazwa pliku w którym będą logi</param>
        public async Task LogToFile(Exception? ex, string path)
        {
            await Task.Factory.StartNew(() =>
            {
                var contents = new string[]
                {
                "\r\nLog Entry : ",
                $"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}",
                "  :",
                $"  :{ex.Message}",
                "-------------------------------"
                };
                File.AppendAllLines(path, contents);
            });
        }

        /// <summary>
        /// Metoda, logująca wyjątek do pliku o podanej ścieżce
        /// </summary>
        /// <param name="ex">Wyjątek, który ma zostać zalogowany do pliku</param>
        /// <param name="path">Ścieżka pliku w którym będą logi</param>
        public async Task LogToFile(string message, string path)
        {
            await Task.Factory.StartNew(() =>
            {
                var contents = new string[] {
                    message,
                    "----------------------"
                };
                File.AppendAllLines(path, contents);
            });
        }
    }
}
