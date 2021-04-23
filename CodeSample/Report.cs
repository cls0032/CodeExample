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
    public class Report //: GenericAutoProcessorProcess
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
                application = dr["Name"].ToString();
                reportName = dr["ReportName"].ToString();
                desc = dr["Description"].ToString();
                fileName = dr["FileName"].ToString();
                storedProc = dr["StoredProc"].ToString();
                active = (dr["Active"] == DBNull.Value) ? false : Convert.ToBoolean(dr["Active"]);
                archived = (dr["Archived"] == DBNull.Value) ? true : Convert.ToBoolean(dr["Archived"]);
                reportData = dr["ReportData"].ToString();
                reportScript = dr["ReportScript"].ToString();

            }
        }

    }
}
