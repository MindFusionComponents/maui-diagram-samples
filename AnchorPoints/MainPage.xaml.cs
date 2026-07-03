using MindFusion.Diagramming;
using MindFusion.Drawing;

using Colors = Microsoft.Maui.Graphics.Colors;
using LinearGradientBrush = MindFusion.Drawing.LinearGradientBrush;


namespace AnchorPoints;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		var diagram = diagramView.Diagram;
		diagram.AlignToGrid = false;
		diagram.AllowSelfLoops = false;
		diagram.LinkShape = LinkShape.Cascading;
		diagram.LinkSegments = 3;
		diagram.RouteLinks = true;
		diagram.SelectAfterCreate = false;
		diagram.ShapeHandlesStyle = HandlesStyle.DashFrame;
		diagram.SnapToAnchor = SnapToAnchor.OnCreateOrModify;
		diagram.ShowAnchors = ShowAnchors.Always;
		diagram.NodeEffects.Add(new GlassEffect());

		diagram.DiagramStyle.ShadowBrush = new SolidBrush(
			Color.FromArgb("#96BEBEBE"));
		diagram.DiagramLinkStyle.Stroke = new SolidBrush(
			Color.FromRgb(100, 100, 100));
		diagram.DiagramLinkStyle.HeadStroke = new SolidBrush(
			Color.FromRgb(100, 100, 100));

		var pattern1 = new AnchorPattern(new AnchorPoint[]
		{
			new AnchorPoint(50, 0, true, true),
			new AnchorPoint(100, 50, true, true),
			new AnchorPoint(50, 100, true, true),
			new AnchorPoint(0, 50, true, true)
		});

		var pattern2 = new AnchorPattern(new AnchorPoint[]
		{
			new AnchorPoint(10, 0, true, false, MarkStyle.Circle, Colors.RoyalBlue),
			new AnchorPoint(50, 0, true, false, MarkStyle.Circle, Colors.Blue),
			new AnchorPoint(90, 0, true, false, MarkStyle.Circle, Colors.Firebrick),
			new AnchorPoint(10, 100, false, true, MarkStyle.Rectangle),
			new AnchorPoint(50, 100, false, true, MarkStyle.Rectangle),
			new AnchorPoint(90, 100, false, true, MarkStyle.Rectangle),
			new AnchorPoint(0, 50, true, true, MarkStyle.Custom)
		});

		var pb1 = new ShapeNode(diagram);
		pb1.Bounds = new Rect(10, 7, 25, 18);
		pb1.Shape = Shapes.Ellipse;
		pb1.Text = "Start";
		pb1.AnchorPattern = pattern1;
		diagram.Nodes.Add(pb1);

		var pb2 = new ShapeNode(diagram);
		pb2.Bounds = new Rect(20, 75, 25, 18);
		pb2.Text = "node 1";
		pb2.AnchorPattern = pattern2;
		diagram.Nodes.Add(pb2);

		var pb3 = new ShapeNode(diagram);
		pb3.Bounds = new Rect(70, 70, 25, 18);
		pb3.Text = "node 2";
		pb3.AnchorPattern = pattern2;
		diagram.Nodes.Add(pb3);

		var pb4 = new ShapeNode(diagram);
		pb4.Bounds = new Rect(80, 100, 25, 18);
		pb4.Shape = Shapes.Ellipse;
		pb4.Text = "End";
		pb4.AnchorPattern = pattern1;
		diagram.Nodes.Add(pb4);

		var decb1 = new ShapeNode(diagram);
		decb1.Bounds = new Rect(20, 35, 30, 20);
		decb1.Shape = Shapes.Decision;
		decb1.Text = "check 1";
		decb1.AnchorPattern = AnchorPattern.Decision1In3Out;
		diagram.Nodes.Add(decb1);

		var decb2 = new ShapeNode(diagram);
		decb2.Bounds = new Rect(70, 30, 30, 20);
		decb2.Shape = Shapes.Decision;
		decb2.Text = "check 2";
		decb2.AnchorPattern = AnchorPattern.Decision2In2Out;
		diagram.Nodes.Add(decb2);

		diagram.Links.Add(
			new DiagramLink(diagram, decb1, decb2));

		diagram.NodeCreated +=
			(s, e) => e.Node.AnchorPattern = pattern2;

		diagram.DrawAnchorPoint +=
			(s, e) => e.Graphics.DrawPolygon(
				new Pen(Colors.Red, 0), new[]
				{
					e.Location.Offset(0, -1),
					e.Location.Offset(1, 0),
					e.Location.Offset(0, 1),
					e.Location.Offset(-1, 0)
				});
	}
}
