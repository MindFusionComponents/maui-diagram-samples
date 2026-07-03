using MindFusion.Diagramming;

namespace StressTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

		void OnCreateItems(object sender, EventArgs e)
		{
			var diagram = new Diagram();

			diagram.DefaultShape = Shapes.Rectangle;
			diagram.ValidityChecks = false;
			diagram.AutoResize = AutoResize.None;
			diagram.SelectionOnTop = false;
			diagram.SelectAfterCreate = false;

			var total = 10000;
			var rowSize = (int)Math.Sqrt(total);
			var size = 20;
			var dist = 30;

			var diagSize = rowSize * dist;
			diagram.Bounds = new Rect(
				0, 0, diagSize, diagSize);

			var x = diagram.Bounds.Left;
			var y = diagram.Bounds.Top;
			for (int i = 0; i < total; i++)
			{
				var node = diagram.Factory.CreateShapeNode(x, y, size, size);
				node.Text = i.ToString();
				if (x > diagram.Bounds.Left)
				{
					diagram.Factory.CreateDiagramLink(
						diagram.Nodes[i - 1], diagram.Nodes[i]);
				}

				x += dist;
				if (x >= diagram.Bounds.Right)
				{
					x = diagram.Bounds.Left;
					y += dist;
				}
			}

			diagram.EnableSpatialIndex = true;
			diagramView.Diagram = diagram;
		}
	}
}
