using Npgsql;
using Npgsql.PostgresTypes;
using System;
using System.Data.SqlClient;
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
            string ipAddress = txtIPrd.Text;
			string databaseName = txtDbNamerd.Text;

			
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
				
				//Check that we truely want to delete
				
				MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show("Are you sure you want to delete this Machine number?", "Are you sure?", buttons, MessageBoxIcon.Question);
				if (result==DialogResult.No)
                {
					return;
                }

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

		private void button2_Click(object sender, EventArgs e)
		{
			label14.Text = "";
			string ipAddress = txtIPrs.Text;
			string databaseName = txtDbNamers.Text;

			if (databaseName == "")
			{
				label14.Text = "Database field is empty. Please add a database name." + ipAddress;
				return;
			}
			if (!int.TryParse(txtFeatureId.Text, out int featureId))
			{
				label14.Text = "Feature ID is invalid. Please use a valid Feature ID.";
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
				string sql = "SELECT movieid, systemdate, starttime, roomid FROM schedule WHERE featureid=" + featureId + ";";

				NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, conn);
				
				NpgsqlDataReader reader = npgsqlCommand.ExecuteReader();

				(int movieId, int systemDate, int startTime, int roomId) = (0, 0, 0, 0);

				while (reader.Read())
				{
					movieId = Convert.ToInt32(reader[0]);
					systemDate = Convert.ToInt32(reader[1]);
					startTime = Convert.ToInt32(reader[2]);
					roomId = Convert.ToInt32(reader[3]);
				}
				reader.Close();

				sql = "SELECT abbrevtext FROM movie WHERE movieid=" + movieId;

				NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);

				string abbrevText = Convert.ToString(cmd.ExecuteScalar());

				MessageBoxButtons buttons = MessageBoxButtons.YesNo;
				DialogResult result = MessageBox.Show("This will delete the Movie '" + abbrevText + "' in Auditorium " + roomId + " starting at the 24hr time of " + startTime + " on this YYYYMMDD of " + systemDate + ". Are you certain?", "Are you sure?", buttons, MessageBoxIcon.Question);
				if (result == DialogResult.No)
				{
					return;
				}
				sql = "DELETE FROM featureschedule WHERE featureid=" + featureId + ";" + "DELETE FROM schedule WHERE featureid=" + featureId + ";";
				NpgsqlCommand cmd2 = new NpgsqlCommand(sql, conn);
				cmd2.ExecuteNonQuery();

				label14.Text = "The showing with feature ID (" + featureId + ") is removed!\nPlease move out and back into the 'Show Times' modual in Process to see the change.";

			}
		}
    }
 }  




