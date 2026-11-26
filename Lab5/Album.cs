namespace Lab5
{
    public class Album
    {
        private readonly int _id;
        private readonly string _title;
        private readonly int _artistId;

        public Album(int id, string title, int artistId)
        {
            _id = id;
            _title = title;
            _artistId = artistId;
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }
        }

        public int ArtistId
        {
            get
            {
                return _artistId;
            }
        }

        public override string ToString()
        {
            return $"{_id}. {_title} (исполнитель {_artistId})";
        }
    }
}
