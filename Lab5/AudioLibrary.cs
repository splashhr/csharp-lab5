using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace Lab5
{
    public class AudioLibrary
    {
        private const string ArtistsSheet = "Артисты";
        private const string AlbumsSheet = "Альбомы";
        private const string TracksSheet = "Треки";
        private const string GenresSheet = "Жанры";
        private const int MillisecondsInMinute = 60000;

        private static readonly string[] s_artistHeaders;
        private static readonly string[] s_albumHeaders;
        private static readonly string[] s_trackHeaders;
        private static readonly string[] s_genreHeaders;

        private readonly List<Artist> _artists;
        private readonly List<Album> _albums;
        private readonly List<Track> _tracks;
        private readonly List<Genre> _genres;

        static AudioLibrary()
        {
            s_artistHeaders = new string[] { "ID", "Имя" };
            s_albumHeaders = new string[] { "ID", "Название", "ID Исполнителя" };
            s_trackHeaders = new string[]
            {
                "ID", "Название трека", "ID альбома", "ID жанра", "Длительность", "Размер", "Стоимость",
            };
            s_genreHeaders = new string[] { "ID", "Название" };
        }

        public AudioLibrary()
        {
            _artists = new List<Artist>();
            _albums = new List<Album>();
            _tracks = new List<Track>();
            _genres = new List<Genre>();
        }

        public bool IsEmpty
        {
            get
            {
                return _artists.Count == 0 && _albums.Count == 0 && _tracks.Count == 0 && _genres.Count == 0;
            }
        }

        public void Load(string path)
        {
            if (!string.Equals(Path.GetExtension(path), ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Нужен файл Excel в формате .xlsx.");
            }

            using var workbook = OpenWorkbook(path);
            List<Artist> artists;
            List<Album> albums;
            List<Track> tracks;
            List<Genre> genres;
            try
            {
                artists = (from row in GetDataRows(workbook, ArtistsSheet)
                           select new Artist(row.Cell(1).GetValue<int>(), row.Cell(2).GetString())).ToList();
                albums = (from row in GetDataRows(workbook, AlbumsSheet)
                          select new Album(
                              row.Cell(1).GetValue<int>(),
                              row.Cell(2).GetString(),
                              row.Cell(3).GetValue<int>())).ToList();
                tracks = (from row in GetDataRows(workbook, TracksSheet)
                          select new Track(
                              row.Cell(1).GetValue<int>(),
                              row.Cell(2).GetString(),
                              row.Cell(3).GetValue<int>(),
                              row.Cell(4).GetValue<int>(),
                              row.Cell(5).GetValue<int>(),
                              row.Cell(6).GetValue<int>(),
                              row.Cell(7).GetValue<int>())).ToList();
                genres = (from row in GetDataRows(workbook, GenresSheet)
                          select new Genre(row.Cell(1).GetValue<int>(), row.Cell(2).GetString())).ToList();
            }
            catch (InvalidCastException exception)
            {
                throw new InvalidDataException("В файле есть ячейка, которая не является числом: " + exception.Message);
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException("В файле есть ячейка с неверным значением: " + exception.Message);
            }

            ReplaceAll(_artists, artists);
            ReplaceAll(_albums, albums);
            ReplaceAll(_tracks, tracks);
            ReplaceAll(_genres, genres);
        }

        public void Save(string path)
        {
            using var workbook = new XLWorkbook();

            var albums = workbook.Worksheets.Add(AlbumsSheet);
            WriteHeaders(albums, s_albumHeaders);
            var albumRow = 2;
            foreach (var album in from a in _albums orderby a.Id select a)
            {
                albums.Cell(albumRow, 1).Value = album.Id;
                albums.Cell(albumRow, 2).Value = album.Title;
                albums.Cell(albumRow, 3).Value = album.ArtistId;
                albumRow++;
            }

            var artists = workbook.Worksheets.Add(ArtistsSheet);
            WriteHeaders(artists, s_artistHeaders);
            var artistRow = 2;
            foreach (var artist in from a in _artists orderby a.Id select a)
            {
                artists.Cell(artistRow, 1).Value = artist.Id;
                artists.Cell(artistRow, 2).Value = artist.Name;
                artistRow++;
            }

            var tracks = workbook.Worksheets.Add(TracksSheet);
            WriteHeaders(tracks, s_trackHeaders);
            var trackRow = 2;
            foreach (var track in from t in _tracks orderby t.Id select t)
            {
                tracks.Cell(trackRow, 1).Value = track.Id;
                tracks.Cell(trackRow, 2).Value = track.Name;
                tracks.Cell(trackRow, 3).Value = track.AlbumId;
                tracks.Cell(trackRow, 4).Value = track.GenreId;
                tracks.Cell(trackRow, 5).Value = track.Milliseconds;
                tracks.Cell(trackRow, 6).Value = track.Bytes;
                tracks.Cell(trackRow, 7).Value = track.Price;
                trackRow++;
            }

            var genres = workbook.Worksheets.Add(GenresSheet);
            WriteHeaders(genres, s_genreHeaders);
            var genreRow = 2;
            foreach (var genre in from g in _genres orderby g.Id select g)
            {
                genres.Cell(genreRow, 1).Value = genre.Id;
                genres.Cell(genreRow, 2).Value = genre.Name;
                genreRow++;
            }

            workbook.SaveAs(path);
        }

        public int GetRowCount(Table table)
        {
            switch (table)
            {
                case Table.Artists:
                    return _artists.Count;
                case Table.Albums:
                    return _albums.Count;
                case Table.Tracks:
                    return _tracks.Count;
                case Table.Genres:
                    return _genres.Count;
                default:
                    throw new ArgumentOutOfRangeException(nameof(table));
            }
        }

        public List<string> GetPage(Table table, int page, int pageSize)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

            var skip = (page - 1) * pageSize;
            switch (table)
            {
                case Table.Artists:
                    return (from a in _artists orderby a.Id select a.ToString()).Skip(skip).Take(pageSize).ToList();
                case Table.Albums:
                    return (from a in _albums orderby a.Id select a.ToString()).Skip(skip).Take(pageSize).ToList();
                case Table.Tracks:
                    return (from t in _tracks orderby t.Id select t.ToString()).Skip(skip).Take(pageSize).ToList();
                case Table.Genres:
                    return (from g in _genres orderby g.Id select g.ToString()).Skip(skip).Take(pageSize).ToList();
                default:
                    throw new ArgumentOutOfRangeException(nameof(table));
            }
        }

        public void Delete(Table table, int id)
        {
            switch (table)
            {
                case Table.Artists:
                    DeleteArtist(id);
                    break;
                case Table.Albums:
                    DeleteAlbum(id);
                    break;
                case Table.Tracks:
                    _tracks.Remove(FindTrack(id));
                    break;
                case Table.Genres:
                    DeleteGenre(id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(table));
            }
        }

        public Artist AddArtist(string name)
        {
            var artist = new Artist(GetNextId(from a in _artists select a.Id), CheckName(name));
            _artists.Add(artist);
            return artist;
        }

        public Genre AddGenre(string name)
        {
            var genre = new Genre(GetNextId(from g in _genres select g.Id), CheckName(name));
            _genres.Add(genre);
            return genre;
        }

        public Album AddAlbum(string title, int artistId)
        {
            FindArtist(artistId);
            var album = new Album(GetNextId(from a in _albums select a.Id), CheckName(title), artistId);
            _albums.Add(album);
            return album;
        }

        public Track AddTrack(string name, int albumId, int genreId, int milliseconds, int bytes, int price)
        {
            FindAlbum(albumId);
            FindGenre(genreId);
            if (milliseconds <= 0 || bytes <= 0 || price < 0)
            {
                throw new ArgumentException("Длительность и размер должны быть больше нуля, стоимость - не меньше 0.");
            }

            var id = GetNextId(from t in _tracks select t.Id);
            var track = new Track(id, CheckName(name), albumId, genreId, milliseconds, bytes, price);
            _tracks.Add(track);
            return track;
        }

        public List<Track> FindTracksLongerThan(int minutes)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(minutes);

            var query = from t in _tracks
                        where t.Milliseconds > minutes * MillisecondsInMinute
                        orderby t.Milliseconds descending
                        select t;
            return query.ToList();
        }

        public double GetAveragePriceInGenre(string genreName)
        {
            FindGenreByName(genreName);
            var prices = from t in _tracks
                         join g in _genres on t.GenreId equals g.Id
                         where IsSameName(g.Name, genreName)
                         select t.Price;
            if (!prices.Any())
            {
                throw new InvalidOperationException("В этом жанре нет треков.");
            }

            return prices.Average();
        }

        public List<string> FindAlbumsWithGenre(string genreName)
        {
            FindGenreByName(genreName);
            var query = from t in _tracks
                        join g in _genres on t.GenreId equals g.Id
                        join a in _albums on t.AlbumId equals a.Id
                        where IsSameName(g.Name, genreName)
                        group t by a into albumTracks
                        orderby albumTracks.Count() descending, albumTracks.Key.Title
                        select $"{albumTracks.Key.Title} - треков: {albumTracks.Count()}";
            return query.ToList();
        }

        public double GetArtistTotalMinutes(string artistName)
        {
            var artists = from a in _artists where IsSameName(a.Name, artistName) select a;
            if (!artists.Any())
            {
                throw new KeyNotFoundException($"Исполнитель «{artistName}» не найден.");
            }

            var durations = from ar in _artists
                            join al in _albums on ar.Id equals al.ArtistId
                            join t in _tracks on al.Id equals t.AlbumId
                            where IsSameName(ar.Name, artistName)
                            select (long)t.Milliseconds;
            return (double)durations.Sum() / MillisecondsInMinute;
        }

        private static XLWorkbook OpenWorkbook(string path)
        {
            try
            {
                return new XLWorkbook(path);
            }
            catch (FileFormatException)
            {
                throw new InvalidDataException("Файл повреждён или не является книгой Excel.");
            }
        }

        private static IEnumerable<IXLRow> GetDataRows(XLWorkbook workbook, string sheetName)
        {
            if (!workbook.TryGetWorksheet(sheetName, out var sheet))
            {
                throw new InvalidDataException($"В файле нет листа «{sheetName}».");
            }

            return from row in sheet.RowsUsed() where row.RowNumber() > 1 select row;
        }

        private static void WriteHeaders(IXLWorksheet sheet, string[] headers)
        {
            for (var column = 0; column < headers.Length; column++)
            {
                sheet.Cell(1, column + 1).Value = headers[column];
            }
        }

        private static void ReplaceAll<T>(List<T> target, List<T> source)
        {
            target.Clear();
            target.AddRange(source);
        }

        private static int GetNextId(IEnumerable<int> ids)
        {
            if (!ids.Any())
            {
                return 1;
            }

            return ids.Max() + 1;
        }

        private static string CheckName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Название не должно быть пустым.");
            }

            return name.Trim();
        }

        private static bool IsSameName(string first, string second)
        {
            return string.Equals(first.Trim(), second.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private void DeleteArtist(int id)
        {
            var artist = FindArtist(id);
            var albumCount = (from a in _albums where a.ArtistId == id select a).Count();
            if (albumCount > 0)
            {
                throw new InvalidOperationException($"Нельзя удалить исполнителя: у него альбомов - {albumCount}.");
            }

            _artists.Remove(artist);
        }

        private void DeleteAlbum(int id)
        {
            var album = FindAlbum(id);
            var trackCount = (from t in _tracks where t.AlbumId == id select t).Count();
            if (trackCount > 0)
            {
                throw new InvalidOperationException($"Нельзя удалить альбом: в нём треков - {trackCount}.");
            }

            _albums.Remove(album);
        }

        private void DeleteGenre(int id)
        {
            var genre = FindGenre(id);
            var trackCount = (from t in _tracks where t.GenreId == id select t).Count();
            if (trackCount > 0)
            {
                throw new InvalidOperationException($"Нельзя удалить жанр: в нём треков - {trackCount}.");
            }

            _genres.Remove(genre);
        }

        private Artist FindArtist(int id)
        {
            var artist = (from a in _artists where a.Id == id select a).FirstOrDefault();
            return artist ?? throw new KeyNotFoundException($"Исполнителя с ID {id} нет.");
        }

        private Album FindAlbum(int id)
        {
            var album = (from a in _albums where a.Id == id select a).FirstOrDefault();
            return album ?? throw new KeyNotFoundException($"Альбома с ID {id} нет.");
        }

        private Track FindTrack(int id)
        {
            var track = (from t in _tracks where t.Id == id select t).FirstOrDefault();
            return track ?? throw new KeyNotFoundException($"Трека с ID {id} нет.");
        }

        private Genre FindGenre(int id)
        {
            var genre = (from g in _genres where g.Id == id select g).FirstOrDefault();
            return genre ?? throw new KeyNotFoundException($"Жанра с ID {id} нет.");
        }

        private Genre FindGenreByName(string name)
        {
            var genre = (from g in _genres where IsSameName(g.Name, name) select g).FirstOrDefault();
            return genre ?? throw new KeyNotFoundException($"Жанр «{name}» не найден.");
        }
    }
}
