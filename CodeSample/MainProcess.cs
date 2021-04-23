using System;
using Common.Logging;
using System.IO;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Net.Mail;
using DataDynamics.ActiveReports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSample
{
	public class MainProcess
	{
        protected ILog Log;
        private Statistics stats;
        private SqlConnection conn;

        private bool OpenConnection()
        {
            // Return Value
            bool bRetVal = false;

            // Should have already validated our connection string at this point
            try
            {
                // Create and Open a new SqlConnection
                conn = new SqlConnection(Constants.CONN_STRING);
                conn.Open();

                // Set Return Value
                bRetVal = true;
            }
            catch (Exception e)
            {
                Log.Info(
                  String.Format(
                    "MyProcess: EXCEPTION : {0}\n{1}",
                    e.Message, e.StackTrace));
            }

            // Return Value
            return bRetVal;
        }

        private void CloseConnection()
        {
            if (conn != null)
            {
                try
                {
                    conn.Close();
                }
                catch (Exception e)
                {
                    Log.Info(
                      String.Format(
                        "MyProcess: EXCEPTION : {0}\n{1}",
                        e.Message, e.StackTrace));
                }
            }
        }

        /// <summary>
        /// Logs process start, checks feature flag, then processes
        /// </summary>
        public void Execute()
        {
            //TODO: create a Statistics class
            Statistics stats = new Statistics();
            stats.BeginDate = DateTime.Now;
            stats.Title = Constants.PROCESS_NAME;
            //TODO: Do I need to create a Log class as well?
            Log.Info(String.Format("{0} Started : {1:D}", Constants.PROCESS_NAME, stats.BeginDate));

            //TODO:  open connection
            // If we should run aka we can open a DB connection
            if (OpenConnection())
            {
                try
                {
                    bool emailsEnabled = (GetParameterValueByName(Constants.PARAMETER_EMAIL) == "true");
                    if (emailsEnabled)
                    {
                        ProcessPopulation();
                    }
                    stats.NothingToDo = (stats.Processed <= 0);
                }
                catch (Exception e)
                {
                    // Log Message
                    String message = String.Format("EXCEPTION : {0}\r\n{1}", e.Message, e.StackTrace);
                    Log.Error(message, e);
                    stats.ErrorDescriptions.Add(message);
                }
                finally
                {
                    // Close DB
                    CloseConnection();
                }
            }

            stats.EndDate = DateTime.Now;
            Log.Info(String.Format("Processor Ended : {0:D}", stats.EndDate));

            // Finally send notifications and log JobStatistics
            Log.Info(stats.ToString());
            try
            {
                //TODO: finish figuring out what all needs to be thrown into notification email
                SendEmail(Constants.EMAIL_NOTIFICATION_FROMADDRESS, Constants.EMAIL_NOTIFICATION_TOADDRESS,Constants.EMAIL_NOTIFICATION_SUBJECT, stats.ToHtml());
            }
            catch (Exception e)
            {
                Log.Error(String.Format("EXCEPTION : Sending Email Notification : {0}\r\n{1}", e.Message, e.StackTrace), e);
            }
        }
        /// <summary>
		/// After we have created the report, sends the emails with them attached.
		/// </summary>
		/// <returns>true if sent, false otherwise</returns>
		// does this really need to return something?
        private bool ProcessPopulation()
        {
            try
            {
                Log.Info("Processing Population...");
                SqlHelper mySqlHelper = new SqlHelper(Constants.CONN_STRING);

                string emailAttachmentPath = GetEmailAttachmentPath();
                string subfolderName = @"GenericEmailAttachments";
                string attachmentFolder = $@"{emailAttachmentPath}\{subfolderName}";
                if (!Directory.Exists(attachmentFolder))
                {
                    Directory.CreateDirectory(attachmentFolder);
                }

                DataTable dt = new DataTable();
                dt = mySqlHelper.GetDataTable(Constants.SP_GET_FINALIZEDPOPULATION, null);

                if (dt == null || dt.Rows == null && dt.Rows.Count <= 0) return false;

                string reportName = "My Report";
                Guid? reportID = GetReportIDByName(reportName);
                if (!reportID.HasValue || reportID.Value == Guid.Empty) throw new ApplicationException($"Could not find report. REPORT_NAME=[{reportName}]");

                Log.Info($"Found [{dt.Rows.Count}] finalized instances to send emails for.");
                foreach (DataRow row in dt.Rows)
                {
                    SendPopulationEmail(row, reportID.Value, subfolderName);
                }
            }
            catch (Exception ex)
            {
                stats.AddError($"An error occurred while trying to send Emails.  ERROR=[{ex.Message}]");
            }
            return true;
        }
        private string GetEmailAttachmentPath()
        {
            string networkStorageLocation = GetParameterValueByName("Email Template Attachment Location");
            while (networkStorageLocation.EndsWith(@"\"))
            {
                networkStorageLocation = networkStorageLocation.Substring(0, networkStorageLocation.Length - 1);
            }
            return networkStorageLocation;
        }
        /// <summary>
        /// For Feature Flags, gets the paramater value
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>
        /// Returns the value of the parameter
        /// </returns>
        public string GetParameterValueByName(string parameter)
        {
            string retVal = null;
            try
            {
                SqlHelper helper = new SqlHelper(Constants.CONN_STRING);
                Hashtable inParms = new Hashtable();
                Hashtable outParms = new Hashtable();
                inParms.Add("@parmName", parameter);
                outParms.Add("@parmValue", "");
                helper.ExecuteSproc(Constants.SP_GET_PARAMETERVALUE_BYNAME, inParms, ref outParms);
                retVal = outParms["@parmValue"].ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return retVal;
        }
        /// <summary>
		/// Gets the ID of the report by the name of the report we are wanting
		/// </summary>
		/// <param name="name"></param>
		/// <returns> guid ID</returns>

        public Guid GetReportIDByName(string name)
        {
            Guid retVal = Guid.Empty;
            Hashtable inParms = new Hashtable();
            Hashtable outParms = new Hashtable();
            inParms.Add("@Name", name);
            outParms.Add("@ID", Guid.Empty);
            SqlHelper mySqlHelper = new SqlHelper(Constants.CONN_STRING);
            try
            {
                //TODO: Create an execute sproc helper similar to how I did the GetDataTable
                mySqlHelper.ExecuteSproc(Constants.SP_GET_REPORTID_BYNAME, inParms, ref outParms);
                retVal = new Guid(outParms["@ID"].ToString());
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return retVal;
        }
        /// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		/// <param name="reportID"></param>
		/// <param name="subfolderName"></param>
		// TODO: create a new method that will take addresses etc and send email via SMTP
        private void SendPopulationEmail(DataRow row, Guid reportID, string subfolderName)
        {
            Guid populationID = Guid.Empty;
            try
            {

                populationID = (Guid)row["ID"];
                Log.Info($"Sending email.  REQUEST_ID=[{populationID.ToString()}]");

                string genericNumber = row["genericNumber"].ToString().Replace(" ", "");
                string genericName = row["genericName"].ToString().Replace(" ", "");
                string genericEmail = row["genericEmail"].ToString();
                string reportFileName = $"certificate_{genericName}_{genericNumber}.pdf";
                string reportFileRelativePath = $@"{subfolderName}\{reportFileName}";
                string emailAttachmentPath = GetEmailAttachmentPath();
                string reportFileFullPath = $@"{emailAttachmentPath}\{reportFileRelativePath}";

                // Create the file attachment
                Attachment fileAttachment = CreateFileAttachment(populationID, reportID, reportFileFullPath);

                // Email with the file attachment
                SendEmail(Constants.EMAIL_REPORT_FROMADDRESS, genericEmail, Constants.EMAIL_REPORT_SUBJECT, Constants.EMAIL_REPORT_BODY, fileAttachment);

                stats.Successes++;
            }
            catch (Exception ex)
            {
                stats.AddError($"Error sending Email.  REQUEST_ID=[{populationID.ToString()}], ERROR=[{ex.Message}]");
            }
            finally
            {
                stats.Processed++;
            }
        }
        /// <summary>
		/// creates the report attachment?
		/// </summary>
		/// <param name="populationID"></param>
		/// <param name="reportID"></param>
		/// <param name="certFileFullPath"></param>
		/// <returns></returns>
        private Attachment CreateFileAttachment(Guid populationID, Guid reportID, string reportFileFullPath)
        {
            Attachment retVal;

            try
            {
                Hashtable parms = new Hashtable() { { "@PopulationID", populationID } };
                var thisReport = GetReportObject(reportID, parms);
                thisReport.Run();
                var exporter = new DataDynamics.ActiveReports.Export.Pdf.PdfExport();
                MemoryStream mStream = new MemoryStream();
                exporter.Export(thisReport.Document, mStream);
                mStream.Position = 0;
                if (mStream == null) throw new ApplicationException("Error creating MemoryStream.");

                // Create file attachment
                if (File.Exists(reportFileFullPath))
                {
                    File.Delete(reportFileFullPath);
                }
                FileStream fs = new FileStream(reportFileFullPath, FileMode.OpenOrCreate, FileAccess.Write);
                Int32 iBufferSize = 10000;
                byte[] buffer = new byte[iBufferSize];

                long dataToRead = mStream.Length;
                while (dataToRead > 0)
                {
                    var length = mStream.Read(buffer, 0, iBufferSize);
                    fs.Write(buffer, 0, length);
                    dataToRead -= length;
                }
                Attachment emailAttachment = new Attachment(fs, "name");
                fs.Flush();
                fs.Close();
                fs = null;
                retVal = emailAttachment;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the File Attachment.  ERROR=[{ex.Message}]", ex);
            }
            return retVal;
        }
        /// <summary>
        /// Creates the actual report object (in this case a certificate)
        /// </summary>
        /// <param name="reportID"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public DataDynamics.ActiveReports.ActiveReport GetReportObject(Guid reportID, Hashtable parms)
        {
            Report newReport = GetReport(reportID);
            DataDynamics.ActiveReports.ActiveReport tempReport = new DataDynamics.ActiveReports.ActiveReport();
            if (newReport.reportData.Length > 0)
            {
                System.Text.Encoding unicodeEncoder = System.Text.Encoding.Unicode;
                char[] chars = newReport.reportData.ToCharArray(0, newReport.reportData.Length);
                int byteCount = unicodeEncoder.GetByteCount(chars, 0, newReport.reportData.Length);
                byte[] buffer = new byte[byteCount];

                unicodeEncoder.GetBytes(chars, 0, newReport.reportData.Length, buffer, 0);

                System.IO.MemoryStream _stream = new System.IO.MemoryStream(buffer);

                _stream.Position = 0;
                tempReport.LoadLayout(_stream);
                tempReport.ScriptLanguage = "VB.NET";
                tempReport.Script = newReport.reportScript;

                string[] reportParms = GetReportStoredProcParms(newReport.storedProc);

                if (reportParms.Length > 0)
                {
                    foreach (string s in reportParms)
                    {
                        foreach (DictionaryEntry something in parms)
                        {
                            if (something.Key.ToString().ToLower() == s.ToLower())
                            {
                                newReport.storedProc += " " + something.Key.ToString() + "='" + something.Value.ToString() + "',";
                            }
                        }
                    }
                    newReport.storedProc = newReport.storedProc.Substring(0, newReport.storedProc.Length - 1);
                }

                DataTable dt = new DataTable();
                SqlHelper mySqlHelper = new SqlHelper(Constants.CONN_STRING);
                dt = mySqlHelper.GetDataTable(newReport.storedProc, null);

                if (tempReport.Script.Equals(null) || tempReport.Script == "")
                {
                    tempReport.DataSource = dt;
                }
                else
                {
                    DataDynamics.ActiveReports.DataSources.SqlDBDataSource _ds = new DataDynamics.ActiveReports.DataSources.SqlDBDataSource();
                    _ds.ConnectionString = Constants.CONN_STRING;
                    _ds.SQL = newReport.storedProc;
                    tempReport.DataSource = _ds;
                }
                _stream.Close();
            }
            return tempReport;
        }

        /// <summary>
        /// Gets the report info from the database
        /// The report is designed in a screen beforehand and all the info is saved as a template
        /// </summary>
        /// <param name="guidID"></param>
        /// <returns></returns>
        public Report GetReport(Guid guidID)
        {
            Report retVal = new Report();
            DataTable dt = new DataTable();
            Hashtable inParms = new Hashtable();
            inParms.Add("@ID", guidID);
            SqlHelper mySqlHelper = new SqlHelper(Constants.CONN_STRING);

            dt = mySqlHelper.GetDataTable(Constants.SP_GET_REPORTDATA_BYID, inParms);

            if (dt.Rows.Count > 0)
            {
                retVal = new Report(dt.Rows[0]);
            }
            return retVal;
        }
        /// <summary>
		/// Some reports have replacement values within, this gets the sproc parameters for those
		/// </summary>
		/// <param name="storedProcName"></param>
		/// <returns></returns>
        private string[] GetReportStoredProcParms(string storedProcName)
        {

            DataTable dt = new DataTable();
            Hashtable parms = new Hashtable();
            parms.Add("@SprocName", storedProcName);
            SqlHelper mySqlHelper = new SqlHelper(Constants.CONN_STRING);
            dt = mySqlHelper.GetDataTable(Constants.SP_GET_PARAMS_BYREPORTSPROC, parms);

            string[] retVal = new string[dt.Rows.Count];

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < retVal.Length; i++)
                {
                    retVal[i] = dt.Rows[i]["name"].ToString();
                }
            }
            return retVal;
        }

        private void SendEmail(string fromAddress, string toAddress, string subject, string body, Attachment reportFile)
		{
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromAddress);
                message.To.Add(toAddress);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                message.Attachments.Add(reportFile);

                StringBuilder sbLog = new StringBuilder();
                sbLog.Append("SENDING EMAIL LOG {\n");
                sbLog.Append(String.Format("\tFROM : {0}\n", message.From));
                sbLog.Append(String.Format("\tTO : {0}\n", toAddress));
                sbLog.Append(String.Format("\tSUBJECT : {0}\n", fromAddress));

                sbLog.Append("}");
                Log.Info(sbLog.ToString());

                // Create SMTP Client
                SmtpClient smtpClient = new SmtpClient(Constants.SMTP_SERVER);
                smtpClient.Send(message);
            }
            catch (Exception e)
            {
                Log.Error("EXCEPTION: Failed to send email.", e);
            }
        }

    }
}
