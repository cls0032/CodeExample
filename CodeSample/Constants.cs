using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSample
{
	static class Constants
	{
		public const string PROCESS_NAME = "Generic Process";
		public static string PARAMETER_EMAIL = "Generic Email";
		public static string EMAIL_NOTIFICATION_TOADDRESS = "generic@lookatmego.com";
		public static string EMAIL_NOTIFICATION_FROMADDRESS = "generic@sarcasm.edu";
		public static string EMAIL_NOTIFICATION_SUBJECT = "Your Awesome Process Has Logged Something";
		public static string EMAIL_REPORT_FROMADDRESS = "generic@icandothisallday.net";
		public static string EMAIL_REPORT_SUBJECT = "Super Catchy Subject Line";
		public static string EMAIL_REPORT_BODY = "Normally bunch of HTML fun stuff";
		public static string SP_GET_FINALIZEDPOPULATION = "p_GenericProcessor_SelectFinalizedForEmail";
		public static string SP_GET_PARAMETERVALUE_BYNAME = "p_Parameters_GetValueByName";
		public static string CONN_STRING = "Data Source=DBName; Initial Catalog=BaseDB; User Id=MyUserID; Password=MyPW";
		public static string SP_GET_REPORTID_BYNAME = "p_GenericReports_GetIDByName";
		public static string SP_GET_REPORTDATA_BYID = "p_Reports_GetReportData";
		public static string SP_GET_PARAMS_BYREPORTSPROC = "p_ReportSProc_GetParams";
		public static string SMTP_SERVER = "Relay-s16.myEmailClient.local";
	}
}
