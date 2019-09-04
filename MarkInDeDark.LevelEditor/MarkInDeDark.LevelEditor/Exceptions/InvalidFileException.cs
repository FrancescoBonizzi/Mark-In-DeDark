using System;

namespace MarkInDeDark.LevelEditor.Exceptions
{
    public class InvalidFileException : Exception
    {
        public InvalidFileException(string reason)
            : base($"Il file selezionato non è valido: {reason}")
        {

        }
    }
}
