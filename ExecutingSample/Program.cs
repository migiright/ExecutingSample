using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ExecutingSample
{
	class Program
	{
		static readonly IReadOnlyCollection<string> Sources = new ReadOnlyCollection<string>(new string[]{
				"compileError.cpp",
				//"runtimeError.cpp",
				//"timeout.cpp",
				//"wrongOutput.cpp",
				//"correct.cpp",
		});
		static readonly ProcessStartInfo CompileProcessStartInfo = new ProcessStartInfo(Environment.GetEnvironmentVariable("comspec")) {
			RedirectStandardOutput = true,
			RedirectStandardInput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			//Arguments =  @"""/k """"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat""""""",
			Arguments =  @"""/k """"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat""""""",
		};

		const string Options = @"/GS /analyze- /W3 /Od /Zc:inline /fp:precise /RTC1 /Oy- /MDd /EHsc /nologo";

		static readonly Regex ReturnValueRegex = new Regex(@"^@@@errorlevel=(\d+)$", RegexOptions.Compiled);

		static void Compile()
		{
			//Sources.Select(async s => {
			//	using(var p = Process.Start(CompileProcessStartInfo)) {
			//		await p.StandardInput.WriteLineAsync($"cl sources/{s} {Options}");
			//		await p.StandardInput.WriteLineAsync("echo errorlevel=%errorlevel%");

			//	}
			//});
			var o = Sources.Select(async s => {
				Match m;
				var co = new List<string>();
				using (var p = Process.Start(CompileProcessStartInfo)) {
					await p.StandardInput.WriteLineAsync($"cl sources/{s} {Options}");
					await p.StandardInput.WriteLineAsync("echo @@@errorlevel=%errorlevel%");
					await p.StandardInput.WriteLineAsync("exit");
					string l;
					while ((m = ReturnValueRegex.Match(l = await p.StandardOutput.ReadLineAsync())).Value == "") {
						co.Add(l);
					}
				}
				return (s: s, r: int.Parse(m.Groups[1].Value), co: co.GetRange(6, co.Count-6-2).Aggregate("", (ls, l) => ls + l + Environment.NewLine));
				//return (s: s, r: 0, co: "aaa");
			})
			.ToObservable()
			.Merge();
			o.Subscribe((v) => {
				Console.WriteLine($"{v.s} is completed by return value {v.r}");
				//if(v.r != 0) {
				Console.Write(v.co);
				//}

				using (var p = Process.Start(new ProcessStartInfo($"{v.s.Substring(0, v.s.Length-4)}.exe") {
					RedirectStandardOutput = true,
					RedirectStandardInput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
				})) {
					var l = new List<string>();
					string s;
					p.WaitForExit();
					for (int i = 0; i < 10; ++i) { p.StandardInput.WriteLine(); }
					while ((s = p.StandardOutput.ReadLine()) != null) {
						Console.WriteLine(s);
					}
					Console.WriteLine("end");
				}
			});
			Console.ReadLine();
			o.Wait();
		}

		private static void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine("error: " + e.Data);
		}

		private static void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine(e.Data);
		}

		static void Main(string[] args)
		{
			Compile();
		}
	}
}
