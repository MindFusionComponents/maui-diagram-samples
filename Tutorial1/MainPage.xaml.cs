using System.Reflection;
using System.Xml.Linq;

using MindFusion.Diagramming;
using MindFusion.Diagramming.Layout;

namespace Tutorial1;


public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		Diagram diagram = diagramView.Diagram;

		var nodeMap = new Dictionary<string, DiagramNode>();
		var bounds = new Rect(0, 0, 18, 6);

		// Load the graph xml
		var assembly = typeof(App).GetTypeInfo().Assembly;
		Stream stream = assembly.GetManifestResourceStream("Tutorial1.SampleGraph.xml");

		string text;
		using (var reader = new StreamReader(stream))
		{
			text = reader.ReadToEnd();
		}
		XDocument document = XDocument.Parse(text);

		// load node data
		var nodes = document.Descendants("Node");
		foreach (var node in nodes)
		{
			var diagramNode = diagram.Factory.CreateShapeNode(bounds);
			nodeMap[node.Attribute("id").Value] = diagramNode;
			diagramNode.Text = node.Attribute("name").Value;
		}

		// load link data
		var links = document.Descendants("Link");
		foreach (var link in links)
		{
			diagram.Factory.CreateDiagramLink(
				nodeMap[link.Attribute("origin").Value],
				nodeMap[link.Attribute("target").Value]).
				HeadShapeSize = 2;
		}

		// arrange the graph
		var layout = new LayeredLayout();
		layout.LayerDistance = 12;
		layout.Arrange(diagram);
	}
}
