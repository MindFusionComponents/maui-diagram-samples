using MindFusion.Diagramming;

namespace FreeForm;


public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		diagramView.Diagram.FreeFormTargets = new[]
		{
			Shapes.Rectangle,
			Shapes.Ellipse,
			Shapes.Decision
		};
	}
}
