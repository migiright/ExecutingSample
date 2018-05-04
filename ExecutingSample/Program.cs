using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecutingSample
{
	class Program
	{
		static void Main(string[] args)
		{
			var i = new System.Diagnostics.ProcessStartInfo("a.exe") {
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				UseShellExecute = false,
			};
			using (var p = System.Diagnostics.Process.Start(i)) {
				p.StandardInput.WriteLine(14);
				Console.WriteLine(p.StandardOutput.ReadToEnd());
			}
		}
	}
}
