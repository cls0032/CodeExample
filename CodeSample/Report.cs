using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSample
{
    /// <summary>
	/// Report class for the Report Template that we will use
	/// </summary>
    public class Report 
    {
        public string application { get; set; }
        public string reportName { get; set; }
        public string desc { get; set; }
        public string fileName { get; set; }
        public string reportData { get; set; }
        public string reportScript { get; set; }
        public string storedProc { get; set; }
        public bool active { get; set; }
        public bool archived { get; set; }

        public Report() { }
        public Report(DataRow dr) //: base()
        {
            if (dr != null)
            {
                application = (dr["Name"] == DBNull.Value) ? "Student Name" : dr["Name"].ToString();
                reportName = (dr["ReportName"] == DBNull.Value) ? "Generic Report" : dr["ReportName"].ToString();
                desc = (dr["Description"] == DBNull.Value) ? "Generic Description" : dr["Description"].ToString();
                fileName = (dr["FileName"] == DBNull.Value) ? "Generic FileName" : dr["FileName"].ToString();
                storedProc = (dr["StoredProc"] == DBNull.Value) ? "p_Generice_DoSomething" : dr["StoredProc"].ToString();
                archived = (dr["Archived"] == DBNull.Value) ? true : Convert.ToBoolean(dr["Archived"]);
                reportData = (dr["ReportData"] == DBNull.Value) ? "Generic Data" : dr["ReportData"].ToString();
                reportScript = (dr["ReportScript"] == DBNull.Value) ? "Generic Script" : dr["ReportScript"].ToString();

            }
        }

    }
}
