using System;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.Win32.SafeHandles;

namespace DbFactoryNet
{
    public partial class Form1 : Form
    {
        DbConnection dbConnection = null;
        DbProviderFactory dbProviderFactory = null;
        string providerName = String.Empty;
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
        }

        private void button1_Click_GetDbFactories(object sender, EventArgs e)
        {
            DataTable dt = DbProviderFactories.GetFactoryClasses();
            dataGridView1.DataSource = dt;
            comboBox1.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                comboBox1.Items.Add(row["InvariantName"]);
            }
            dataGridView1.AutoResizeColumn(0);
            dataGridView1.AutoResizeColumn(2);
            if(comboBox1.Items.Count>0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dbProviderFactory = DbProviderFactories.GetFactory($"{comboBox1.SelectedItem}");
            dbConnection = dbProviderFactory.CreateConnection();
            try
            {
                foreach(ConnectionStringSettings conn in ConfigurationManager.ConnectionStrings)
                {
                    if(conn.ProviderName == $"{comboBox1.SelectedItem}")
                    {
                        providerName = conn.ConnectionString;
                        textBox1.Text = providerName;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            dbConnection.ConnectionString = textBox1.Text;
            // создаем адаптер из фабрики
            DbDataAdapter adapter = dbProviderFactory.CreateDataAdapter();
            adapter.SelectCommand = dbConnection.CreateCommand();
            adapter.SelectCommand.CommandText = textBox2.
            Text.ToString();
            // выполняем запрос select из адаптера
            DataTable table = new DataTable();
            adapter.Fill(table);
            // выводим результаты запроса
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = table;
        }

        private void button2_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
        }
    }
}
