using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSample
{
	public class Statistics 
	{
		public Statistics() { }
		public Statistics(string title) { }

		public int Processed { get; set; }
		public int Successes { get; set; }

		public List<string> ErrorDescriptions;

		public override string ToHtml()
		{

		}

		public string Title { get; set; }
		public bool NothingToDo { get; set; }
		public int Errors { get; set; }
		public DateTime BeginDate { get; set; }
		public DateTime EndDate { get; set; }
		public string RunTime { get; }

		public void AddError(string message)
		{
			ErrorDescriptions.Add(message);
		}



	}

}
