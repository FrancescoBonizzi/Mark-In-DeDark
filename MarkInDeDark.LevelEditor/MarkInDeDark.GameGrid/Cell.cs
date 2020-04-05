namespace MarkInDeDark.GameGrid
{
    public class Cell
    {
        public GroundTypes GroundType { get; set; }
        public ObjectTypes? ObjectType { get; set; }

        /// <summary>
        /// Può essere il numero (es candele) ma anche un'indicazione del path (Es per i nemici)
        /// </summary>
        public string ObjectTypeAdditionalData { get; set; }
        public WallTypes Top { get; set; }
        public WallTypes Right { get; set; }
        public WallTypes Bottom { get; set; }
        public WallTypes Left { get; set; }

        public string StringCode { get; set; }
    }
}
