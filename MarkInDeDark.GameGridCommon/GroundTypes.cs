using System;

namespace MarkInDeDark.GameGridCommon
{
    public enum GroundTypes : int
    {
        NonCella = 0,
        Terra = 1,

        [Obsolete]
        Macerie = 2,

        TrappolaMorteIstantanea = 3,
        TrappolaTemporizzata = 4,
        TriggerGoblin = 5,
        Terra2 = 6,
        Buio = 7
    }
}
