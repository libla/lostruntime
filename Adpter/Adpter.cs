using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class Adpter
{
	public static void Run()
	{
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (Type type in assembly.GetAllTypes())
			{
				object[] attrs = type.GetCustomAttributes(typeof(InitAttribute), false);
				for (int i = 0; i < attrs.Length; i++)
				{
					InitAttribute? attr = attrs[i] as InitAttribute;
					if (attr != null)
					{
						RuntimeHelpers.RunClassConstructor(type.TypeHandle);
						break;
					}
				}
			}
		}
		run?.Invoke();
	}

	private static IEnumerable<Type> GetAllTypes(this Assembly assembly)
	{
		Type?[] types;
		try
		{
			types = assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException e)
		{
			types = e.Types;
		}
		return types.Where(type => type != null)!;
	}

	public static Action run;
}

public class InitAttribute : Attribute { }