namespace RuTracker.Client.Model
{
    public sealed class User
    {
        public readonly int Id;
        public readonly string Name;

        public User(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => Name;
    }
}
