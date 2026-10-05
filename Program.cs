using System;
using System.Diagnostics;
using System.IO;
using System.Management; // Требуется установка NuGet-пакета System.Management
using System.Text.RegularExpressions;

namespace BlackMythBenchmarkAutomation
{
    class Program
    {
        // УКАЖИТЕ ЗДЕСЬ ВАШИ ПУТИ К ФАЙЛАМ
        static string benchmarkExePath = @"C:\Program Files (x86)\Steam\steamapps\common\Black Myth Wukong Benchmark Tool\b1\Binaries\Win64\BlackMythWukongBenchmark.exe";
        static string configPath = @"C:\Users\ВашеИмя\AppData\Local\BlackMythWukong\Saved\Config\Windows\GameUserSettings.ini";
        static string resultPath = @"C:\Users\ВашеИмя\AppData\Local\BlackMythWukong\Saved\BenchmarkResults.txt";

        static void Main(string[] args)
        {
            Console.WriteLine("=== Сбор информации о системе ===");
            PrintSystemInfo();

            // --- ПРОХОД 1: CPU-ТЕСТ ---
            Console.WriteLine("\n=== Запуск CPU-теста ===");
            // Настройки для CPU-теста: минимальное разрешение и графика
            ApplySettings(configPath, "1280x720", "Low", "false", "false");
            RunBenchmark(benchmarkExePath);
            string cpuResult = ParseResults(resultPath);
            Console.WriteLine($"Результат CPU-теста: {cpuResult}");

            // --- ПРОХОД 2: GPU-ТЕСТ ---
            Console.WriteLine("\n=== Запуск GPU-теста ===");
            // Настройки для GPU-теста: максимальное разрешение и графика
            ApplySettings(configPath, "3840x2160", "Cinematic", "true", "false");
            RunBenchmark(benchmarkExePath);
            string gpuResult = ParseResults(resultPath);
            Console.WriteLine($"Результат GPU-теста: {gpuResult}");

            // --- ИТОГОВЫЙ ОТЧЕТ ---
            Console.WriteLine("\n=== ИТОГОВЫЙ ОТЧЕТ ===");
            Console.WriteLine($"CPU-тест: {cpuResult} (Настройки: 720p, Low)");
            Console.WriteLine($"GPU-тест: {gpuResult} (Настройки: 4K, Cinematic)");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static void PrintSystemInfo()
        {
            try
            {
                // Получение информации о CPU
                ManagementObjectSearcher cpuSearcher = new ManagementObjectSearcher("select * from Win32_Processor");
                foreach (ManagementObject obj in cpuSearcher.Get())
                    Console.WriteLine($"CPU: {obj["Name"]}");

                // Получение информации о GPU
                ManagementObjectSearcher gpuSearcher = new ManagementObjectSearcher("select * from Win32_VideoController");
                foreach (ManagementObject obj in gpuSearcher.Get())
                    Console.WriteLine($"GPU: {obj["Name"]}");

                // Получение информации о RAM
                ManagementObjectSearcher ramSearcher = new ManagementObjectSearcher("select * from Win32_ComputerSystem");
                foreach (ManagementObject obj in ramSearcher.Get())
                    Console.WriteLine($"RAM: {Math.Round(Convert.ToDouble(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024), 2)} GB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сборе информации: {ex.Message}");
            }
        }

        static void ApplySettings(string path, string resolution, string quality, string rayTracing, string dlss)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Файл конфигурации не найден: {path}");
                return;
            }

            // Читаем файл, заменяем строки и сохраняем
            // ВАЖНО: Названия параметров (Resolution, Quality и т.д.) зависят от игры.
            // Вам нужно открыть файл .ini и посмотреть, как называются нужные параметры.
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("Resolution="))
                    lines[i] = $"Resolution={resolution}";
                if (lines[i].StartsWith("GraphicsQuality="))
                    lines[i] = $"GraphicsQuality={quality}";
                if (lines[i].StartsWith("RayTracing="))
                    lines[i] = $"RayTracing={rayTracing}";
                if (lines[i].StartsWith("DLSS="))
                    lines[i] = $"DLSS={dlss}";
            }
            File.WriteAllLines(path, lines);
            Console.WriteLine("Настройки успешно применены.");
        }

        static void RunBenchmark(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Исполняемый файл не найден: {path}");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit(); // Ждем завершения бенчмарка
            }
            Console.WriteLine("Бенчмарк завершен.");
        }

        static string ParseResults(string path)
        {
            if (!File.Exists(path))
            {
                return "Файл результатов не найден.";
            }

            // Простой парсер: ищем строку с FPS
            // Если результаты в JSON, используйте System.Text.Json
            string content = File.ReadAllText(path);
            Match match = Regex.Match(content, @"Average FPS:\s*(\d+)");
            if (match.Success)
            {
                return $"Средний FPS: {match.Groups[1].Value}";
            }
            return "Не удалось распарсить результат.";
        }
    }
}
