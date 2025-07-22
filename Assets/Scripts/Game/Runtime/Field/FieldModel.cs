using Game.Entities;

namespace Game.Field
{
    public class FieldModel
    {
        public EntityModel[,] Entities;

        public FieldModel(int rows, int columns)
        {
            Entities = new EntityModel[rows, columns];
            this.FillWithEmpty();
        }
        
        public void SetValue(int row, int column, EntityModel value)
        {
            Entities[row, column] = value;
        }
    }

    public static class FieldExtension
    {
        public static void FillWithEmpty(this FieldModel field)
        {
            for (int row = 0; row < field.Entities.GetLength(0); row++)
            {
                for (int column = 0; column < field.Entities.GetLength(1); column++)
                {
                    field.Entities[row, column] = EntityModel.Empty;
                }
            }
        }
    }
}