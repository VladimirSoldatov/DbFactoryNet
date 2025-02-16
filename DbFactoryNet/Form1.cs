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
        string connectionString = String.Empty;
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
        }

        private async void button1_Click_GetDbFactories(object sender, EventArgs e)
        {
            dbConnection.ConnectionString = connectionString;
            await dbConnection.OpenAsync();
            DbCommand comm = dbConnection.CreateCommand();
            comm.CommandText = "WAITFOR DELAY '00:00:05';";
            comm.CommandText += textBox1.Text.ToString();
            DataTable table = new DataTable();
            using (DbDataReader reader = await comm.ExecuteReaderAsync())
            {
                int line = 0;
                do
                {
                    while (await reader.ReadAsync())
                    {
                        if (line == 0)
                        {
                            for (int i = 0; i <
                            reader.FieldCount; i++)
                            {
                                table.Columns.Add(reader.GetName(i));
                            }
                            line++;
                        }
                        DataRow row = table.NewRow();
                        for (int i = 0; i < reader.
                        FieldCount; i++)
                        {
                            row[i] = await reader.GetFieldValueAsync<Object>(i);
                        }
                        table.Rows.Add(row);
                    }
                } while (reader.NextResult());
            }
            //выводим результаты запроса
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = table;
            dbConnection.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dbProviderFactory = DbProviderFactories.GetFactory($"{comboBox1.SelectedItem}");
            dbConnection = dbProviderFactory.CreateConnection();
            try
            {
                foreach (ConnectionStringSettings conn in ConfigurationManager.ConnectionStrings)
                {
                    if (conn.ProviderName == $"{comboBox1.SelectedItem}")
                    {
                        providerName = conn.ConnectionString;
                        textBox1.Text = providerName;
                    }
                }
            }
            catch (Exception ex)
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
                MessageBox.Show(ex.Message);
                dbTransaction.Rollback();
            }
            finally
            {
                dbConnection.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            const string AsyncEnable = "Asynchronous Procesing=true;";
            string connectionString = textBox1.Text;
            if (!connectionString.Contains(AsyncEnable))
            {
                connectionString += AsyncEnable;
            }
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            string commandText = "DELAY FOR '00:00:05'; ";
            commandText += "SELECT * FROM Authors;";
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.CommandTimeout = 30;
            try
            {
                sqlConnection.Open();
                AsyncCallback asyncCallback = new AsyncCallback(GetDataCallback);
                sqlCommand.BeginExecuteReader(asyncCallback, sqlCommand);
                MessageBox.Show("Added thread is working...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }
        }
        DataTable table;
        private void GetDataCallback(IAsyncResult result)
        {
            SqlDataReader reader = null;
            try
            {
                /// блок 1
                SqlCommand command = (SqlCommand)result.AsyncState;
                ///
                /// блок 2
                reader = command.EndExecuteReader(result);
                ///
                table = new DataTable();
                int line = 0;
                do
                {
                    while (reader.Read())
                    {
                        if (line == 0)
                        {
                            for (int i = 0; i <
                            reader.FieldCount; i++)
                            {
                                table.Columns.Add(reader.GetName(i));
                            }
                            line++;
                        }
                        DataRow row = table.NewRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader[i];
                        }

                        table.Rows.Add(row);
                    }
                } while (reader.NextResult());
                DgvAction();
            }
            catch (Exception ex)
            {
                MessageBox.Show("From Callback 1:" + ex.Message);
            }
            finally
            {
                try
                {
                    if (!reader.IsClosed)
                    {
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("From Callback 2:" +
                    ex.Message);
                }
            }
        }
        private void DgvAction()
        {
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(DgvAction));
                return;
            }
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = table;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            dbConnection = dbProviderFactory.CreateConnection();
            connectionString = GetConnectionStringByProvider("System.Data.SqlClient");
            if (connectionString == null)
            {
                MessageBox.Show("В конфигурационном" +
                    " файле нет требуемой" +
                    " строки подключения");
            }
        }

        static string GetConnectionStringByProvider(string providerName)
        {
            string returnValue = null;
            //читаем все строки подключения из App.config
            ConnectionStringSettingsCollection
            settings = ConfigurationManager.
            ConnectionStrings;
            //ищем и возвращаем строку подключения
            //для providerName
            

        if (settings != null)
            {
                foreach (ConnectionStringSettings cs
                in settings)
                {
                    if (cs.ProviderName == providerName)
                    {
                        returnValue = cs.ConnectionString;
                        break;
                    }
                }
            }
            return returnValue;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show(DateTime.Now.ToShortDateString());
        }

        ///<summary>
        ///управление доступностью кнопки
        ///</summary>
        ///<param name="sender"></param>
        ///<param name="e"></param>

    }
}
