//
// Copyright (c) 2022, MindFusion LLC - Bulgaria.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using MindFusion.Diagramming.Wpf.Layout;


namespace MindFusion.Diagramming.Wpf.Samples.CS.ClassDiagram
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			overview.DiagramView = diagramView;
			overview.ScaleMode = ScaleMode.CombinedScales;
			diagram.Selection.Style = SelectionStyle.Frame;
			_openFileDialog = new OpenFileDialog();

			diagram.TableRowHeight = 5 * coef;
			diagram.TableColumnWidth = 4 * coef;
			diagram.ShowAnchors = ShowAnchors.Never;
			diagram.TableShape = SimpleShape.RoundedRectangle;
			diagram.TableConnectionStyle = TableConnectionStyle.Both;

			Style tableNodeStyle = new Style();
			tableNodeStyle.Setters.Add(new Setter(TableNode.FontSizeProperty, 9.0));
			diagram.TableNodeStyle = tableNodeStyle;
		}

		double coef = 3.5;


		private void menuLoadAssembly_Click(object sender, RoutedEventArgs e)
		{
			if (_openFileDialog.ShowDialog() == true)
			{
				// Create the TypeName <-> TableNode map
				_workMap = new Hashtable();

				// Create tables nodes and fill the tree
				ProcessAssembly(_openFileDialog.FileName);

				// Generate links between classes
				GenerateLinks();

				// Arrange the diagram
				Arrange();
			}
		}

		private void menuExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void ProcessAssembly(string assemblyFile)
		{
			// Clear the tree
			treeView1.Items.Clear();

			// Clear diagram
			diagram.ClearAll();

			// Load the assembly
			Assembly assembly;

			try
			{
				assembly = Assembly.LoadFrom(assemblyFile);
			}
			catch (Exception exception)
			{
				MessageBox.Show("Failed to load assembly.\nReason: \"" +
					exception.Message + "\"", "Class Diagrammer",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (assembly == null)
			{
				MessageBox.Show("Failed to load assembly.", "ClassDiagrammer",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Get exported types
			Type[] types = assembly.GetExportedTypes();

			// Sort the exported types
			ArrayList sortedTypes = new ArrayList(types);
			sortedTypes.Sort(new TypeComparer());

			// Populate the tree view with assembly information
			foreach (Type type in sortedTypes)
			{
				// Add the namespace of the type
				var node = AddNamespace(type.Namespace);

				// Add the type itself
				AddType(node, type);
			}

			// Update tables' bounds
			foreach (DiagramNode node in diagram.Nodes)
			{
				TableNode t = node as TableNode;
				if (t == null)
					continue;

				int rows = Math.Min(8, t.RowCount);
				double height = (7 + rows * 5) * coef;

				t.Bounds = new Rect(
					t.Bounds.X, t.Bounds.Y,
					t.Bounds.Width, height);
			}

			// Combine enums & delegates
			DiagramNodeCollection enums = new DiagramNodeCollection();
			DiagramNodeCollection delegates = new DiagramNodeCollection();
			foreach (DiagramNode node in diagram.Nodes)
			{
				TableNode t = node as TableNode;
				if (t == null)
					continue;

				Entity entity = t.Tag as Entity;
				if (entity.Type == ImageEnum)
				    enums.Add(t);
				else if (entity.Type == ImageDelegate)
					delegates.Add(t);
			}

			BuildGroup(enums, "Enums", ":enums");
			BuildGroup(delegates, "Delegates", ":delegates");
		}

		private void BuildGroup(DiagramNodeCollection tables, string name, object tag)
		{
			if (tables.Count > 0)
			{
				int side = (int)Math.Ceiling(Math.Sqrt(tables.Count));
				int rows = (int)Math.Ceiling((double)tables.Count / side);
				double singleWidth = tables[0].Bounds.Width;
				double width = 5 * (side + 1) * coef + singleWidth * side;
				double height = (5 * (rows + 1) + 5) * coef;

				double[] rowHeights = new double[rows];
				for (int i = 0; i < tables.Count; i++)
					rowHeights[i / side] = Math.Max(tables[i].Bounds.Height, rowHeights[i / side]);

				foreach (double h in rowHeights)
					height += h;

				var container = diagram.Factory.CreateContainerNode(0, 0, width, height);

				int index = 0;
				foreach (TableNode e in tables)
				{
					container.Add(e);

					double x = (index % side) * (5 + singleWidth);
					double y = 0;

					for (int r = 0; r < index / side; r++)
						y += 5 * coef + rowHeights[r];

					e.Bounds = new Rect(x + 5 * coef, y + 10 * coef,
						e.Bounds.Width, e.Bounds.Height);

					index++;
				}

				container.Caption = name;
				container.TextAlignment = TextAlignment.Center;
				container.TextVerticalAlignment = AlignmentY.Center;
				container.HandlesStyle = HandlesStyle.HatchHandles3;
				container.FontSize = 12;
				container.FontWeight = FontWeights.Bold;
				container.IgnoreLayout = true;
				container.Tag = tag;
				container.UpdateBounds();
			}
		}

		/// <summary>
		/// Walks the tree and creates the appropriate tables in the diagram.
		/// </summary>
		private void GenerateLinks()
		{
			// Create relation between types in the assembly
			foreach (DiagramNode node in diagram.Nodes)
			{
				TableNode t = node as TableNode;
				if (t == null)
					continue;

				//TreeViewItem treeNode = t.Tag as TreeViewItem;
				//Type type = treeNode.Tag as Type;
				Entity entity = t.Tag as Entity;
				Type type = entity.Info as Type;
				Type baseType = type.BaseType;
				if (baseType == null)
					continue;

				string baseTypeName = baseType.Namespace + Type.Delimiter + baseType.Name;

				TableNode parent = _workMap[baseTypeName] as TableNode;
				if (parent != null)
					//diagram.Links.Add(new DiagramLink(diagram, parent, t));
					diagram.Factory.CreateDiagramLink(parent, t);
			}
		}

		/// <summary>
		/// Arranges the diagram.
		/// </summary>
		private void Arrange()
		{
			TreeLayout layout = new TreeLayout();

			layout.Type = TreeLayoutType.Centered;
			layout.LinkStyle = TreeLayoutLinkType.Cascading3;
			layout.LevelDistance = 25;
			layout.KeepGroupLayout = true;
			layout.Anchoring = Anchoring.Keep;
			layout.MultipleGraphsPlacement = MultipleGraphsPlacement.MinimalArea;
			layout.Margins = new Size(50, 50);

			layout.Arrange(diagram);

			// Place enums and delegates at the end
			DiagramNode enums = diagram.FindNode(":enums");
			DiagramNode delegates = diagram.FindNode(":delegates");

			double x = 0;
			foreach (DiagramNode node in diagram.Nodes)
			{
				TableNode t = node as TableNode;
				if (t == null)
					continue;

				if (t.Container == null)
					x = Math.Max(x, t.Bounds.Right);
			}

			if (enums != null)
			{
				enums.Move(x + 5, 5);
				x = enums.Bounds.Right;
			}

			if (delegates != null)
			{
				delegates.Move(x + 5, 5);
			}

			diagram.ResizeToFitItems(5);
		}

		private System.Windows.Controls.TreeViewItem AddNamespace(string namespaceName)
		{
			string[] names = namespaceName.Split(new char[] { '.' });

			ItemCollection nodes = treeView1.Items;
			System.Windows.Controls.TreeViewItem node = null;

			foreach (string name in names)
			{
				node = null;
				foreach (System.Windows.Controls.TreeViewItem n2 in nodes)
				{
					if ((n2.Header as Entity).Name == name)
					{
						node = n2;
						break;
					}
				}

				if (node == null)
				{
					// Find where to insert the new node
					int pos = nodes.Count;
					for (int i = 0; i < nodes.Count; i++)
					{
						var n3 = nodes[i] as System.Windows.Controls.TreeViewItem;
						if (DefaultComparer.Compare((n3.Header as Entity).Name, name) > 0)
							pos = i;
					}

					node = new System.Windows.Controls.TreeViewItem()
					{
						Header = new Entity
						{
							Name = name,
							Image = _ImageNamespace,
							Info = null,
							Type = ImageNamespace,
						},
					};
					nodes.Insert(pos, node);
				}

				nodes = node.Items;
			}

			return node;
		}

		private System.Windows.Controls.TreeViewItem AddType(System.Windows.Controls.TreeViewItem parent, Type type)
		{
			if (type.MemberType == MemberTypes.NestedType)
			{
				Stack stack = new Stack();

				Type tempType = type;
				while (tempType.MemberType == MemberTypes.NestedType)
				{
					tempType = tempType.DeclaringType;
					stack.Push(tempType);
				}

				var tempParent = parent;
				while (stack.Count > 0)
					tempParent = AddTypeNoContents(tempParent, stack.Pop() as Type);

				return AddTypeWithContents(tempParent, type);
			}
			else
			{
				return AddTypeWithContents(parent, type);
			}
		}

		/// <summary>
		/// Adds the specified type to the tree without
		/// adding the type contents.
		/// </summary>
		private System.Windows.Controls.TreeViewItem AddTypeNoContents(System.Windows.Controls.TreeViewItem parent, Type type)
		{
			// Do not add if already in
			foreach (System.Windows.Controls.TreeViewItem node in parent.Items)
			{
				if ((node.Header as Entity).Name == type.Name)
					return node;
			}

			// Create a table for the type
			//TableNode t = new TableNode(diagram);
			TableNode t = diagram.Factory.CreateTableNode(
				new Rect(10 * coef, 10 * coef, 40 * coef, 10 * coef));
			t.CellTextStyle.FontSize = 9;
			t.CellFrameStyle = CellFrameStyle.None;
			t.TextAlignment = TextAlignment.Center;
			t.TextVerticalAlignment = AlignmentY.Center;
			//FormattedText form = new FormattedText(type.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(
			//    new FontFamily("Arial"),FontStyles.Normal,FontWeights.Bold,FontStretches.Normal), 15, Brushes.Black);
			t.Caption = type.Name;
			t.RedimTable(3, 0);
			t.ToolTip = type.Namespace + Type.Delimiter + type.Name;
			t.OffsetHeaderRows = true;
			t.FontWeight = FontWeights.Bold;
			//diagram.Nodes.Add(t);

			int entityType = -1;
			ImageSource image = null;
			if (type.IsClass)
			{
				if (type.IsSubclassOf(typeof(Delegate)))
				{
					entityType = ImageDelegate;
					image = _ImageDelegate;

					t.Brush = DelegateBrush;
				}
				else
				{
					entityType = ImageClass;
					image = _ImageClass;

					// Set brush and tooltip
					t.Brush = ClassBrush;

					if (!type.BaseType.Equals(typeof(Object)))
					{
						t.ToolTip += type.BaseType.Namespace +
							Type.Delimiter + type.BaseType.Name;
					}
				}
			}
			else if (type.IsInterface)
			{
				entityType = ImageInterface;
				image = _ImageInterface;

				t.Brush = InterfaceBrush;
			}
			else if (type.IsEnum)
			{
				entityType = ImageEnum;
				image = _ImageEnum;

				t.Brush = EnumBrush;
			}
			else if (type.IsValueType)
			{
				entityType = ImageStruct;
				image = _ImageStruct;

				t.Brush = StructBrush;
			}

			var child = new System.Windows.Controls.TreeViewItem();
			//child.Tag = type;
			child.Header = new Entity()
			{
				Name = type.Name,
				Info = type,
				Image = image,
				Type = entityType,
			};
			parent.Items.Add(child);

			t.Tag = child.Header;
			t.CustomDraw = CustomDraw.Additional;
			t.AnchorPattern = AnchorPattern.TopInBottomOut;

			_workMap[type.Namespace + Type.Delimiter + type.Name] = t;

			return child;
		}

		/// <summary>
		/// Adds the specified type to the tree without recursing
		/// upwards if the type is nested.
		/// </summary>
		private System.Windows.Controls.TreeViewItem AddTypeWithContents(
			System.Windows.Controls.TreeViewItem parent, Type type)
		{
			// Add type item
			var child = AddTypeNoContents(parent, type);
			Entity entity = child.Header as Entity;

			// Get the table that represents the type
			TableNode table = _workMap[type.Namespace + Type.Delimiter + type.Name] as TableNode;

			// Add contents
			//switch (parent.Items.IndexOf(child))
			switch (entity.Type)
			{
				case ImageClass:
				case ImageStruct:
				case ImageInterface:
					{
						// Enumerate methods first
						MethodInfo[] methods = type.GetMethods(
							BindingFlags.Instance | BindingFlags.Public |
								BindingFlags.NonPublic |
									BindingFlags.Static | BindingFlags.DeclaredOnly);

						ConstructorInfo[] constructors = type.GetConstructors(
							BindingFlags.Instance | BindingFlags.Public |
								BindingFlags.NonPublic |
									BindingFlags.Static | BindingFlags.DeclaredOnly);

						ArrayList sortedMethods = new ArrayList();
						sortedMethods.AddRange(methods);
						sortedMethods.AddRange(constructors);
						sortedMethods.Sort(new MemberInfoComparer());

						int methodsRow = -1;

						foreach (MethodBase method in sortedMethods)
						{
							// We skip private stuff
							if (method.IsPrivate)
								continue;

							// We skip internal stuff
							if (method.IsAssembly)
								continue;

							// We process only methods here
							if (method.IsSpecialName && !method.IsConstructor)
								continue;

							int methodIcon = method.IsFamily ? ImagePMethod : ImageMethod;

							string methodName = method.Name;
							if (method.MemberType == MemberTypes.Constructor)
								methodName = method.DeclaringType.Name;

							var childChild = new System.Windows.Controls.TreeViewItem();
							//childChild.Header = methodName;
							childChild.Header = new Entity()
							{
								Name = methodName,
								Info = method,
								Image = method.IsFamily ? _ImagePMethod : _ImageMethod,
								Type = method.IsFamily ? ImagePMethod : ImageMethod,
							};
							//childChild.Tag = method;
							child.Items.Add(childChild);

							// Add a row to the table (if not overload)
							int col = 2;
							if (!method.IsStatic)
								col = 1;

							if (methodsRow == -1)
							{
								methodsRow = table.AddRow();
								table[0, methodsRow].ColumnSpan = 3;
								table[0, methodsRow].Text = "Methods";
								table[0, methodsRow].TextAlignment = TextAlignment.Left;
								table[0, methodsRow].TextVerticalAlignment = AlignmentY.Center;
								table[0, methodsRow].FontWeight = FontWeights.Bold;
								table.Rows[methodsRow].Header = true;
								table.Rows[methodsRow].Expanded = true;
							}
							else
							{
								bool found = false;
								for (int r = methodsRow + 1; r < table.RowCount; r++)
								{
									if (table[col, r].Text == methodName)
									{
										int overloads = (int)table[col, r].Tag + 1;
										table[col, r].ToolTip = methodName + " (+ " + overloads +
											(overloads == 1 ? " overload)" : " overloads)");
										table[col, r].Tag = overloads;

										found = true;
										break;
									}
								}

								if (found)
									continue;
							}

							int row = table.AddRow();
							if (!method.IsStatic)
								table[col, row].ColumnSpan = 2;

							table[0, row].Image = (ImageSource)Application.Current.Resources["Method"];
							table[0, row].ImageAlign = ImageAlign.Center;
							table[col, row].Text = methodName;
							table[col, row].TextAlignment = TextAlignment.Left;
							table[col, row].TextVerticalAlignment = AlignmentY.Center;
							table[col, row].ToolTip = methodName;
							table[col, row].Tag = 0;

							if (method.IsStatic)
							{
								table[1, row].Image = (ImageSource)Application.Current.Resources["Static"];
								table[1, row].ImageAlign = ImageAlign.Center;
							}
						}

						// Enumerate properties
						PropertyInfo[] properties = type.GetProperties(
							BindingFlags.Instance | BindingFlags.Public |
								BindingFlags.NonPublic |
									BindingFlags.Static | BindingFlags.DeclaredOnly);

						ArrayList sortedProperties = new ArrayList(properties);
						sortedProperties.Sort(new MemberInfoComparer());

						int propertiesRow = -1;

						foreach (PropertyInfo property in sortedProperties)
						{
							MethodInfo getMethod = property.GetGetMethod(true);
							MethodInfo setMethod = property.GetSetMethod(true);

							if (getMethod == null && setMethod == null)
								continue;

							bool isPublic = false;
							bool isProtected = false;
							bool isStatic = false;

							if (getMethod != null)
							{
								isPublic = isPublic || getMethod.IsPublic;
								isProtected = isPublic || getMethod.IsFamily;
								isStatic = isStatic || getMethod.IsStatic;
							}

							if (setMethod != null)
							{
								isPublic = isPublic || setMethod.IsPublic;
								isProtected = isPublic || setMethod.IsFamily;
								isStatic = isStatic || setMethod.IsStatic;
							}

							if (!isPublic && !isProtected)
								continue;

							int propertyIcon = isPublic ? ImageProperty : ImagePProperty;

							var childChild = new System.Windows.Controls.TreeViewItem();
							//childChild.Header = property.Name;
							//childChild.Tag = property;
							childChild.Header = new Entity()
							{
								Name = property.Name,
								Info = property,
								Image = isPublic ? _ImageProperty : _ImagePProperty,
								Type = isPublic ? ImageProperty : ImagePProperty,
							};
							child.Items.Add(childChild);

							// Add a row to the table (if not overload)
							int col = 2;
							if (!isStatic)
								col = 1;

							if (propertiesRow == -1)
							{
								propertiesRow = table.AddRow();
								table[0, propertiesRow].ColumnSpan = 3;
								table[0, propertiesRow].Text = "Properties";
								table[0, propertiesRow].TextAlignment = TextAlignment.Left;
								table[0, propertiesRow].TextVerticalAlignment = AlignmentY.Center;
								table[0, propertiesRow].FontWeight = FontWeights.Bold;
								table.Rows[propertiesRow].Header = true;
								table.Rows[propertiesRow].Expanded = true;
							}

							string description = "";
							object[] attributes = null;
							try
							{
								attributes = property.GetCustomAttributes(false);
							}
							catch
							{
								attributes = new object[0];
							}
							foreach (Attribute attribute in attributes)
							{
								if (attribute is DescriptionAttribute)
								{
									description = (attribute as DescriptionAttribute).Description;
									break;
								}
							}

							int row = table.AddRow();
							if (!isStatic)
								table[col, row].ColumnSpan = 2;

							table[0, row].Image = (ImageSource)Application.Current.Resources["Property"];
							table[0, row].ImageAlign = ImageAlign.Center;
							table[col, row].Text = property.Name + " : " +
								property.PropertyType.Name;
							table[col, row].TextAlignment = TextAlignment.Left;
							table[col, row].TextVerticalAlignment = AlignmentY.Center;
							table[col, row].ToolTip = property.Name + " : " +
								property.PropertyType.Namespace + Type.Delimiter +
									property.PropertyType.Name;
							if (description.Length > 0)
								table[col, row].ToolTip +=  description;

							if (isStatic)
							{
								table[1, row].Image = (ImageSource)Application.Current.Resources["Static"];
								table[1, row].ImageAlign = ImageAlign.Center;
							}
						}

						// Enumerate events
						EventInfo[] events = type.GetEvents(
							BindingFlags.Instance | BindingFlags.Public |
								BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

						ArrayList sortedEvents = new ArrayList(events);
						sortedEvents.Sort(new MemberInfoComparer());

						int eventsRow = -1;

						foreach (EventInfo eventInfo in sortedEvents)
						{
							MethodInfo addMethod = eventInfo.GetAddMethod(true);
							MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);

							if (addMethod == null && removeMethod == null)
								continue;

							bool isPublic = false;
							bool isStatic = false;

							if (addMethod != null)
							{
								isPublic = isPublic || addMethod.IsPublic;
								isStatic = isStatic || addMethod.IsStatic;
							}

							if (removeMethod != null)
							{
								isPublic = isPublic || removeMethod.IsPublic;
								isStatic = isStatic || removeMethod.IsStatic;
							}

							if (!isPublic)
								continue;

							var childChild = new System.Windows.Controls.TreeViewItem();
							//childChild.Header = eventInfo.Name;
							//childChild.Tag = eventInfo;
							childChild.Header = new Entity()
							{
								Name = eventInfo.Name,
								Info = eventInfo,
								Image = _ImageEvent,
								Type = ImageEvent,
							};
							child.Items.Add(childChild);

							// Add a row to the table
							int col = 2;
							if (!isStatic)
								col = 1;

							if (eventsRow == -1)
							{
								eventsRow = table.AddRow();
								table[0, eventsRow].ColumnSpan = 3;
								table[0, eventsRow].Text = "Events";
								table[0, eventsRow].TextAlignment = TextAlignment.Left;
								table[0, eventsRow].TextVerticalAlignment = AlignmentY.Center;
								table[0, eventsRow].FontWeight = FontWeights.Bold;
								table.Rows[eventsRow].Header = true;
								table.Rows[eventsRow].Expanded = true;
							}

							string description = "";
							foreach (Attribute attribute in eventInfo.GetCustomAttributes(false))
							{
								if (attribute is DescriptionAttribute)
								{
									description = (attribute as DescriptionAttribute).Description;
									break;
								}
							}

							int row = table.AddRow();

							if (!isStatic)
								table[col, row].ColumnSpan = 2;

							table[0, row].Image = (ImageSource)Application.Current.Resources["Event"];
							table[0, row].ImageAlign = ImageAlign.Center;
							table[col, row].Text = eventInfo.Name;
							table[col, row].TextAlignment = TextAlignment.Left;
							table[col, row].TextVerticalAlignment = AlignmentY.Center;
							table[col, row].ToolTip = eventInfo.Name;

							if (isStatic)
							{
								table[1, row].Image = (ImageSource)Application.Current.Resources["Static"];
								table[1, row].ImageAlign = ImageAlign.Center;
							}
						}

						// Enumerate fields
						if (!type.IsInterface)
						{
							FieldInfo[] fields = type.GetFields(
								BindingFlags.Instance | BindingFlags.Public |
									BindingFlags.NonPublic |
										BindingFlags.Static | BindingFlags.DeclaredOnly);

							ArrayList sortedFields = new ArrayList(fields);
							sortedFields.Sort(new MemberInfoComparer());

							int fieldsRow = -1;

							foreach (FieldInfo field in sortedFields)
							{
								if (!field.IsPublic && !field.IsFamily)
									continue;

								if (field.IsSpecialName)
									continue;

								int fieldIcon = (field.IsPublic) ? ImageField : ImagePField;

								var childChild = new System.Windows.Controls.TreeViewItem();
								//childChild.Header = field.Name;
								//childChild.Tag = field;
								childChild.Header = new Entity()
								{
									Name = field.Name,
									Info = field,
									Image = (field.IsPublic) ? _ImageField : _ImagePField,
									Type = (field.IsPublic) ? ImageField : ImagePField,
								};
								child.Items.Add(childChild);

								// Add a row to the table
								int col = 2;
								if (!field.IsStatic)
									col = 1;

								if (fieldsRow == -1)
								{
									fieldsRow = table.AddRow();
									table[0, fieldsRow].ColumnSpan = 3;
									table[0, fieldsRow].Text = "Fields";
									table[0, fieldsRow].TextAlignment = TextAlignment.Left;
									table[0, fieldsRow].TextVerticalAlignment = AlignmentY.Center;
									table[0, fieldsRow].FontWeight = FontWeights.Bold;
									table.Rows[fieldsRow].Header = true;
									table.Rows[fieldsRow].Expanded = true;
								}

								int row = table.AddRow();

								if (!field.IsStatic)
									table[col, row].ColumnSpan = 2;

								table[0, row].Image = (ImageSource)Application.Current.Resources["Field"];
								table[0, row].ImageAlign = ImageAlign.Center;
								table[col, row].Text = field.Name + " : " +
									field.FieldType.Name;
								table[col, row].TextAlignment = TextAlignment.Left;
								table[col, row].TextVerticalAlignment = AlignmentY.Center;
								table[col, row].ToolTip = field.Name + " : " +
									field.FieldType.Namespace + Type.Delimiter +
										field.FieldType.Name;
								table[col, row].Text = field.Name;
								table[col, row].ToolTip = field.Name;

								if (field.IsStatic)
								{
									table[1, row].Image = (ImageSource)Application.Current.Resources["Static"];
									table[1, row].ImageAlign = ImageAlign.Center;
								}
							}
						}
					}
					break;

				case ImageEnum:
					{
						FieldInfo[] fields = type.GetFields(
							BindingFlags.Instance | BindingFlags.Public |
								BindingFlags.NonPublic |
									BindingFlags.Static | BindingFlags.DeclaredOnly);

						ArrayList sortedFields = new ArrayList(fields);
						sortedFields.Sort(new MemberInfoComparer());

						foreach (FieldInfo field in sortedFields)
						{
							// We skip private stuff
							if (field.IsPrivate)
								continue;

							// We skip internal stuff
							if (field.IsAssembly)
								continue;

							// Skip __value field
							if (field.IsSpecialName)
								continue;

							var childChild = new System.Windows.Controls.TreeViewItem();
							//childChild.Header = field.Name;
							//childChild.Tag = field;
							childChild.Header = new Entity()
							{
								Name = field.Name,
								Info = field,
								Image = _ImageEnumField,
								Type = ImageEnumField,
							};
							child.Items.Add(childChild);

							// Add a row to the table
							int row = table.AddRow();

							table[1, row].Image = (ImageSource)Application.Current.Resources["EnumField"];
							table[1, row].ImageAlign = ImageAlign.Center;
							table[2, row].Text = field.Name;
							table[2, row].TextAlignment = TextAlignment.Left;
							table[2, row].TextVerticalAlignment = AlignmentY.Center;
							table[2, row].ToolTip = field.Name + " (" + System.Convert.ToInt64(field.GetValue(null)) + ")";
						}
					}
					break;
			}

			return child;
		}


		/// <summary>
		/// Gets the count of non-collapsed row.
		/// </summary>
		private int GetRowCount(TableNode t)
		{
			int rowCount = 0;

			bool inCollapse = false;
			for (int r = 0; r < t.RowCount; r++)
			{
				if (!inCollapse)
					rowCount++;

				if (t.Rows[r].Header)
				{
					if (!t.Rows[r].Expanded)
						inCollapse = true;
					else
						inCollapse = false;
				}
			}

			return rowCount;
		}

		private void diagram_DrawNode(object sender, DrawNodeEventArgs e)
		{
			TableNode table = e.Node as TableNode;
			if (table == null)
				return;

			DrawingContext g = e.Graphics;

			//ImageSource image = _pureImages[(table.Tag as TreeNode).ImageIndex];
			//ImageSource image = (ImageSource)Application.Current.Resources["Event"];
			ImageSource image = (table.Tag as Entity).Image;
			g.DrawImage(image, new Rect(2 * coef, 2, image.Width, image.Height));			

			// Draw scroll arrows
			ImageSource arrow = (ImageSource)Application.Current.Resources["ArrowUp"];
			Point dest = new Point(table.Bounds.Width - arrow.Width - 2 * coef, 2);
			if (table.CurrentScrollRow > 0)
				g.DrawImage(arrow, new Rect(dest.X, dest.Y, arrow.Width, arrow.Height));

			arrow = (ImageSource)Application.Current.Resources["ArrowDown"];
			dest.Y += arrow.Height + 1;

			int visibleRows = (int)((table.Bounds.Height - 7) / table.RowHeight);
			if (table.CurrentScrollRow + visibleRows < GetRowCount(table))
				g.DrawImage(arrow, new Rect(dest.X, dest.Y, arrow.Width, arrow.Height));
		}

		private void diagram_NodeClicked(object sender, NodeEventArgs e)
		{
			TableNode table = e.Node as TableNode;
			if (table == null)
				return;

			// Check scroll arrows
			Rect bounds = table.Bounds;
			Point mousePos = e.MousePosition;
			mousePos.X -= bounds.X;
			mousePos.Y -= bounds.Y;
			bounds.Offset(-bounds.X, -bounds.Y);

			ImageSource arrow = (ImageSource)Application.Current.Resources["ArrowUp"];
			Rect imageBounds = new Rect(0, 0, arrow.Width, arrow.Height);


			Point dest = new Point(bounds.Width - imageBounds.Width - 2, bounds.Y + 2);

			if (table.CurrentScrollRow > 0)
			{
				if (new Rect(dest.X, dest.Y, imageBounds.Width, imageBounds.Height).Contains(mousePos))
					table.CurrentScrollRow--;
			}

			dest.Y += imageBounds.Height + 1;

			int visibleRows = (int)((bounds.Height - 7) / table.RowHeight);
			if (table.CurrentScrollRow + visibleRows < GetRowCount(table))
			{
				if (new Rect(dest.X, dest.Y, imageBounds.Width, imageBounds.Height).Contains(mousePos))
					table.CurrentScrollRow++;
			}
		}


		private Comparer DefaultComparer = new Comparer(
			CultureInfo.InvariantCulture);

		private Brush ClassBrush =
			new LinearGradientBrush(Colors.White, Colors.LightBlue, 45);

		private Brush StructBrush =
			new LinearGradientBrush(Colors.White, Colors.LightPink, 45);

		private Brush InterfaceBrush =
			new LinearGradientBrush(Colors.White, Colors.LightGray, 45);

		private Brush EnumBrush =
			new LinearGradientBrush(Colors.White, Color.FromArgb(255, 255, 255, 160), 45);

		private Brush DelegateBrush =
			new LinearGradientBrush(Colors.White, Colors.PaleGreen, 45);

		private const int ImageClass = 0;
		private const int ImageDelegate = 1;
		private const int ImageEnum = 2;
		private const int ImageEnumField = 3;
		private const int ImageEvent = 4;
		private const int ImageField = 5;
		private const int ImageInterface = 6;
		private const int ImageMethod = 7;
		private const int ImagePMethod = 8;
		private const int ImagePProperty = 9;
		private const int ImageProperty = 10;
		private const int ImageStatic = 11;
		private const int ImageStruct = 12;
		private const int ImageNamespace = 13;
		private const int ImagePField = 14;

		private ImageSource _ImageClass = Application.Current.Resources["Class"] as ImageSource;
		private ImageSource _ImageDelegate = Application.Current.Resources["Delegate"] as ImageSource;
		private ImageSource _ImageEnum = Application.Current.Resources["Enum"] as ImageSource;
		private ImageSource _ImageEnumField = Application.Current.Resources["EnumField"] as ImageSource;
		private ImageSource _ImageEvent = Application.Current.Resources["Event"] as ImageSource;
		private ImageSource _ImageField = Application.Current.Resources["ClasField"] as ImageSource;
		private ImageSource _ImageInterface = Application.Current.Resources["Interface"] as ImageSource;
		private ImageSource _ImageMethod = Application.Current.Resources["Method"] as ImageSource;
		private ImageSource _ImagePMethod = Application.Current.Resources["PMethod"] as ImageSource;
		private ImageSource _ImagePProperty = Application.Current.Resources["PProperty"] as ImageSource;
		private ImageSource _ImageProperty = Application.Current.Resources["Property"] as ImageSource;
		private ImageSource _ImageStatic = Application.Current.Resources["Static"] as ImageSource;
		private ImageSource _ImageStruct = Application.Current.Resources["Struct"] as ImageSource;
		private ImageSource _ImageNamespace = Application.Current.Resources["Namespace"] as ImageSource;
		private ImageSource _ImagePField = Application.Current.Resources["PField"] as ImageSource;

		private OpenFileDialog _openFileDialog;
		private Hashtable _workMap;

		private StringFormat LeftCenterFormat = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Center,
		};
	}

	class Entity
	{
		public string Name
		{
			get;
			set;
		}

		public ImageSource Image
		{
			get;
			set;
		}

		public MemberInfo Info
		{
			get;
			set;
		}

		public int Type
		{
			get;
			set;
		}
	}
}
