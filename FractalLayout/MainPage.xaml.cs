using MindFusion.Diagramming;

using Colors = Microsoft.Maui.Graphics.Colors;
using Brush = MindFusion.Drawing.Brush;
using LinearGradientBrush = MindFusion.Drawing.LinearGradientBrush;


namespace FractalLayout;

public partial class MainPage : ContentPage
{
	Diagram diagram;
	readonly Rect bounds;
	readonly Brush[] brushes;
	readonly Random random;

	public MainPage()
	{
		InitializeComponent();

		diagram = diagramView.Diagram;

		bounds = new Rect(0, 0, 10, 8);

		brushes = new Brush[] {
				new LinearGradientBrush(Colors.LightSteelBlue, Colors.BlueViolet, 0),
				new LinearGradientBrush(Colors.White, Colors.LightBlue, 0),
				new LinearGradientBrush(Colors.White, Colors.DeepSkyBlue, 0),
				new LinearGradientBrush(Colors.LimeGreen, Colors.Green, 0)
			};

		random = new Random();
	}

	void OnRandomClick(object sender, EventArgs e)
	{
		diagram.ClearAll();

		ShapeNode root = diagram.Factory.CreateShapeNode(bounds);
		RandomTree(root, 4, 4);
		Arrange(root);
	}

	void Arrange(DiagramNode root)
	{
		var layout = new MindFusion.Diagramming.Layout.FractalLayout();
		layout.Root = root;
		layout.Arrange(diagram);
		diagram.ResizeToFitItems(0);
		diagramView.ZoomToFit();
	}

	void RandomTree(DiagramNode node, int depth, int minChildren)
	{
		if (depth <= 0)
			return;

		Diagram diagram = node.Parent;
		int children = random.Next(depth) - 1 + minChildren;

		if (diagram.Nodes.Count < 3 && children < 2)
			children = 2;

		for (int i = 0; i < children; ++i)
		{
			// create child node and link
			ShapeNode child = diagram.Factory.CreateShapeNode(bounds);
			child.Brush = brushes[depth % brushes.Length];
			diagram.Factory.CreateDiagramLink(node, child)
				.HeadShape = null;

			// build child branch
			RandomTree(child, depth - 1, minChildren);
		}
	}
}
