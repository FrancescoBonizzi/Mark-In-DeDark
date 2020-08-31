namespace MarkInDeDark.GameGridCommon
{
    public static class GridObjectsHelpers
    {
        public static bool IsRiempimento(GroundTypes groundType)
        {
            return groundType == GroundTypes.Riempimento1
                 || groundType == GroundTypes.Riempimento2
                 || groundType == GroundTypes.Riempimento3
                 || groundType == GroundTypes.Riempimento4
                 || groundType == GroundTypes.Riempimento5
                 || groundType == GroundTypes.Riempimento6
                 || groundType == GroundTypes.Riempimento7
                 || groundType == GroundTypes.Riempimento8
                 || groundType == GroundTypes.Riempimento9
                 || groundType == GroundTypes.Riempimento10
                 || groundType == GroundTypes.Riempimento11
                 || groundType == GroundTypes.Riempimento12
                 || groundType == GroundTypes.Riempimento13;
        }
    }
}
