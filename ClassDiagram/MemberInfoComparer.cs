//
// Copyright (c) 2022, MindFusion LLC - Bulgaria.
//

using System;
using System.Collections;
using System.Reflection;


namespace MindFusion.Diagramming.Wpf.Samples.CS.ClassDiagram
{
	public class MemberInfoComparer : IComparer
	{
		public MemberInfoComparer()
		{
			_comparer = new Comparer(
				System.Globalization.CultureInfo.InvariantCulture);
		}

		public int Compare(object x, object y)
		{
			MemberInfo xx = x as MemberInfo;
			MemberInfo yy = y as MemberInfo;

			string xName = xx.Name;
			if (xx.MemberType == MemberTypes.Constructor)
				xName = xx.DeclaringType.Name;

			string yName = yy.Name;
			if (yy.MemberType == MemberTypes.Constructor)
				yName = yy.DeclaringType.Name;

			return _comparer.Compare(xName, yName);
		}

		private Comparer _comparer;
	}
}
