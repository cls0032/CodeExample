using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CodeSample
{
	public partial class ExampleCode : ServiceBase
	{
		public ExampleCode()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			MainProcess mainProcess = new MainProcess();
			mainProcess.Execute();
		}

		protected override void OnStop()
		{
		}


	}
}
