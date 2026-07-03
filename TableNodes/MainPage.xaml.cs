using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using MindFusion.Diagramming;
using MindFusion.Drawing;

using SolidBrush = MindFusion.Drawing.SolidBrush;
using Pen = MindFusion.Drawing.Pen;
using LinearGradientBrush = MindFusion.Drawing.LinearGradientBrush;

namespace TableNodes;

public partial class MainPage : ContentPage
{
	private Diagram diagram;

	public MainPage()
	{
		InitializeComponent();

		diagram = diagramView.Diagram;
		diagram.SelectAfterCreate = false;

		// Set diagram canvas properties for high-quality diagram
		diagram.Bounds = new Rect(0, 0, 160, 120);
		diagram.BackBrush = new SolidBrush(Color.FromRgb(248, 250, 252)); // modern soft light slate gray
        diagram.BackBrush = new SolidBrush(Colors.White); // modern soft light slate gray
        diagram.DiagramStyle.ShadowBrush = new SolidBrush(Color.FromRgba(15, 23, 42, 6)); // Soft micro-shadow
		diagram.DiagramStyle.FontFamily = "OpenSansRegular";
		diagram.DiagramStyle.FontSize = 2.0f;
		diagram.TableRowHeight = 5;

		// Configure links layout default (Cascading links are gorgeous for database schemas)
		diagram.RouteLinks = true;
		diagram.AutoSnapLinks = true;
		diagram.LinkSegments = 3;
		diagram.LinkShape = LinkShape.Cascading;

		// Styles/Brushes for tables
		var strokeColor = Color.FromRgb(51, 65, 85); // Slate 700
		var fillColor = Color.FromRgb(255, 255, 255); // White
		var captionColor = Color.FromRgb(241, 245, 249); // Slate 100

		// Helper to create and configure a TableNode beautifully
		TableNode CreateSchemaTable(string caption, double x, double y, int rowCount)
		{
			// Create TableNode at specified coordinates
			var table = diagram.Factory.CreateTableNode(x, y, 32, 28);
			table.RowCount = rowCount;
			table.ColumnCount = 2;
			table.Caption = caption;
			table.CaptionHeight = 6;

			// Enable Row-to-Row connections
			table.ConnectionStyle = TableConnectionStyle.Rows;

			// Styling
			table.Pen = new Pen(strokeColor, 0.4);
			table.Brush = new SolidBrush(fillColor);
			table.CaptionBrush = new SolidBrush(captionColor);
			table.TextBrush = new SolidBrush(Color.FromRgb(15, 23, 42)); // Slate 900
			table.Font = Microsoft.Maui.Font.OfSize("OpenSansSemibold", 2.0);

			// Configure individual rows
			for (int r = 0; r < rowCount; r++)
			{
				table[0, r].TextBrush = new SolidBrush(Color.FromRgb(100, 116, 139)); // Gray-500 datatype text
				table[1, r].TextBrush = new SolidBrush(Color.FromRgb(15, 23, 42));    // Slate-900 field name text
				table[0, r].Font = Microsoft.Maui.Font.OfSize("OpenSansRegular", 1.8);
				table[1, r].Font = Microsoft.Maui.Font.OfSize("OpenSansSemibold", 1.8);
			}

			return table;
		}

	
		// USER Table
		var userTable = CreateSchemaTable("User", 10, 20, 4);
		userTable[0, 0].Text = "[PK] int";     userTable[1, 0].Text = "id";
		userTable[0, 1].Text = "varchar";      userTable[1, 1].Text = "username";
		userTable[0, 2].Text = "varchar";      userTable[1, 2].Text = "email";
		userTable[0, 3].Text = "datetime";     userTable[1, 3].Text = "created_at";

		// ORDER Table
		var orderTable = CreateSchemaTable("Order", 60, 20, 4);
		orderTable[0, 0].Text = "[PK] int";     orderTable[1, 0].Text = "id";
		orderTable[0, 1].Text = "[FK] int";     orderTable[1, 1].Text = "user_id";
		orderTable[0, 2].Text = "decimal";      orderTable[1, 2].Text = "total_amount";
		orderTable[0, 3].Text = "varchar";      orderTable[1, 3].Text = "status";

		// ORDERITEM Table
		var orderItemTable = CreateSchemaTable("OrderItem", 110, 20, 4);
		orderItemTable[0, 0].Text = "[PK] int";     orderItemTable[1, 0].Text = "id";
		orderItemTable[0, 1].Text = "[FK] int";     orderItemTable[1, 1].Text = "order_id";
		orderItemTable[0, 2].Text = "[FK] int";     orderItemTable[1, 2].Text = "product_id";
		orderItemTable[0, 3].Text = "int";          orderItemTable[1, 3].Text = "quantity";

		// PRODUCT Table
		var productTable = CreateSchemaTable("Product", 110, 70, 4);
		productTable[0, 0].Text = "[PK] int";     productTable[1, 0].Text = "id";
		productTable[0, 1].Text = "varchar";      productTable[1, 1].Text = "name";
		productTable[0, 2].Text = "decimal";      productTable[1, 2].Text = "price";
		productTable[0, 3].Text = "int";          productTable[1, 3].Text = "stock";

		// create links connecting rows between tables
		void CreateRelationship(TableNode fromTable, int fromRow, TableNode toTable, int toRow)
		{
			var link = diagram.Factory.CreateDiagramLink(fromTable, toTable);
			link.OriginIndex = fromRow;
			link.DestinationIndex = toRow;
			link.Pen = new Pen(Color.FromRgb(100, 116, 139), 0.4); // Slate 500
			link.HeadShapeSize = 1.6;
		}

		// User.id (Row 0) -> Order.user_id (Row 1)
		CreateRelationship(userTable, 0, orderTable, 1);

		// Order.id (Row 0) -> OrderItem.order_id (Row 1)
		CreateRelationship(orderTable, 0, orderItemTable, 1);

		// Product.id (Row 0) -> OrderItem.product_id (Row 2)
		CreateRelationship(productTable, 0, orderItemTable, 2);

		// Auto-fit diagram to contain all schemas beautifully
		diagram.ResizeToFitItems(5, true);
	}
}
