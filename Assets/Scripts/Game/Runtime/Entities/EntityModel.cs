namespace Game.Entities
{
    public struct EntityModel
    {
        public static EntityModel Empty = new EntityModel(0);
        public int Value { get; private set; }

        public EntityModel(int value)
        {
            Value = value;
        }
    }
}