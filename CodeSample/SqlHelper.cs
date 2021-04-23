using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSample
{
    /// <summary>
    /// Does SQL functions
    /// </summary>
	public class SqlHelper 
	{
        private const int DEFAULT_TIMEOUT = 120;
        public string ConnectionString { get; set; }
        public int ConnectionTimeout { get; set; }

        public SqlHelper(string connectionString)
        {
            SetupDefaults();
            this.ConnectionString = connectionString;
        }
        private void SetupDefaults()
        {
            this.ConnectionTimeout = DEFAULT_TIMEOUT;
        }
        public DataTable GetDataTable(string sprocName, Hashtable parms)
        {
            DataTable retVal = new DataTable();
            // Open connection
            conn = new SqlConnection(!String.IsNullOrEmpty(connString) ? connString : this.ConnectionString);
            conn.Open();

            // Create command
            cmd = new SqlCommand
            {
                CommandText = sprocName,
                Connection = conn,
                CommandTimeout = connTimeout.HasValue ? connTimeout.Value : this.ConnectionTimeout,
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters (if any)
            if (parms != null)
            {
                foreach (DictionaryEntry de in parms)
                {
                    if (de.Value != null)
                    {
                        SqlParameter param;
                        param = CreateParamater(de);
                        cmd.Parameters.Add(param);
                    }

                }
            }

            //Execute command
            try
            {
                adapter = new SqlDataAdapter(cmd);
                adapter.Fill(retVal);
            }
            catch (Exception e)
            {
                resultMessage = String.Format("Error processing sproc: {0}, error: {1}, {2}", sprocName, e.Message, e.StackTrace);
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (adapter != null) adapter.Dispose();
                if (conn.State != ConnectionState.Closed) conn.Close();
            }
            if (resultMessage != "") Log.Info(resultMessage);

            return retVal;
        }
        private SqlParameter CreateParamater(DictionaryEntry dictEnt)
        {
            SqlParameter retVal;
            switch (dictEnt.Value.GetType().ToString())
            {
                case "System.Guid":
                    retVal = new SqlParameter(dictEnt.Key.ToString(), SqlDbType.UniqueIdentifier);
                    retVal.Value = dictEnt.Value;
                    break;

                case "System.DateTime":
                    retVal = new SqlParameter(dictEnt.Key.ToString(), SqlDbType.DateTime);
                    retVal.Value = dictEnt.Value;
                    break;

                default:
                    retVal = new SqlParameter(dictEnt.Key.ToString(), dictEnt.Value.ToString());
                    break;
            }
            return retVal;
        }
        public void ExecuteSproc(string sprocName, Hashtable inParms, ref Hashtable outParms)
		{
            // Declare vars
            SqlCommand cmd;
            SqlConnection conn;


            // Open connection
            conn = new SqlConnection(!String.IsNullOrEmpty(connString) ? connString : this.ConnectionString);
            conn.Open();


            // Create command
            cmd = new SqlCommand
            {
                CommandText = sprocName,
                Connection = conn,
                CommandTimeout = connTimeout.HasValue ? connTimeout.Value : this.ConnectionTimeout,
                CommandType = CommandType.StoredProcedure
            };

            if (inParms != null)
			{
                foreach (DictionaryEntry dictEnt in inParms)
				{
                    SqlParameter parm = CreateParamater(dictEnt);
                    cmd.Parameters.Add(parm);
				}
			}

            if (outParms != null)
			{
                foreach (DictionaryEntry dictEnt in outParms)
				{
                    SqlParameter parm = CreateParamater (dictEnt);
                    parm.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(parm);
				}
			}

            // Execute command
            try
            {
                cmd.ExecuteNonQuery();

                if (outParms != null && outParms.Count > 0)
                {
                    foreach (SqlParameter p in cmd.Parameters)
                    {
                        if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output)
                        {
                            outParms[p.ParameterName] = p.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error executing stored procedure {0}", sprocName), ex);
            }
            finally
            {
                // Cleanup
                if (cmd != null) cmd.Dispose();
                if (conn.State != ConnectionState.Closed) conn.Close();
            }
        }
    }
}
