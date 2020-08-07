using Npgsql;
using System;
using System.Windows.Forms;

namespace RetrieverHelperApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
			// assign vars with input from textboxes
			label7.Text = "";
            string ipAddress = txtIP.Text;
			string databaseName = txtDbName.Text;

			
			if (databaseName == "")
			{
				label7.Text = "Database field is empty. Please add a database name.";
				return;
			}
			if (!int.TryParse(txtMachine.Text, out int machineNumber))
			{
				label7.Text = "Machine Number is invalid. Please correct the Machine #.";
				return;
			}

			//open connection

			using (NpgsqlConnection conn = new NpgsqlConnection("Host=" + ipAddress + "; User Id=postgres; Password=Harvard%2525; Database=" + databaseName))
			{
				try
				{
					conn.Open();
				}
				catch (NpgsqlException ex2)
				{
					MessageBox.Show($"SqlException: {ex2.Message}");
					return;
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Exception: {ex.Message}");
					return;
				}

				//find amount of rows

				NpgsqlCommand command1 = new NpgsqlCommand("SELECT count(*) FROM zvenuecomputer", conn);
				int rowCount = Convert.ToInt32(command1.ExecuteScalar());

				// check for valid machine #

				if (machineNumber == 1)
				{
					MessageBox.Show("Machine #1 doesn't exist!\nHit back on the browser and try a different machine #.");
					return;
				}
				if (machineNumber == rowCount)
				{
					MessageBox.Show("You can just delete that machine in Process, silly!\nTry again with a machine #\nthat you can't delete in Process.");
					return;
				}
				//


				//Delete machine number and move all below it up

				string sqlCommand = "DELETE FROM zvenuecomputer WHERE computer =" + machineNumber + ";DELETE FROM zvenuedevice WHERE machine =" + machineNumber + ";";
				for (int x = machineNumber + 1; x <= rowCount; x++)
				{
					sqlCommand = sqlCommand + "UPDATE zvenuecomputer SET computer = " + (x - 1) + " WHERE computer= " + x + ";UPDATE zvenuedevice SET machine = " + (x - 1) + " WHERE machine =" + x + ";";
				}
				NpgsqlCommand cmd = new NpgsqlCommand(sqlCommand, conn);
				cmd.ExecuteNonQuery();

				//Sucess!
                label7.Text = "Machine # " + machineNumber + " removed!  Please restart Process to see the changes.";
            }
		}
	}
 }  




