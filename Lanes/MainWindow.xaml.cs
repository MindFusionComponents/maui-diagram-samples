//
// Copyright (c) 2022, MindFusion LLC - Bulgaria.
//

using System;
using System.Windows;
using System.Windows.Media;

using MindFusion.Diagramming.Wpf.Lanes;

using Grid = MindFusion.Diagramming.Wpf.Lanes.Grid;


namespace MindFusion.Diagramming.Wpf.Samples.CS.Lanes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Initialize flowchart
			diagram.AutoResize = AutoResize.None;
			var style = new System.Windows.Style();
			style.Setters.Add(new Setter(DiagramNode.BrushProperty, new SolidColorBrush(Color.FromRgb(0xCF, 0xBF, 0xEB))));
			diagram.ShapeNodeStyle = style;
            
            pattern = new AnchorPattern(new[] {
				new AnchorPoint(100, 50, false, true, MarkStyle.None),
				new AnchorPoint(0, 0, true, false, MarkStyle.None)
			});

            // Initialize grid
            Header header;
            Grid grid = diagram.LaneGrid;

            HeaderCollection columns = grid.ColumnHeaders;

            grid.MinHeaderSize = 30;
            //grid.HookHeaders = false;
            grid.HeadersOnTop = false;
            grid.ColumnHeadersHeights = new double[] { 30, 30 };
            grid.AlignCells = false;
            grid.AllowResizeHeaders = false;

            for (int i = 1; i <= 8; i++)
            {
                header = new Header(string.Format("Week {0}, {1}", i, DateTime.Now.Year));
                header.SubHeaders.Add(new Header("S"));
                header.SubHeaders.Add(new Header("M"));
                header.SubHeaders.Add(new Header("T"));
                header.SubHeaders.Add(new Header("W"));
                header.SubHeaders.Add(new Header("T"));
                header.SubHeaders.Add(new Header("F"));
                header.SubHeaders.Add(new Header("S"));
				header.SubHeaders[0].Style.BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xFE, 0xF8, 0xDA));
				header.SubHeaders[header.SubHeaders.Count - 1].Style.BackgroundBrush = new SolidColorBrush(Color.FromRgb(0xFE, 0xF8, 0xDA));
                columns.Add(header);
            }

            for (int i = 1; i <= 6; i++)
            {
                grid.RowHeaders.Add(new Header("Task " + i));
                grid.RowHeaders[i - 1].Width = 50;
            }

            int count = 8 * 7;
            for (int i = 0; i < count; i++)
            {
                Header column = grid.GetColumn(i);

                if (i % 7 != 0)
                {
                    grid[column, null].Style.LeftBorderPen = null;
                }
                else
                {
                    grid[column, null].Style.LeftBorderPen =
                        new Pen(
                            new LinearGradientBrush(Colors.Gray, Colors.Transparent, 0), 1);
                }

                if (i != count - 1)
                    grid[column, null].Style.RightBorderPen = null;

                if (i % 7 == 0 ||
                    i % 7 == 6)
                {
                    grid[column, null].Style.BackgroundBrush = Brushes.WhiteSmoke;
                }
            }

            count = 6;
            for (int i = 0; i < count; i++)
            {
                Header row = grid.GetRow(i);

                if (i == 0)
                {
                    grid[null, row].Style.TopBorderPen = null;
                }
                else
                {
                    grid[null, row].Style.TopBorderPen =
                        new Pen(new LinearGradientBrush(Colors.Gray, Colors.Gray, 0), 0.7);
                }

                if (i != count - 1)
                    grid[null, row].Style.BottomBorderPen = null;
            }

            diagram.EnableLanes = true;

            // Ensure the document is big enough to contain the grid
			var rect = grid.GetColumnHeaderBounds();
			rect.Union(grid.GetRowHeaderBounds());
			diagram.Bounds = rect;
            diagram.AlignToGrid = false;
        }

        private AnchorPattern pattern;

        /// <summary>
        /// Gets the size of a pixel measured in document units.
        /// </summary>
        private double GetPixel()
        {
            Point pt1 = diagramView.ViewToDiagram(new Point(0, 0));
            Point pt2 = diagramView.ViewToDiagram(new Point(1, 1));

            return pt2.X - pt1.X;
        }

        /// <summary>
        /// Fixes the segments of the specified link according
        /// to the position of its related boxes.
        /// </summary>
        private static void FixLink(DiagramLink link)
        {
            Rect originBounds = link.Origin.Bounds;
            Rect destBounds = link.Destination.Bounds;

            if (originBounds.Right < destBounds.Left)
            {
                link.SegmentCount = 2;
            }
            else
            {
                link.SegmentCount = 4;

                Point pt0 = link.ControlPoints[0];
                Point pt1;
                Point pt2;
                Point pt3 = link.ControlPoints[3];
                Point pt4 = link.ControlPoints[4];

                pt1 = pt0;
                pt1.X += 6;

                pt2 = pt1;
                pt2.Y += 15;

                pt3.X = pt4.X;
                pt3.Y = pt2.Y;

                link.ControlPoints[1] = pt1;
                link.ControlPoints[2] = pt2;
                link.ControlPoints[3] = pt3;

                link.UpdateFromPoints();
            }
        }

        private void diagram_HitTestAdjustmentHandles(object sender, HitTestEventArgs e)
        {
            if (!(e.Item is ShapeNode))
            {
                e.HitResult = null;
                return;
            }

            var node = e.Item as ShapeNode;

            if (!node.Bounds.Contains(e.MousePosition))
            {
				e.HitResult = null;
                return;
            }

            Rect left = node.Bounds;
            left.Width = 2;

            if (left.Contains(e.MousePosition))
            {
				e.HitResult = new NodeAdjustmentHandle(NodeHandleType.ResizeMiddleLeft);
                return;
            }

            Rect right = node.Bounds;
            right.X = right.Right - 2;
            right.Width = 2;

            if (right.Contains(e.MousePosition))
            {
				e.HitResult = new NodeAdjustmentHandle(NodeHandleType.ResizeMiddleRight);
                return;
            }

            Rect middle = node.Bounds;
            middle.X = middle.X + middle.Width / 4;
            middle.Width = middle.Width / 2;

            if (middle.Contains(e.MousePosition))
            {
				e.HitResult = new NodeAdjustmentHandle(NodeHandleType.Move);
                return;
            }

			e.HitResult = null;
        }

        private void diagram_NodeCreated(object sender, NodeEventArgs e)
        {
            e.Node.HandlesStyle = HandlesStyle.Custom;
            e.Node.AnchorPattern = pattern;

            // Place the box within the grid
            Rect bounds = e.Node.Bounds;
            Point topLeft = new Point(bounds.Left, bounds.Top);

            Grid grid = diagram.LaneGrid;

            Rect cellBounds = Rect.Empty;
            grid.GetCellFromPoint(topLeft, ref cellBounds);

            double pixel = GetPixel();

            bounds.Y = cellBounds.Y + 4 * pixel;
            bounds.Height = cellBounds.Height - 10 * pixel;
            e.Node.Bounds = bounds;
        }

        private void diagram_NodeCreating(object sender, NodeValidationEventArgs e)
        {
            Rect bounds = e.Node.Bounds;
            Point topLeft = new Point(bounds.Left, bounds.Top);

            Grid grid = diagram.LaneGrid;

            Rect cellBounds = Rect.Empty;
            ICell cell = grid.GetCellFromPoint(topLeft, ref cellBounds);

            if (cell == null)
                e.Cancel = true;
        }

        private void diagram_SelectionChanged(object sender, EventArgs e)
        {
            foreach (ShapeNode node in diagram.Nodes)
            {
                node.Stroke = Brushes.Black;
            }

            foreach (DiagramLink link in diagram.Links)
            {
                link.Stroke = Brushes.Black;
                link.HeadStroke = Brushes.Black;
                link.Brush = Brushes.Black;
            }

            foreach (ShapeNode node in diagram.Selection.Nodes)
            {
                node.Stroke = Brushes.Red;
            }

            foreach (DiagramLink link in diagram.Selection.Links)
            {
                link.Stroke = Brushes.Red;
                link.HeadStroke = Brushes.Red;
                link.Brush = Brushes.Red;
            }
        }

        private void diagram_NodeModifying(object sender, NodeValidationEventArgs e)
        {
            Rect bounds = e.Node.Bounds;
            Point topLeft = new Point(bounds.Left, bounds.Top + bounds.Height / 2);

            Grid grid = diagram.LaneGrid;

            Rect cellBounds = Rect.Empty;
            ICell cell = grid.GetCellFromPoint(topLeft, ref cellBounds);

            if (cell == null || cellBounds.IsEmpty)
                e.Cancel = true;
        }

		private void diagram_SelectionModifying(object sender, SelectionValidationEventArgs e)
		{
			Rect bounds = diagram.Selection.Bounds;

			var gridBounds = diagram.LaneGrid.GetCellBounds(diagram.LaneGrid[null, null]);
			if (!gridBounds.Contains(bounds))
				e.Cancel = true;
		}

        private void diagram_LinkCreated(object sender, LinkEventArgs e)
        {
            e.Link.AllowMoveStart = false;
            e.Link.AllowMoveEnd = false;
            e.Link.HandlesStyle = HandlesStyle.Custom;

            FixLink(e.Link);
        }

        private void diagram_NodeModified(object sender, NodeEventArgs e)
        {
            // Place the box within the grid
            Rect bounds = e.Node.Bounds;
            Point topLeft = new Point(bounds.Left, bounds.Top + bounds.Height / 2);

            Grid grid = diagram.LaneGrid;

            Rect cellBounds = Rect.Empty;
            grid.GetCellFromPoint(topLeft, ref cellBounds);
			if (cellBounds.IsEmpty)
				return;

            double pixel = GetPixel();

            bounds.Y = cellBounds.Y + 4 * pixel;
            bounds.Height = cellBounds.Height - 10 * pixel;
            e.Node.Bounds = bounds;

            // Fix links
        	var links = e.Node.GetAllLinks();
            foreach (DiagramLink link in links)
                FixLink(link);
        }
    }
}