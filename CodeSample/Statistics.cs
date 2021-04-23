using System;
using System.Collections.Generic;
using System.Text;

namespace CodeSample
{
	public class Statistics 
	{
		public Statistics() { }
		public Statistics(string title) { }

		public int Processed { get; set; }
		public int Successes { get; set; }

		public List<string> ErrorDescriptions;

		public string ToHtml()
		{
			string retVal = "";
			//normally would be fanciness here to set it up to look pretty when we get
			//the statistics/error reports/
			//but for now I'm just throwing it to a string. 
			StringBuilder myBuilder = new StringBuilder();
			foreach (string errorMessage in ErrorDescriptions)
			{
				myBuilder.Append(errorMessage);
			}
			retVal = myBuilder.ToString();
			retVal = $"Number Processed: {Processed.ToString()} " + retVal;
			return retVal;
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
