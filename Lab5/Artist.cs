namespace Lab5
{
    public class Artist
    {
        private readonly int _id;
        private readonly string _name;

        public Artist(int id, string name)
        {
            _id = id;
            _name = name;
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

        public override string ToString()
        {
            return $"{_id}. {_name}";
        }
    }
}
