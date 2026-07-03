using MindFusion.Diagramming;
using MindFusion.Drawing;


namespace NodeList;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		var diagram = diagramView.Diagram;
		diagram.Bounds = new Rect(0, 0, 2000, 2000);
		diagram.RouteLinks = true;
		diagram.RoundedLinks = true;
		diagram.AutoSnapLinks = true;
		diagram.NodeCreated +=
			(s, e) => e.Node.AnchorPattern = AnchorPattern.Decision2In2Out;

		nodeListView.Target = diagramView;
		nodeListView.IconSize = new Size(32, 32);

		var nodeRect = new Rect(0, 0, 64, 64);

		nodeListView.AddNode(
			new ShapeNode { Shape = Shapes.Rectangle, Bounds = nodeRect }, "Process");

		nodeListView.AddNode(
			new ShapeNode { Shape = Shapes.Ellipse, Bounds = nodeRect }, "Task");

		nodeListView.AddNode(
			new ShapeNode { Shape = Shapes.Decision, Bounds = nodeRect }, "Decision");

		nodeListView.AddNode(
			new ContainerNode { Bounds = nodeRect }, "Container");

		nodeListView.AddNode(
			new TableNode
			{
				Caption = "Grid",
				RowCount = 12, ColumnCount = 2,
				Bounds = nodeRect,
				Brush = Brushes.White,
				CellFrameStyle = CellFrameStyle.Simple,
				Scrollable = true
			},
			"Grid");

		var treeNode = new TreeViewNode();
		treeNode.Bounds = nodeRect;
		for (int i = 0; i < 4; i++)
		{
			var row = new TreeViewItem("row " + i);
			treeNode.RootItems.Add(row);

			for (int j = 0; j < 3; j++)
				row.Children.Add(new TreeViewItem("sub " + j));
		}
		nodeListView.AddNode(treeNode, "Tree");
	}
}
