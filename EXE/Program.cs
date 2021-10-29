using System;
using System.Reflection;

namespace DOTNETCORE
{
	class Program
	{
		static void Main(string[] args)
		{
			foreach (var arg in args)
			{
				if (arg.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
					Assembly.LoadFrom(arg);
				else
					Assembly.LoadFrom(arg + ".dll");
			}
			Adpter.Run();
		}
	}
}
