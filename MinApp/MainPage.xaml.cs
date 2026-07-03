using MindFusion.Diagramming;

namespace MinApp;


public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		var diagram = diagramView.Diagram;
		diagram.AutoResize = AutoResize.AllDirections;

		var node1 = diagram.Factory.CreateShapeNode(10, 10, 30, 30);
		node1.Text = "Hello";

		var node2 = diagram.Factory.CreateShapeNode(60, 25, 30, 30);
		node2.Text = ".NET MAUI";

		diagram.Factory.CreateDiagramLink(node1, node2);
	}
}
