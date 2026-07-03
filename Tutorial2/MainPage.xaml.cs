using MindFusion.Diagramming;
using MindFusion.Diagramming.Layout;
using System.Reflection;
using System.Xml.Linq;

namespace Tutorial2;


public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		diagram = diagramView.Diagram;

		var root = diagram.Factory.CreateShapeNode(bounds);
		root.Text = "Project";

		// Load the graph xml
		var assembly = typeof(App).GetTypeInfo().Assembly;
		Stream stream = assembly.GetManifestResourceStream("Tutorial2.SampleTree.xml");

		string text;
		using (var reader = new StreamReader(stream))
		{
			text = reader.ReadToEnd();
		}

		XDocument document = XDocument.Parse(text);
		CreateChildren(root, document.Root);

		var layout = new TreeLayout();
		layout.Type = TreeLayoutType.Cascading;
		layout.Direction = TreeLayoutDirections.LeftToRight;
		layout.LinkStyle = TreeLayoutLinkType.Cascading2;
		layout.NodeDistance = 3;
		layout.LevelDistance = -8;
		layout.Arrange(diagram);
	}

	void CreateChildren(DiagramNode parentDiagNode, XElement parentXmlNode)
	{
		var activities = parentXmlNode.Elements();
		foreach (var activity in activities)
		{
			if (activity.Name == "Activity")
			{
				var node = diagram.Factory.CreateShapeNode(bounds);
				node.Text = activity.Attribute(XName.Get("Name")).Value;
				diagram.Factory.CreateDiagramLink(parentDiagNode, node).HeadShapeSize = 2;
				CreateChildren(node, activity);
			}
		}
	}

	Rect bounds = new Rect(0, 0, 24, 6);
	Diagram diagram;
}
