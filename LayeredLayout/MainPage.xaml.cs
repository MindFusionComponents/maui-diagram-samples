using MindFusion.Diagramming;

using SolidBrush = MindFusion.Drawing.SolidBrush;
using LinearGradientBrush = MindFusion.Drawing.LinearGradientBrush;


namespace LayeredLayout;

public partial class MainPage : ContentPage
{
	Diagram diagram;
	Random random = new Random();

	public MainPage()
	{
		InitializeComponent();

		diagram = diagramView.Diagram;

		diagram.ShapeNodeStyle.Brush = new LinearGradientBrush(
			new double[] { 0, 0.37, 0.86, 1 },
			new Color[]
			{
				Color.FromRgb(111, 127, 159),
				Color.FromRgb(207, 222, 255),
				Color.FromRgb(111, 127, 159),
				Color.FromRgb(47, 63, 111)
			},
			0);
		diagram.ShapeNodeStyle.Stroke = new SolidBrush(
			Color.FromRgb(97, 106, 127));
		diagram.DiagramLinkStyle.Stroke = new SolidBrush(
			Color.FromRgb(97, 106, 127));
	}

	void OnRandomClick(object sender, EventArgs e)
	{
		RandomGraph();
		Arrange();
	}

	private void Arrange()
	{
		MindFusion.Diagramming.Layout.LayeredLayout layout = new MindFusion.Diagramming.Layout.LayeredLayout();
		layout.Anchoring = Anchoring.Reassign;
		layout.EnforceLinkFlow = true;
		layout.StraightenLongLinks = true;
		layout.NodeDistance = 10;
		layout.LayerDistance = 15;
		layout.Arrange(diagram);

		diagram.ResizeToFitItems(5);
		diagramView.ZoomToFit();
	}

	private void RandomGraph()
	{
		diagram.ClearAll();

		for (int i = 0; i < 15; ++i)
		{
			int c = diagram.Nodes.Count;
			int g = 2 + random.Next(15);
			for (int j = 0; j < g; ++j)
			{
				ShapeNode node = diagram.Factory.CreateShapeNode(0, 0, 40, 40);
				node.AnchorPattern = AnchorPattern.TopInBottomOut;
				if (j > 0)
					diagram.Factory.CreateDiagramLink(diagram.Nodes[diagram.Nodes.Count - 2], node);
			}
			if (i > 0)
			{
				for (int j = 0; j < 1 + random.Next(3); ++j)
					diagram.Factory.CreateDiagramLink(
						diagram.Nodes[random.Next(c)],
						diagram.Nodes[c + random.Next(g)]);
			}
		}
	}
}
