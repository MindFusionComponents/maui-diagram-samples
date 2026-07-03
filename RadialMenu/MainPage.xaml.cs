using Microsoft.Maui.Controls;
using MindFusion.Diagramming;
using MindFusion.Drawing;


#if IOS || ANDROID || MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#elif WINDOWS
using Microsoft.Maui.Graphics.Win2D;
#endif


using IImage = Microsoft.Maui.Graphics.IImage;


namespace RadialMenu;

public partial class MainPage : ContentPage
{
	Diagram diagram;
	IImage rectImage;
	IImage ellipseImage;

	public MainPage()
	{
		InitializeComponent();

		Appearing += OnAppearing;

		diagram = diagramView.Diagram;

		var node = diagram.Factory.CreateShapeNode(10, 10, 50, 40);
		node.Text =
			"This sample demonstrates how to customize item's context menu. \n" +
			"Press and hold to display the menu.";

		diagramView.RadialMenuCreated += OnRadialMenuCreated;
	}

	void OnAppearing(object sender, EventArgs e)
	{
		if (rectImage != null)
			return;

		var iconSize = new Rect(0, 0, 32, 32);

		// set up a temporary diagram to export shape images
		var imageGen = new Diagram();
		imageGen.BackBrush = Brushes.Transparent;
		imageGen.MeasureUnit = MeasureUnit.Pixel;
		imageGen.AutoResize = AutoResize.None;
		imageGen.ShadowsStyle = ShadowsStyle.None;

		// node that will render shapes
		var node = imageGen.Factory.CreateShapeNode(
			iconSize.Inflate(-1, -1));
		node.Brush = Brushes.White;

		// export a rectangle icon
		node.Shape = Shapes.Rectangle;
		rectImage = imageGen.CreateImage(
			iconSize, 100, false);

		// export an ellipse icon
		node.Shape = Shapes.Ellipse;
		ellipseImage = imageGen.CreateImage(
			iconSize, 100, false);
	}

	void OnRadialMenuCreated(object sender, RadialMenuEventArgs e)
	{
		// remove the toggle-selection item
		e.Menu.RemoveMenuItem(e.Menu.Children[2] as RadialMenuItem);

		// add change-shape items
		var shapeNode = e.Target as ShapeNode;
		if (shapeNode != null)
		{
			var rectItem = e.Menu.AddMenuItem(rectImage);
			rectItem.Clicked +=
				(s, args) => shapeNode.Shape = Shapes.Rectangle;

			var ellipseItem = e.Menu.AddMenuItem(ellipseImage);
			ellipseItem.Clicked +=
				(s, args) => shapeNode.Shape = Shapes.Ellipse;
		}

		var node = e.Target as DiagramNode;
		if (node != null)
		{
			// add clone item
			var cloneIcon = LoadImage("add.png");
			var cloneItem = e.Menu.AddMenuItem(cloneIcon);
			cloneItem.Clicked += (s, args) =>
			{
				var clone = (DiagramNode)node.Clone(false);
				clone.Bounds = node.Bounds.Offset(10, 10);
				diagram.Nodes.Add(clone);
			};
		}

		// add lock item
		var lockIcon = LoadImage("lock.png");
		var lockItem = e.Menu.AddMenuItem(lockIcon);
		lockItem.Clicked += (s, args) =>
		{
			args.Item.Brush = Brushes.Red;
			args.Item.Locked = true;
		};

		IImage LoadImage(string resourceName)
		{
			IImage image = null;

			var assembly = GetType().Assembly;
			using (var stream = assembly.GetManifestResourceStream(
				"RadialMenu.Resources.Images." + resourceName))
			{
#if IOS || ANDROID || MACCATALYST
				// PlatformImage isn't currently supported on Windows.
				image = PlatformImage.FromStream(stream);
#elif WINDOWS
				image = new W2DImageLoadingService().FromStream(stream);
#endif
			}

			return image;
		}
	}
}
