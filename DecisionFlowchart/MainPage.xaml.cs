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

namespace DecisionFlowchart;

public partial class MainPage : ContentPage
{
	// Data structure to hold node parameters
	private class NodeData
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public string Type { get; set; } // "Start", "Decision", "Terminal", "Enroll"
	}

	// Data structure to hold link parameters
	private class LinkData
	{
		public string From { get; set; }
		public string To { get; set; }
		public string Text { get; set; }
		public Color Color { get; set; }
	}

	public MainPage()
	{
		InitializeComponent();

		Diagram diagram = diagramView.Diagram;
		diagram.SelectAfterCreate = false;

		// Set diagram canvas properties
		diagram.Bounds = new Rect(0, 0, 200, 200);
		diagram.BackBrush = new SolidBrush(Color.FromRgb(248, 250, 252)); // modern soft light slate gray
		diagram.DiagramStyle.ShadowBrush = new SolidBrush(Color.FromRgba(15, 23, 42, 8)); // Subtle modern shadow
		diagram.DiagramStyle.FontFamily = "OpenSansRegular";
		diagram.DiagramStyle.FontSize = 2.4f;

		var assembly = typeof(App).GetTypeInfo().Assembly;
		var nodes = new Dictionary<string, ShapeNode>();

		// create the employee nodes
		void StyleNode(ShapeNode node, Color strokeColor, Color fillColor, string fontFamily = "OpenSansRegular", double fontSize = 2.4)
		{
			node.Pen = new Pen(strokeColor, 0.4);
			node.Brush = new SolidBrush(fillColor);
			node.TextBrush = new SolidBrush(Color.FromRgb(15, 23, 42)); // Slate 900
			node.Font = Microsoft.Maui.Font.OfSize(fontFamily, fontSize);
		}

		//list the node types
		var nodeDataList = new List<NodeData>
		{
			new NodeData { Id = "Start", Text = "START:\nConsidering University?", Type = "Start" },
			new NodeData { Id = "Dec1", Text = "Do you have a\nclear career goal\nrequiring a degree?", Type = "Decision" },
			new NodeData { Id = "Dec2", Text = "Can you afford\ntuition without\nhigh-interest debt?", Type = "Decision" },
			new NodeData { Id = "Dec3", Text = "Do you want to\nexplore fields\n& network?", Type = "Decision" },
			new NodeData { Id = "Dec4", Text = "Prepared for\nacademic rigor &\nself-study?", Type = "Decision" },
			new NodeData { Id = "Dec5", Text = "Can you get\nscholarships or\ngrants / aid?", Type = "Decision" },
			new NodeData { Id = "Dec6", Text = "Willing to spend\n3-4 years & money\nto explore?", Type = "Decision" },
			new NodeData { Id = "Term1", Text = "Consider Alternative Paths\n(Vocational, starting a\nbusiness, self-study)", Type = "Terminal" },
			new NodeData { Id = "Term2", Text = "Take a gap year or work\nfirst to build discipline", Type = "Terminal" },
			new NodeData { Id = "Term3", Text = "Consider Community\nCollege first or\npart-time study", Type = "Terminal" },
			new NodeData { Id = "Enroll", Text = "", Type = "Enroll" } // Special Image Node
		};

		// populate the node list
		foreach (var data in nodeDataList)
		{
			ShapeNode node;
			if (data.Type == "Enroll")
			{
				node = diagram.Factory.CreateShapeNode(0, 0, 24, 24);
				node.Shape = Shapes.Rectangle;
				node.Transparent = true;
				node.Pen = new Pen(Colors.Transparent, 0);
				node.ShadowOffsetX = 0;
				node.ShadowOffsetY = 0;

				using (Stream stream = assembly.GetManifestResourceStream("DecisionFlowchart.rode_sign.png"))
				{
					if (stream != null)
					{
						IImage image = PlatformImage.FromStream(stream);
						node.Image = image;
					}
				}

				var label = node.AddLabel("ENROLL AT\nUNIVERSITY!");
				label.SetEdgePosition(2, 0, 12); // Bottom edge, Y offset 12
				label.Font = Microsoft.Maui.Font.OfSize("OpenSansSemibold", 2.4);
				label.TextBrush = new SolidBrush(Color.FromRgb(21, 128, 61)); // Emerald 700
			}
			else
			{
				if (data.Type == "Decision")
				{
					node = diagram.Factory.CreateShapeNode(0, 0, 24, 16);
					node.Shape = Shapes.Decision;
					StyleNode(node, Color.FromRgb(109, 40, 217), Color.FromRgb(243, 232, 255), "OpenSansRegular", 2.1);
				}
				else if (data.Type == "Start")
				{
					node = diagram.Factory.CreateShapeNode(0, 0, 28, 12);
					node.Shape = Shapes.RoundRect;
					StyleNode(node, Color.FromRgb(14, 116, 144), Color.FromRgb(207, 250, 254), "OpenSansSemibold", 2.3);
				}
				else // Terminal
				{
					node = diagram.Factory.CreateShapeNode(0, 0, 28, 12);
					node.Shape = Shapes.RoundRect;
					StyleNode(node, Color.FromRgb(180, 83, 9), Color.FromRgb(254, 243, 199), "OpenSansRegular", 2.1);
				}
				node.Text = data.Text;
			}
			nodes[data.Id] = node;
		}

		// Helper to create and style links
		void CreateLink(DiagramNode origin, DiagramNode target, string labelText, Color strokeColor)
		{
			var link = diagram.Factory.CreateDiagramLink(origin, target);
			link.Text = labelText;
			link.TextStyle = LinkTextStyle.OverLongestSegment;
			link.TextBrush = new SolidBrush(Color.FromRgb(71, 85, 105)); // Slate 600
			link.Font = Microsoft.Maui.Font.OfSize("OpenSansSemibold", 2.2);
			link.Pen = new Pen(strokeColor, 0.4);
			link.HeadShapeSize = 2.2;
		}

		Color yesColor = Color.FromRgb(21, 128, 61);  // Emerald-700
		Color noColor = Color.FromRgb(220, 38, 38);   // Red-600
		Color neutralColor = Color.FromRgb(100, 116, 139); // Slate-500

		// connect the nodes
		var linkDataList = new List<LinkData>
		{
			new LinkData { From = "Start", To = "Dec1", Text = "", Color = neutralColor },
			new LinkData { From = "Dec1", To = "Dec2", Text = "Yes", Color = yesColor },
			new LinkData { From = "Dec1", To = "Dec3", Text = "No", Color = noColor },
			new LinkData { From = "Dec2", To = "Dec4", Text = "Yes", Color = yesColor },
			new LinkData { From = "Dec2", To = "Dec5", Text = "No", Color = noColor },
			new LinkData { From = "Dec3", To = "Dec6", Text = "Yes", Color = yesColor },
			new LinkData { From = "Dec3", To = "Term1", Text = "No", Color = noColor },
			new LinkData { From = "Dec4", To = "Enroll", Text = "Yes", Color = yesColor },
			new LinkData { From = "Dec4", To = "Term2", Text = "No", Color = noColor },
			new LinkData { From = "Dec5", To = "Dec4", Text = "Yes", Color = yesColor },
			new LinkData { From = "Dec5", To = "Term3", Text = "No", Color = noColor },
			new LinkData { From = "Dec6", To = "Dec2", Text = "Yes", Color = yesColor },
			new LinkData { From = "Dec6", To = "Term1", Text = "No", Color = noColor }
		};

		
		foreach (var linkData in linkDataList)
		{
			CreateLink(nodes[linkData.From], nodes[linkData.To], linkData.Text, linkData.Color);
		}

		// Set diagram routing configuration
		diagram.RouteLinks = true;
		diagram.AutoSnapLinks = true;

		// apply the DecisionLayout algorithm with some customizations
		var layout = new DecisionLayout();
		layout.StartNode = nodes["Start"];
		layout.HorizontalPadding = 12f;
		layout.VerticalPadding = 16f;
		layout.LinkPadding = 8f;
		layout.Arrange(diagram);

		// Perform automatic scaling to fit the items
		diagram.ResizeToFitItems(5, true);
	}
}
