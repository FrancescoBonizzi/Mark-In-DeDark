namespace MarkInDeDark.LevelEditor.Domain
{
    public class Cell
    {
        public GroundTypes GroundType { get; set; }
        public WallTypes Top { get; set; }
        public WallTypes Right { get; set; }
        public WallTypes Bottom { get; set; }
        public WallTypes Left { get; set; }
    }
}
