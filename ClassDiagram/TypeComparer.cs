//
// Copyright (c) 2022, MindFusion LLC - Bulgaria.
//

using System;
using System.Collections;


namespace MindFusion.Diagramming.Wpf.Samples.CS.ClassDiagram
{
	public class TypeComparer : IComparer
	{
		public TypeComparer()
		{
			_comparer = new Comparer(
				System.Globalization.CultureInfo.InvariantCulture);
		}

		public int Compare(object x, object y)
		{
			Type xx = x as Type;
			Type yy = y as Type;

			int xPriority = GetPriority(xx);
			int yPriority = GetPriority(yy);

			if (xPriority < yPriority)
				return -1;

			if (xPriority > yPriority)
				return 1;

			return _comparer.Compare(xx.Name, yy.Name);
		}

		private int GetPriority(Type type)
		{
			// Classes first
			if (type.IsClass)
			{
				// Delegates last
				if (type.IsSubclassOf(typeof(Delegate)))
					return 3;

				return 0;
			}

			// Interfaces next
			if (type.IsInterface)
				return 1;

			// Structs next
			if (type.IsValueType)
				return 2;

			return 4;
		}


		private Comparer _comparer;
	}
}
