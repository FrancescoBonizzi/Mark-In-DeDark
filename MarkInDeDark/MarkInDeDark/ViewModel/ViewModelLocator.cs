using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkInDeDark.ViewModel
{
    public class ViewModelLocator
    {
        public static MainViewModel MainViewModelIstance { get; set; }

        public MainViewModel Main
        {
            get { return MainViewModelIstance; }
        }
    }
}
