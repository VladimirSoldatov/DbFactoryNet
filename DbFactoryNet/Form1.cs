using System;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.Win32.SafeHandles;
using System.Data.SqlClient;

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
            adapter.SelectCommand.CommandText = textBox2.Text.ToString();
            // выполняем запрос select из адаптера
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet, "Authors");
            // выводим результаты запроса

            DataViewManager dvm = new DataViewManager(dataSet);
            dvm.DataViewSettings["Authors"].Sort = "FirstName ASC";
            DataView dataView = dvm.CreateDataView(dataSet.Tables["Authors"]);
            dataGridView1.DataSource = dataView;

            dvm = new DataViewManager(dataSet);
            dvm.DataViewSettings["Authors"].RowFilter = "FirstName = 'Сергей'";          
            dataView = dvm.CreateDataView(dataSet.Tables["Authors"]);

            dataGridView2.DataSource = null;
            dataGridView2.DataSource = dataView;
            dataGridView2.AllowUserToAddRows = false;



        }

        private void button2_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

            if (textBox2.Text.Length > 5)
                button2.Enabled = true;
            else
                button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dbConnection.ConnectionString = textBox1.Text;
            DbTransaction dbTransaction = null;

            try
            {
                dbConnection.Open();
                // создаем адаптер из фабрики
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbTransaction = dbConnection.BeginTransaction();
                dbCommand.Transaction = dbTransaction;
                dbCommand.CommandText = textBox2.Text;
                dbCommand.ExecuteNonQuery();
                dbCommand.CommandText = "INSERT INTO Authors(id, FirstName, LastName) VALUES (7,'Кошкин', 'Дом')";
                dbCommand.ExecuteNonQuery();
                dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("!!!");
                dbTransaction.Rollback();
            }
            finally
            {
                dbConnection.Close();
            }
        }
    }
}
