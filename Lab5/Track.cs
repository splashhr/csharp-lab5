using System.Globalization;

namespace Lab5
{
    public class Track
    {
        private const double BytesInMegabyte = 1024 * 1024;

        private readonly int _id;
        private readonly string _name;
        private readonly int _albumId;
        private readonly int _genreId;
        private readonly int _milliseconds;
        private readonly int _bytes;
        private readonly int _price;

        public Track(int id, string name, int albumId, int genreId, int milliseconds, int bytes, int price)
        {
            _id = id;
            _name = name;
            _albumId = albumId;
            _genreId = genreId;
            _milliseconds = milliseconds;
            _bytes = bytes;
            _price = price;
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public int AlbumId
        {
            get
            {
                return _albumId;
            }
        }

        public int GenreId
        {
            get
            {
                return _genreId;
            }
        }

        public int Milliseconds
        {
            get
            {
                return _milliseconds;
            }
        }

        public int Bytes
        {
            get
            {
                return _bytes;
            }
        }

        public int Price
        {
            get
            {
                return _price;
            }
        }

        public override string ToString()
        {
            var seconds = _milliseconds / 1000;
            var megabytes = (_bytes / BytesInMegabyte).ToString("0.0", CultureInfo.InvariantCulture);
            return $"{_id}. {_name} | альбом {_albumId} | жанр {_genreId} | {seconds / 60}:{seconds % 60:00} | "
                + $"{megabytes} МБ | {_price} руб.";
        }
    }
}
