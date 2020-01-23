using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POEMes
{
    class Setting
    {
        public static string ComboBox { get; set; }
        readonly private string pathSource = "Source/SourceText.txt";
        public void AddSourceText()
        {
            using (StreamWriter sw = File.AppendText(pathSource))
            {
                sw.WriteLine(ComboBox);
            }
        }
        public void Function2()
        {
            
        }

    }
}
