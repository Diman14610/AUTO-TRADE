using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POEMes
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        // Последовательность код на низкоуровневом языке
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        // Эмуляция клавиш уже с зажатым Ctrl
        private static void SendCtrlhotKey(char key)
        {
            keybd_event(0x11, 0, 0, 0);
            keybd_event((byte)key, 0, 0, 0);
            keybd_event((byte)key, 0, 0x2, 0);
            keybd_event(0x11, 0, 0x2, 0);
        }

        private IWebDriver driver = new ChromeDriver();
        private string buf;
        int countStep, countMes;
        readonly string pathSource = "Source/SourceText.txt", pathSet = "Settings/Statistics.set";
        private delegate void CallDel();
        private int countlot = 0;
        private CallDel call;

        private void LoadSource()
        {
            AutoCompleteStringCollection auto = new AutoCompleteStringCollection();
            using (StreamReader streamReader = new StreamReader(pathSource))
            {
                while (!streamReader.EndOfStream)
                    auto.Add(streamReader.ReadLine());
            }
            comboBox1.AutoCompleteCustomSource = auto;
            foreach (var item in auto)
                comboBox1.Items.Add(item);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Left = Screen.PrimaryScreen.Bounds.Width - Width;
            Top = Screen.PrimaryScreen.Bounds.Height - Height - 50;
            if (!Directory.Exists("Source"))
            {
                Directory.CreateDirectory("Source");
                File.Create(pathSource).Dispose();
            }
            else
            {
                Setting setting = new Setting();
                using (StreamReader sr = new StreamReader(pathSet))
                {
                    while (!sr.EndOfStream)
                    {
                        switch (sr.ReadLine())
                        {
                            case "1 True":
                                call += setting.AddSourceText;
                                File.WriteAllLines(pathSource, File.ReadAllLines(pathSource).Distinct());
                                break;
                        }
                    }
                }
            }
            LoadSource();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            if (string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                comboBox1.Focus();
                return;
            }
            // New function
            driver.Url = @"https://ru.pathofexile.com/trade/search/Metamorph";
            Text = driver.Title;
            Thread.Sleep(3000);
            try
            {
                driver.FindElement(By.XPath(@".//div[@class='multiselect__tags']/input")).SendKeys(comboBox1.Text);// ищет input и пишет туда текст
            }
            catch
            {
                MessageBox.Show("Сайт недоступен", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var links = driver.FindElements(By.XPath(@".//span[@class='']"));// находит все лоты в выпадающем списке
            int pos = 0;

            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Text.ToLower() == comboBox1.Text.ToLower())
                    pos = i;
            }

            pos += 2;// т.к. валюта и отсчет идет с 0 в циклах
            // если будет неправильное наименование лота
            try
            {
                driver.FindElement(By.XPath(@".//ul[@class='multiselect__content']/li[" + pos + "]")).Click();// 6 position - exa orb
                driver.FindElement(By.XPath(@".//div[@class='controls-center']/button")).Click();

                if (call != null)
                {
                    Setting.ComboBox = comboBox1.Text;
                    call();
                    updateLotToolStripMenuItem.Enabled = true;
                    updateLotToolStripMenuItem.Text = $"Обновить список ({++countlot})";
                }

                //colCom.Add(comboBox1.Text);
                //comboBox1.Items.Add(comboBox1.Text);

                timer1.Interval = Convert.ToInt32(numericUpDown1.Value);
                timer1.Start();
            }
            catch
            {
                MessageBox.Show("Не найдено", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboBox1.Text = "";
                comboBox1.Focus();
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items.Clear();
                Thread.Sleep(1000);

                var links = driver.FindElements(By.XPath(@".//div[@class='price']//span"));//profile-link
                var account = driver.FindElements(By.XPath(@".//span[@class='profile-link']//a"));//var account = driver.FindElements(By.XPath(@".//span[@class='pull-left']//button"));

                for (int i = 0; i < links.Count; i += 6)
                    listBox1.Items.Add(links[i].Text);
                // Если лот будет выкуплен раньше чем напишет программа, то вылетит ошибка, try\catch чинит эту проблему
                try
                {
                    if (checkBox1.Checked == true)
                    {
                        var searchPrice = Regex.Replace(links[0].Text, @"\D", "");

                        if (buf != account[0].Text)
                        {
                            if (Convert.ToInt32(searchPrice) >= Convert.ToInt32(numericUpDown2.Value) & Convert.ToInt32(searchPrice) <= Convert.ToInt32(numericUpDown3.Value))
                            {
                                // SendMessage();
                                // Get a handle to the Calculator application. The window class
                                // and window name were obtained using the Spy++ tool.
                                // POEWindowClass Path of Exile
                                IntPtr POErHandle = FindWindow("POEWindowClass", "Path of Exile");
                                // Verify that Calculator is a running process.
                                if (POErHandle == IntPtr.Zero)
                                {
                                    MessageBox.Show("Path of Exile не запущен", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                // Make Calculator the foreground application and send it 
                                // a set of calculations.                     
                                driver.FindElement(By.XPath(@".//span[@class='pull-left']//button")).Click();
                                SetForegroundWindow(POErHandle);
                                SendKeys.SendWait("{Enter}");
                                //SendKeys.SendWait("^{v}");
                                SendCtrlhotKey('V');
                                SendKeys.SendWait("{Enter}");
                                buf = account[0].Text;
                                label5.Text = "Сообщений " + ++countMes;
                            }
                        }
                    }
                }
                catch { }

                label4.Text = "Обнов. " + ++countStep;
                driver.FindElement(By.XPath(@".//div[@class='controls-center']/button")).Click();
            }
            catch (Exception)
            {
                timer1.Stop();
                button1.Visible = false;
                MessageBox.Show(" Вы закрыли браузер!!!\n Перезапустите программу!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
           
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //driver.Dispose(); //setting
            driver.Quit();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                numericUpDown2.Visible = true;
                numericUpDown3.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
            }
            else
            {
                numericUpDown2.Visible = false;
                numericUpDown3.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
            }
        }

        private void updateLotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            File.WriteAllLines(pathSource, File.ReadAllLines(pathSource).Distinct());
            LoadSource();
            updateLotToolStripMenuItem.Text = "Обновить список";
            updateLotToolStripMenuItem.Enabled = false;
            countlot = 0;
        }

        private void writeToFileCountStepMesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }
    }
}
