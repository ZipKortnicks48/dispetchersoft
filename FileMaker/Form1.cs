using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using FinMonMvkLoader;
using System.Threading;
using System.Threading.Tasks;
using IniFiles;



namespace FileMaker
{
    public partial class Form1 : Form
    {
        ReportItem label = new ReportItem();
        string pathFolder = /*"192.168.28.12/Exchange/TestErrorFolder/"*/"";
        public Form1()
        {
            string path = "set.ini";
            IniFile INI = new IniFile(path);
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line = sr.ReadToEnd();
                }
                
                if (INI.KeyExists("path", "set"))
                    pathFolder = INI.ReadINI("set", "path");
                if (INI.KeyExists("distinct", "set"))
                    label.distinct = INI.ReadINI("set", "distinct");

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message+"   Он будет создан автоматически. Для изменения настроек по умолчанию обратитесь в поддержку");

                //создадим файл
                INI.Write("set", "path", "192.168.28.12/Exchange/TestErrorFolder/");
                INI.Write("set", "distinct", "Липецкий");
                MessageBox.Show("Настройки установлены");
                label.distinct = "Липецкий";
            }
            InitializeComponent();
            comboBox1.Text = label.distinct;
            dateTimePicker1.Value = label.date;
            textBox1.Text = label.reason;
            textBox2.Text = label.time;
            textBox3.Text = label.country;
            label8.Text = "";
        }
        private void Form1_Load(object sender, EventArgs e)
        {   
            comboBox1.SelectedItem = label.distinct;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            label8.Text = "Загрузка файла. Ожидайте.";
            publicFile();
        }
        private void button3_Click(object sender, EventArgs e)
        {
           
            label.distinct = comboBox1.SelectedItem.ToString();
            label.reason = textBox1.Text;
            label.time = textBox2.Text;
            label.country = textBox3.Text;
            label.date = dateTimePicker1.Value.Date;
            foreach(DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.IsNewRow) break;
                AdressItem addr = new AdressItem(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString());
                label.adresses.Add(addr);
            }
            label.addToReport(dataGridView1);
            label = new ReportItem();
            dataGridView2.Rows.Clear();
        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            label.date = dateTimePicker1.Value.Date;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            deletRow(dataGridView2);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            deletRow(dataGridView1);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            dataGridView2.Rows.Clear();
        }
        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Set s = new Set();
            s.Show();
        }
        private void publicFile()
        {
            CSVWritter cw = new CSVWritter(dataGridView1.Rows);
            cw.GenerateCSVFile(button1,label8,dataGridView1, comboBox1.SelectedItem.ToString(), pathFolder);
        }
        void deletRow(DataGridView dgv)
        {
            int delet = dgv.SelectedCells[0].RowIndex;
            if (!dgv.Rows[delet].IsNewRow) dgv.Rows.RemoveAt(delet);
        }
        private void экспортToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog()==DialogResult.Cancel)
            {
                return;
            }
            string filename = saveFileDialog1.FileName;
            CSVWritter cw = new CSVWritter(dataGridView1.Rows);
            cw.saveAs(filename,comboBox1.SelectedItem.ToString());
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpForm f = new HelpForm();
            f.Show();
        }
    }
    class AdressItem
    {
        public string homes;
        public string street;
        public AdressItem(string _street, string _homes)
        {
            street = GetStreetRightWriting(_street);
            homes = GetHomesRightWriting(_homes);
        }
        private string GetStreetRightWriting(string _street)
        {
            string str = _street;
            string substring = "ул.";
            if(str.IndexOf(substring)==0)
            {
                return str;
            }
            else
            {
                str = "ул. " + str;
                return str;
            }
        }

        private string GetHomesRightWriting(string _homes)
        {
            string str = _homes;
            string substring = "д.";
            if (str.IndexOf(substring) == 0)
            {
                return str;
            }
            else
            {
                str = "д. " + str;
                return str;
            }
        }
    }
    class ReportItem
    {
        public string distinct="";
        public DateTime date=DateTime.Today.Date;
        public string reason="Порыв водопровода";
        public string time="c 8.00 до 17.00";
        public List<AdressItem> adresses;
        public string country = "";
        public ReportItem()
        {
            adresses = new List<AdressItem>();
        }
        public void addToReport(DataGridView dgv)
        {
            if(this.isEmpty())
            {
                MessageBox.Show("Заполните все поля.");
                return;  
            }

            string addrString="";
            for(int i=0;i< adresses.Count;i++)
            {
                addrString += adresses[i].street + ", " + adresses[i].homes;
                if (i != adresses.Count - 1) addrString += "; ";
            }
            dgv.Rows.Add(country,addrString,time,reason,date.ToString("d"));
        }
        private bool isEmpty()
        {
            if (country==""||time == "" || reason == "" || date == null || adresses.Count == 0)
                return true;
            return false;
        }
    }
    class CSVWritter
    {
        DataGridViewRowCollection rows;
        public CSVWritter(DataGridViewRowCollection _rows)
        {
            rows = _rows;
        }
        public void saveAs(string pas,string distinct)
        {
            string header = "Район;Дата;Адрес;Причина;Время";
            try
            {
                FileStream fs = new FileStream(pas, FileMode.OpenOrCreate);
                StreamWriter w = new StreamWriter(fs, Encoding.UTF8);
                w.WriteLine(header);
                foreach (DataGridViewRow row in rows)
                {
                    if (row.IsNewRow) break;
                    string s = distinct + ";" + row.Cells[4].FormattedValue.ToString() + ";" + row.Cells[0].FormattedValue.ToString() + ":&" + row.Cells[1].FormattedValue.ToString().Replace(";", "&") + ";" + row.Cells[3].FormattedValue.ToString() + ";" + row.Cells[2].FormattedValue.ToString();
                    w.WriteLine(s);
                }
                w.Close();
                MessageBox.Show("Файл загружен");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool asyncWriting(string p,string distinct)
        {
            string header = "Район;Дата;Адрес;Причина;Время";
            try
            {
                WindowsIdentityEx newId = new WindowsIdentityEx("SiroklasovA", "rkvv", "2");
                WindowsImpersonationContext impersonatedUser = newId.Impersonate();
                FileStream fs = new FileStream(@"//" + p + distinct + " журнал аварийных отключений.csv", FileMode.OpenOrCreate);
                StreamWriter w = new StreamWriter(fs, Encoding.UTF8);
                w.WriteLine(header);
                foreach (DataGridViewRow row in rows)
                {
                    if (row.IsNewRow) break;
                    string s = distinct + ";" + row.Cells[4].FormattedValue.ToString() + ";" + row.Cells[0].FormattedValue.ToString() + ":&" + row.Cells[1].FormattedValue.ToString().Replace(";", "&") + ";" + row.Cells[3].FormattedValue.ToString() + ";" + row.Cells[2].FormattedValue.ToString();
                    w.WriteLine(s);
                }
                w.Close();
                impersonatedUser.Undo();
                MessageBox.Show("Файл загружен");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        public async void GenerateCSVFile(Button btn, Label l,DataGridView dgv, string distinct, string p)
        {
            if (dgv.Rows.Count == 1) MessageBox.Show("Журнал не заполнен");

            dgv.Sort(dgv.Columns[4], 0);
            DataGridViewRowCollection rows = dgv.Rows;
            await Task.Run(() => { asyncWriting( p, distinct); });
            btn.Enabled = true;
            l.Text = "";
        }
    }
}
