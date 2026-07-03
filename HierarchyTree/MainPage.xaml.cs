using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using MindFusion.Diagramming;
using MindFusion.Diagramming.Layout;
using MindFusion.Drawing;

using SolidBrush = MindFusion.Drawing.SolidBrush;
using Pen = MindFusion.Drawing.Pen;
using LinearGradientBrush = MindFusion.Drawing.LinearGradientBrush;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace HierarchyTree;

public partial class MainPage : ContentPage
{
	private Diagram diagram;

	public MainPage()
	{
		InitializeComponent();

		diagram = diagramView.Diagram;
		diagram.SelectAfterCreate = false;

		// Set diagram canvas properties
		diagram.Bounds = new Rect(0, 0, 500, 500);
		diagram.BackBrush = new SolidBrush(Color.FromRgb(248, 250, 252)); // modern soft light slate gray
		diagram.DiagramStyle.ShadowBrush = new SolidBrush(Color.FromRgba(15, 23, 42, 8)); // Soft micro-shadow
		diagram.DiagramStyle.FontFamily = "OpenSansRegular";
		diagram.DiagramStyle.FontSize = 2.4f;

		// Configure links layout default
		diagram.RouteLinks = true;
		diagram.RoundedLinks = true;
		diagram.AutoSnapLinks = true;

		// Wire validation and post-creation events
		diagram.NodeCreating += OnNodeCreating;
		diagram.NodeCreated += OnNodeCreated;

		// Initialize NodeListView
		nodeListView.Target = diagramView;
		nodeListView.IconSize = new Size(54, 28);
        nodeListView.BackgroundColor = Color.FromRgb(220, 220, 220); // modern soft light slate gray

        var nodeRect = new Rect(0, 0, 26, 12);

		// Helper to create beautiful, consistent template cards for NodeListView
		ShapeNode CreateTemplateNode(string text, Color stroke, Color fill)
		{
			var node = new ShapeNode();
			node.Bounds = nodeRect;
			node.Shape = Shapes.RoundRect;
			node.Pen = new Pen(stroke, 0.4);
			node.Brush = new SolidBrush(fill);
			node.Text = text;
			node.TextBrush = new SolidBrush(Color.FromRgb(15, 23, 42)); // Slate 900
			node.Font = Microsoft.Maui.Font.OfSize("OpenSansSemibold", 2.2);

			// Align text perfectly centered horizontally and vertically
			node.TextFormat.HorizontalAlignment = MindFusion.Drawing.HorizontalAlignment.Center;
			node.TextFormat.VerticalAlignment = MindFusion.Drawing.VerticalAlignment.Center;

			return node;
		}

		nodeListView.AddNode(CreateTemplateNode("CEO", Color.FromRgb(30, 58, 138), Color.FromRgb(219, 234, 254)), "CEO");
		nodeListView.AddNode(CreateTemplateNode("VP", Color.FromRgb(91, 33, 182), Color.FromRgb(243, 232, 255)), "VP");
		nodeListView.AddNode(CreateTemplateNode("Manager", Color.FromRgb(6, 95, 70), Color.FromRgb(209, 250, 229)), "Manager");
		nodeListView.AddNode(CreateTemplateNode("Lead", Color.FromRgb(15, 118, 110), Color.FromRgb(204, 251, 241)), "Lead");
		nodeListView.AddNode(CreateTemplateNode("Worker", Color.FromRgb(154, 52, 18), Color.FromRgb(255, 237, 213)), "Worker");

		// Initialize diagram with Root Node (CEO)
		var ceoNode = diagram.Factory.CreateShapeNode(0, 0, 26, 12);
		ceoNode.Shape = Shapes.RoundRect;
		ceoNode.Pen = new Pen(Color.FromRgb(30, 58, 138), 0.4);
		ceoNode.Brush = new SolidBrush(Color.FromRgb(219, 234, 254));
		ceoNode.Text = "CEO";
		ceoNode.TextBrush = new SolidBrush(Color.FromRgb(15, 23, 42));
		ceoNode.Font = Microsoft.Maui.Font.OfSize("OpenSansSemibold", 2.3);


		// Perform initial center layout
		ArrangeTree();
	}

	// Handle the validation event to make sure nodes are always nested
	private void OnNodeCreating(object sender, NodeValidationEventArgs e)
	{
		// Check if there is an existing node under the current pointer coordinate
		var parentNode = diagram.GetNodeAt(e.Position);

		// If dropped on empty space, or if the node under mouse is the temporary node itself
		if (parentNode == null || parentNode == e.Node)
		{
			// Cancel creation
			e.Cancel = true;
		}
	}

	// style the new node and re-arrange the hierarchy
	private void OnNodeCreated(object sender, NodeEventArgs e)
	{
		// Find parent node under drop position (excluding the new node itself)
		DiagramNode parentNode = null;
		foreach (var node in diagram.Nodes)
		{
			if (node != e.Node && node.ContainsPoint(e.Position))
			{
				parentNode = node;
				break;
			}
		}

		if (parentNode != null)
		{
			// Link Parent -> Child
			var link = diagram.Factory.CreateDiagramLink(parentNode, e.Node);
			link.Pen = new Pen(Color.FromRgb(148, 163, 184), 0.4); // Slate 400
			
			// Set gorgeous Arrow Head style and size
			link.HeadShape = ArrowHeads.PointerArrow;
			link.HeadShapeSize = 2.0;

			// Style child node text precisely and enforce uniform sizing
			if (e.Node is ShapeNode shapeNode)
			{
				// Force uniform bounds matching the CEO node (width: 26, height: 12)
				shapeNode.Bounds = new Rect(shapeNode.Bounds.X, shapeNode.Bounds.Y, 26, 12);

				shapeNode.TextBrush = new SolidBrush(Color.FromRgb(15, 23, 42)); // Slate 900
				shapeNode.Font = Microsoft.Maui.Font.OfSize("OpenSansSemibold", 2.2);

			}

			// Smoothly re-arrange hierarchy using TreeLayout
			ArrangeTree();
		}
	}

	// apply the TreeLayout with some custom settings
	private void ArrangeTree()
	{
		var layout = new TreeLayout();
		layout.Type = TreeLayoutType.Centered;
		layout.Direction = TreeLayoutDirections.TopToBottom;
		layout.LinkStyle = TreeLayoutLinkType.Cascading3;
		layout.LevelDistance = 16f;
		layout.NodeDistance = 12f;
		layout.KeepRootPosition = false; // Automatically center the root node and entire tree in doc bounds
		layout.Arrange(diagram);

		// Adjust scroll bounds to fit content
		diagram.ResizeToFitItems(5, true);
	}
}
