using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace POEMes
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (StreamWriter sw = new StreamWriter("Settings/Statistics.set"))
            {
                foreach (CheckBox item in groupBox1.Controls)
                {
                    sw.WriteLine($"{item.Name.Replace("checkBox", "").Replace("*", "")} {item.Checked}");
                }
            }
            Dispose();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("Settings"))
            {
                Directory.CreateDirectory("Settings");
                File.Create("Settings/Statistics.set").Dispose();
            }
            else
            {
                int count = groupBox1.Controls.Count;
                using (StreamReader sr = new StreamReader("Settings/Statistics.set"))// 2  1  0
                {
                    while (!sr.EndOfStream)
                    {
                        if (sr.ReadLine() == count + " True")
                        {
                            foreach (CheckBox item in groupBox1.Controls)
                            {
                                if ("checkBox" + count == item.Name)
                                    item.Checked = true;
                            }
                        }
                        count--;
                    }
                }
            }
        }
    }
}
