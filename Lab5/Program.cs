using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Lab5
{
    internal static class Program
    {
        private const string DefaultPath = "LR5-var15.xlsx";
        private const int PageSize = 15;
        private const string Menu = """
            Лабораторная работа №5. Вариант 15. Аудиотека
            1 - загрузить базу из Excel
            2 - просмотр таблицы
            3 - удалить элемент по ID
            4 - добавить элемент
            5 - запросы
            6 - сохранить в Excel
            0 - выход
            """;
        private const string TableMenu = "Таблица: 1 - Артисты, 2 - Альбомы, 3 - Треки, 4 - Жанры";
        private const string QueryMenu = """
            1 - треки длиннее заданного числа минут (Треки)
            2 - средняя стоимость трека в жанре (Треки, Жанры)
            3 - альбомы, в которых есть треки жанра (Треки, Жанры, Альбомы)
            4 - общая длительность треков исполнителя (Артисты, Альбомы, Треки)
            """;

        private static readonly AudioLibrary s_library;
        private static string s_filePath;

        static Program()
        {
            s_library = new AudioLibrary();
            s_filePath = DefaultPath;
        }

        private static void Main()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            int choice;
            while (true)
            {
                Console.WriteLine(Menu);
                choice = ConsoleInput.ReadInt32("Пункт: ", 0, 6);
                if (choice == 0)
                {
                    return;
                }

                RunItem(choice);
            }
        }

        private static void RunItem(int choice)
        {
            Console.WriteLine();
            if (choice != 1 && s_library.IsEmpty)
            {
                Console.WriteLine("База пустая. Сначала загрузите её (пункт 1).");
                Console.WriteLine();
                return;
            }

            try
            {
                switch (choice)
                {
                    case 1:
                        LoadDatabase();
                        break;
                    case 2:
                        ShowTable();
                        break;
                    case 3:
                        DeleteItem();
                        break;
                    case 4:
                        AddItem();
                        break;
                    case 5:
                        RunQuery();
                        break;
                    case 6:
                        SaveDatabase();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(choice));
                }
            }
            catch (KeyNotFoundException exception)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
            catch (InvalidDataException exception)
            {
                Console.WriteLine($"Ошибка в файле: {exception.Message}");
            }
            catch (FileNotFoundException exception)
            {
                Console.WriteLine($"Ошибка: файл {Path.GetFileName(exception.FileName)} не найден.");
            }
            catch (IOException exception)
            {
                Console.WriteLine($"Ошибка при работе с файлом: {exception.Message}");
            }

            Console.WriteLine();
        }

        private static void LoadDatabase()
        {
            var path = ConsoleInput.ReadTextOrDefault($"Файл (Enter - {DefaultPath}): ", DefaultPath);
            s_library.Load(path);
            s_filePath = path;
            Console.WriteLine($"Загружено: артистов {s_library.GetRowCount(Table.Artists)}, "
                + $"альбомов {s_library.GetRowCount(Table.Albums)}, треков {s_library.GetRowCount(Table.Tracks)}, "
                + $"жанров {s_library.GetRowCount(Table.Genres)}.");
        }

        private static void ShowTable()
        {
            var table = ReadTable();
            var rowCount = s_library.GetRowCount(table);
            if (rowCount == 0)
            {
                Console.WriteLine("Таблица пустая.");
                return;
            }

            var pageCount = (rowCount + PageSize - 1) / PageSize;
            var page = ConsoleInput.ReadInt32($"Записей {rowCount}, страниц {pageCount}. Страница: ", 1, pageCount);
            foreach (var line in s_library.GetPage(table, page, PageSize))
            {
                Console.WriteLine(line);
            }
        }

        private static void DeleteItem()
        {
            var table = ReadTable();
            var id = ConsoleInput.ReadInt32("ID: ", 1, int.MaxValue);
            s_library.Delete(table, id);
            Console.WriteLine($"Запись с ID {id} удалена.");
        }

        private static void AddItem()
        {
            var table = ReadTable();
            switch (table)
            {
                case Table.Artists:
                    Console.WriteLine($"Добавлен: {s_library.AddArtist(ConsoleInput.ReadText("Имя: "))}");
                    break;
                case Table.Genres:
                    Console.WriteLine($"Добавлен: {s_library.AddGenre(ConsoleInput.ReadText("Название: "))}");
                    break;
                case Table.Albums:
                    var title = ConsoleInput.ReadText("Название: ");
                    var artistId = ConsoleInput.ReadInt32("ID исполнителя: ", 1, int.MaxValue);
                    Console.WriteLine($"Добавлен: {s_library.AddAlbum(title, artistId)}");
                    break;
                case Table.Tracks:
                    var name = ConsoleInput.ReadText("Название трека: ");
                    var albumId = ConsoleInput.ReadInt32("ID альбома: ", 1, int.MaxValue);
                    var genreId = ConsoleInput.ReadInt32("ID жанра: ", 1, int.MaxValue);
                    var milliseconds = ConsoleInput.ReadInt32("Длительность, мс: ", 1, int.MaxValue);
                    var bytes = ConsoleInput.ReadInt32("Размер, байт: ", 1, int.MaxValue);
                    var price = ConsoleInput.ReadInt32("Стоимость, руб.: ", 0, int.MaxValue);
                    var track = s_library.AddTrack(name, albumId, genreId, milliseconds, bytes, price);
                    Console.WriteLine($"Добавлен: {track}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(table));
            }
        }

        private static void RunQuery()
        {
            Console.WriteLine(QueryMenu);
            var query = ConsoleInput.ReadInt32("Запрос: ", 1, 4);
            switch (query)
            {
                case 1:
                    var minutes = ConsoleInput.ReadInt32("Минут: ", 0, 100);
                    var tracks = s_library.FindTracksLongerThan(minutes);
                    Console.WriteLine($"Треков длиннее {minutes} мин.: {tracks.Count}");
                    foreach (var track in tracks)
                    {
                        Console.WriteLine(track);
                    }

                    break;
                case 2:
                    var genre = ConsoleInput.ReadText("Жанр: ");
                    var average = s_library.GetAveragePriceInGenre(genre);
                    Console.WriteLine($"Средняя стоимость трека в жанре {genre}: {FormatNumber(average)} руб.");
                    break;
                case 3:
                    var albumGenre = ConsoleInput.ReadText("Жанр: ");
                    var albums = s_library.FindAlbumsWithGenre(albumGenre);
                    Console.WriteLine($"Альбомов с треками жанра {albumGenre}: {albums.Count}");
                    foreach (var album in albums)
                    {
                        Console.WriteLine(album);
                    }

                    break;
                case 4:
                    var artist = ConsoleInput.ReadText("Исполнитель: ");
                    var totalMinutes = s_library.GetArtistTotalMinutes(artist);
                    Console.WriteLine($"Общая длительность треков {artist}: {FormatNumber(totalMinutes)} мин.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(query));
            }
        }

        private static void SaveDatabase()
        {
            var path = ConsoleInput.ReadTextOrDefault($"Файл (Enter - {s_filePath}): ", s_filePath);
            s_library.Save(path);
            Console.WriteLine($"База сохранена в {path}.");
        }

        private static Table ReadTable()
        {
            Console.WriteLine(TableMenu);
            return (Table)ConsoleInput.ReadInt32("Номер таблицы: ", 1, 4);
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
