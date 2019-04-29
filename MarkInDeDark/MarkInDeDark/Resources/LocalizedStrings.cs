using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkInDeDark.Resources
{
    public class LocalizedStrings
    {
        private static Traduzioni _localizedResources = new Traduzioni();

        public Traduzioni T { get { return _localizedResources; } }
    }
}
