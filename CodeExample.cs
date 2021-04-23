using System;
using System.IO;
using System.Collections.Generic;
using SchedulingService;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using DataDynamics.ActiveReports;

namespace GenericAutoProcessorService
{
    public class GenericAutoProcessorProcess : BaseProcess
    {

        private static string SP_GET_REQUESTS = "p_Generic_Select";
        private static string PROCESS_NAME = "Generic Processor";
        private static string EMAIL_NOTIFICATION_TOADDRESS = "generic@lookatmego.com";
        private static string EMAIL_NOTIFICATION_FROMADDRESS = "generic@sarcasm.edu";
        private static string PARAMETER_EMAIL = "Generic Email";
        private static string SP_GET_PARAM_BYNAME = "p_Parameters_GetIDByName";
        private static string SP_GET_REPORTID_BYNAME = "p_GenericReports_GetIDByName";
        private static string SP_GET_FINALIZEDPOPULATION = "p_GenericProcessor_SelectFinalizedForEmail";
        private static string SP_GET_PARAMS_BYREPORTSPROC = "p_ReportSProc_GetParams";
        private static string SP_GET_REPORTDATA_BYID = "p_Reports_GetReportData";
        private static string SP_GET_PROCESSORREQUESTS = "p_CEGenericProcessor_GetRequests";


      
        
        
 
        

        /// <summary>
		/// Sends the notification email once we have finished the automation process
		/// </summary>
		/// <param name="toAddress"></param>
		/// <param name="fromAddress"></param>
		/// <param name="stats"></param>
		// prob need to change this to be generic so can use it for notification email or report email
        private void SendEmailNotification(string toAddress, string fromAddress, doWorkStatistics stats)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromAddress);
                message.To = new MailAddress(toAddress)
                message.Subject = sSubject;
                message.Body = sBody;
                message.IsBodyHtml = true;

                StringBuilder sbLog = new StringBuilder();
                sbLog.Append("SENDING EMAIL LOG {\n");
                sbLog.Append(String.Format("\tFROM : {0}\n", message.From));
                sbLog.Append(String.Format("\tTO : {0}\n", sTo));
                sbLog.Append(String.Format("\tSUBJECT : {0}\n", sSubject));

                // Any CC addresses?
                this.AddAddresses(message.CC, this.Config["EmailCc"]);

                // Any BCC addresses?
                this.AddAddresses(message.Bcc, this.Config["EmailBcc"]);

                sbLog.Append("}");
                Log.Info(sbLog.ToString());

                // Create SMTP Client
                SmtpClient smtpClient = new SmtpClient(EmailSMTPServer);
                smtpClient.Send(message);
            }
            catch (Exception e)
            {
                Log.Error("EXCEPTION: Failed to send email notification.", e);
            }
        }


    }

}
