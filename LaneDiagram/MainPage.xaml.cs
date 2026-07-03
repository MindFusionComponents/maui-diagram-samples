using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using MindFusion.Diagramming;
using MindFusion.Diagramming.Lanes;
using MindFusion.Drawing;

using SolidBrush = MindFusion.Drawing.SolidBrush;
using Pen = MindFusion.Drawing.Pen;
using LinearGradientBrush = MindFusion.Drawing.LinearGradientBrush;

namespace LaneDiagram;

public partial class MainPage : ContentPage
{
	private Diagram diagram;

	public MainPage()
	{
		InitializeComponent();

		diagram = diagramView.Diagram;
		diagram.SelectAfterCreate = false;

		// Set diagram canvas properties for high-quality landscape format
		diagram.Bounds = new Rect(0, 0, 180, 110);
		diagram.BackBrush = new SolidBrush(Color.FromRgb(248, 250, 252)); // modern soft light slate gray
		diagram.DiagramStyle.ShadowBrush = new SolidBrush(Color.FromRgba(15, 23, 42, 6)); // Soft micro-shadow
		diagram.DiagramStyle.FontFamily = "OpenSansRegular";
		diagram.DiagramStyle.FontSize = 2.2f;

		// Configure links layout default (Horizontal cascade for swimlanes)
		diagram.RouteLinks = true;
		//diagram.RoundedLinks = true;
		diagram.AutoSnapLinks = true;
		diagram.LinkCascadeOrientation = MindFusion.Diagramming.Orientation.Horizontal;
		diagram.LinkSegments = 2;
		diagram.LinkShape = LinkShape.Cascading;

		// Wire up snapping events
		diagram.NodeCreated += OnNodeCreated;
		diagram.NodeModified += OnNodeModified;

		// initialize the lane grid
		MindFusion.Diagramming.Lanes.Grid grid = diagram.LaneGrid;
		grid.MinHeaderSize = 35;
		grid.ColumnHeadersHeights = new double[] { 15 };
		grid.AlignCells = true;

		// Add Column Headers (Phases)
		grid.ColumnHeaders.Add(new Header("Phase 1: Research"));
		grid.ColumnHeaders.Add(new Header("Phase 2: Design"));
		grid.ColumnHeaders.Add(new Header("Phase 3: Coding"));
		grid.ColumnHeaders.Add(new Header("Phase 4: Release"));

		// Add Row Headers (Teams)
		grid.RowHeaders.Add(new Header("Core Engine"));
		grid.RowHeaders.Add(new Header("UI Frontend"));
		grid.RowHeaders.Add(new Header("Cloud Support"));

		// Set Row Header widths
		grid.RowHeaders[0].Width = 30;
		grid.RowHeaders[1].Width = 30;
		grid.RowHeaders[2].Width = 30;

		// Enable Swimlanes
		diagram.EnableLanes = true;

		// Adjust document bounds to perfectly encapsulate column and row headers
		var rect = grid.GetColumnHeaderBounds();
		rect.Union(grid.GetRowHeaderBounds());
		diagram.Bounds = rect;

        // add some styling to headers
        var headerBrush = new SolidBrush(Color.FromRgb(241, 245, 249)); // Slate 100
        var headerPen = new Pen(Color.FromRgb(203, 213, 225), 0.4); // Slate 300

        foreach (var header in grid.ColumnHeaders)
        {
            header.Style.BackgroundBrush = headerBrush;
            header.Style.LeftBorderPen = headerPen;
            header.Style.RightBorderPen = headerPen;
            header.Style.TopBorderPen = headerPen;
            header.Style.BottomBorderPen = headerPen;
        }

        foreach (var header in grid.RowHeaders)
        {
            header.Style.BackgroundBrush = headerBrush;
            header.Style.LeftBorderPen = headerPen;
            header.Style.RightBorderPen = headerPen;
            header.Style.TopBorderPen = headerPen;
            header.Style.BottomBorderPen = headerPen;
        }

        // colour the headers in modern colours
        grid[grid.ColumnHeaders[0], null].Style.BackgroundBrush = new SolidBrush(Color.FromRgb(241, 245, 249)); // Soft Slate/Grey
        grid[grid.ColumnHeaders[1], null].Style.BackgroundBrush = new SolidBrush(Color.FromRgb(245, 243, 255)); // Soft Purple/Lavender
        grid[grid.ColumnHeaders[2], null].Style.BackgroundBrush = new SolidBrush(Color.FromRgb(240, 253, 244)); // Soft Green/Mint
        grid[grid.ColumnHeaders[3], null].Style.BackgroundBrush = new SolidBrush(Color.FromRgb(255, 251, 235)); // Soft Amber/Honey


        // add nodes to lanes
        var nodes = new Dictionary<string, ShapeNode>();

		// assistant method to create nodes
		ShapeNode CreateLaneNode(string text, double x, double y, Color stroke, Color fill)
		{
			var node = diagram.Factory.CreateShapeNode(x, y, 24, 12);
			node.Shape = Shapes.RoundRect;
			node.Pen = new Pen(stroke, 0.4);
			node.Brush = new SolidBrush(fill);
			node.Text = text;
			node.TextBrush = new SolidBrush(Color.FromRgb(15, 23, 42)); // Slate 900
			node.Font = Microsoft.Maui.Font.OfSize("OpenSansSemibold", 2.0);
			return node;
		}

		Color blueStroke = Color.FromRgb(30, 58, 138); Color blueFill = Color.FromRgb(219, 234, 254);
		Color tealStroke = Color.FromRgb(15, 118, 110); Color tealFill = Color.FromRgb(204, 251, 241);
		Color purpleStroke = Color.FromRgb(91, 33, 182); Color purpleFill = Color.FromRgb(243, 232, 255);

		// X and Y are calculated to center nodes perfectly within respective cells
		nodes["MarketStudy"] = CreateLaneNode("Market Study", 35, 25, blueStroke, blueFill);     // Phase 1, Core
		nodes["Wireframe"] = CreateLaneNode("UI Wireframe", 70, 55, tealStroke, tealFill);      // Phase 2, UI
		nodes["DbSchema"] = CreateLaneNode("DB Schema", 70, 95, purpleStroke, purpleFill);      // Phase 2, Cloud
		nodes["AlgoDev"] = CreateLaneNode("Algorithm Dev", 105, 25, blueStroke, blueFill);     // Phase 3, Core
		nodes["ApiSupport"] = CreateLaneNode("API Support", 105, 95, purpleStroke, purpleFill);  // Phase 3, Cloud
		nodes["AppLaunch"] = CreateLaneNode("App Launch", 140, 55, tealStroke, tealFill);       // Phase 4, UI

		// connect nodes with links
		void CreateProcessLink(ShapeNode from, ShapeNode to)
		{
			var link = diagram.Factory.CreateDiagramLink(from, to);
			link.Pen = new Pen(Color.FromRgb(148, 163, 184), 0.4); // Slate 400
			link.HeadShapeSize = 1.8;
		}

		CreateProcessLink(nodes["MarketStudy"], nodes["Wireframe"]);
		CreateProcessLink(nodes["MarketStudy"], nodes["DbSchema"]);
		CreateProcessLink(nodes["Wireframe"], nodes["AlgoDev"]);
		CreateProcessLink(nodes["DbSchema"], nodes["ApiSupport"]);
		CreateProcessLink(nodes["AlgoDev"], nodes["AppLaunch"]);
		CreateProcessLink(nodes["ApiSupport"], nodes["AppLaunch"]);
	}

	// Dynamic Snapping logic to snap dropped/moved nodes perfectly inside cells
	private void SnapToCell(DiagramNode node)
	{
		var grid = diagram.LaneGrid;
		Rect cellBounds = new Rect();

		// Use the center point of the node to identify the swimlane cell
		var point = new Point(node.Bounds.Left + node.Bounds.Width / 2, node.Bounds.Top + node.Bounds.Height / 2);
		var cell = grid.GetCellFromPoint(point, ref cellBounds);

		if (cell != null && cellBounds.Width > 0 && cellBounds.Height > 0)
		{
			// Snap bounds with clean, balanced spacing inside the cell
			node.Bounds = new Rect(cellBounds.X + 4, cellBounds.Y + 4, cellBounds.Width - 8, cellBounds.Height - 8);
		}
	}

	private void OnNodeCreated(object sender, NodeEventArgs e)
	{
		SnapToCell(e.Node);
	}

	private void OnNodeModified(object sender, NodeEventArgs e)
	{
		SnapToCell(e.Node);
	}
}
