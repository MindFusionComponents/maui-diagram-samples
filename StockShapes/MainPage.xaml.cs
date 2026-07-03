using MindFusion.Diagramming;
using MindFusion.Drawing;
using System.Diagnostics;
using Behavior = MindFusion.Diagramming.Behavior;


namespace StockShapes;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		Diagram diagram = diagramView.Diagram;
		diagram.SelectAfterCreate = false;
		diagramView.Behavior = Behavior.PanAndModify;

		// set colors
		diagram.BackBrush = new SolidBrush(Color.FromRgb(248, 250, 252));
        diagram.DiagramStyle.ShadowBrush = new SolidBrush(
			Color.FromRgba(0, 0, 0, 20));
		diagram.DiagramStyle.NodeEffects.Add(new GlassEffect());
		diagram.DiagramStyle.FontFamily = "Tahoma";
		diagram.DiagramStyle.FontSize = 3;

		int i = 0;
		double width = 18;
		double height = 18;
		double xOffset = 16;
		double yOffset = 16;
		float labelOffset = 8;
		int perLine = NodesPerLine();

		// enum all predefined shapes
		foreach (Shape shape in Shape.Shapes)
		{
			// skip arrowheads
			if (shape.Outline.Length == 0 ||
				shape == ArrowHeads.RevWithCirc ||
				shape == ArrowHeads.DoubleArrow)
				continue;

			// create a node showing this shape
			ShapeNode node = diagram.Factory.CreateShapeNode(
				(i % perLine) * (width + xOffset) + xOffset,
				(i / perLine) * (height + yOffset) + yOffset,
				width, height, shape);

			node.Pen = new Pen(Colors.Black, 0);
			node.Brush = new SolidBrush(Color.FromRgb(254, 179, 45));

			// add a lebel to display shape's identifier
			var labelText = shape.Id.StartsWith("Bpmn") ?
				"Bpmn\n" + shape.Id.Substring(4) : shape.Id;

			var label = new NodeLabel(node, labelText);
			label.SetEdgePosition(
				2, // bottom edge
				0, labelOffset);
			node.AddLabel(label);

			i = i + 1;
		}

		diagram.ResizeToFitItems(4, true);
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);
		Zoom();
	}

	protected override Size ArrangeOverride(Rect bounds)
    {
        var size = base.ArrangeOverride(bounds);
		Zoom();
		return size;
    }

	void Zoom()
	{
		if (diagramView.Viewport.Width > 0 && !zoomed)
		{
			diagramView.ZoomFactor = 100.0 *
				diagramView.Viewport.Width /
				diagramView.Diagram.Bounds.Width - 5;

			zoomed = true;
		}
	}

	int NodesPerLine()
	{
		if (DeviceInfo.Current.Idiom == DeviceIdiom.Desktop)
			return 8;
		if (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet)
			return 6;
		if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
			return 3;

		return 8;
	}

	bool zoomed = false;
}
