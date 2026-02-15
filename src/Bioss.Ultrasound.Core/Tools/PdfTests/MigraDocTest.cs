//using Bioss.Ultrasound.Core.Tools;
//using MigraDocCore.DocumentObjectModel;
//using MigraDocCore.DocumentObjectModel.Shapes.Charts;
//using MigraDocCore.DocumentObjectModel.Tables;
//using MigraDocCore.Rendering;

//namespace Bioss.Ultrasound.Tools.PdfTests
//{
//    public class MigraDocTest : IPdfTest
//    {
//        public string Name => "MigraDoc";

//        public void CreatePdfFile(string fileName)
//        {
//            MyFontResolver.Apply();

//            var document = CreateDocument();
//            SetStyles(document);

//            AddHeader(document);
//            AddContent(document);
//            AddFooter(document);

//            SaveDocument(document, fileName);
//        }

//        private Document CreateDocument()
//        {
//            var document = new Document();
//            document.Info.Title = "Document Titile";
//            document.Info.Subject = "Document Subject";
//            document.Info.Author = "MCS";
//            document.Info.Keywords = "Products";
//            return document;
//        }

//        private void SetStyles(Document document)
//        {
//            //  Modifying default style
//            var style = document.Styles[StyleNames.Normal];
//            style.Font.Name = "Segoe UI";
//            style.Font.Color = Colors.Black;
//            style.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
//            style.ParagraphFormat.PageBreakBefore = false; // if need next page

//            //  Header style
//            style = document.Styles[StyleNames.Header];
//            style.Font.Name = "Segoe UI";
//            style.Font.Size = 18;
//            style.Font.Color = Colors.Black;
//            style.Font.Bold = true;
//            style.Font.Underline = Underline.Single;
//            style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

//            //  Footer style
//            style = document.Styles[StyleNames.Footer];
//            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Right);

//            //  Modifying predefined style: HeadingN (where N goes from 1 to 9)
//            style = document.Styles[StyleNames.Heading1];
//            style.Font.Name = "Segoe UI";
//            style.Font.Size = 14;
//            style.Font.Bold = true;
//            style.Font.Italic = false;
//            style.Font.Color = Colors.DarkBlue;
//            style.ParagraphFormat.Shading.Color = Colors.SkyBlue;
//            style.ParagraphFormat.Borders.Distance = "3pt";
//            style.ParagraphFormat.Borders.Width = 2.5;
//            style.ParagraphFormat.Borders.Color = Colors.CadetBlue;
//            style.ParagraphFormat.SpaceAfter = "1cm";

//            //  Modifying predefined style: Heading2
//            style = document.Styles[StyleNames.Heading2];
//            style.Font.Name = "Segoe UI";
//            style.Font.Size = 12;
//            style.Font.Bold = false;
//            style.Font.Italic = true;
//            style.Font.Color = Colors.DeepSkyBlue;
//            style.ParagraphFormat.Shading.Color = Colors.White;
//            style.ParagraphFormat.Borders.Width = 0;
//            style.ParagraphFormat.Borders.Color = Colors.CadetBlue;
//            style.ParagraphFormat.SpaceAfter = 3;
//            style.ParagraphFormat.SpaceBefore = 3;

//            //  Adding new style
//            style = document.Styles.AddStyle("MyParagraphStyle", StyleNames.Normal);
//            style.Font.Size = 10;
//            style.Font.Color = Colors.Blue;
//            style.ParagraphFormat.SpaceAfter = 3;


//            style = document.Styles.AddStyle("MyTableStyle", StyleNames.Normal);
//            style.Font.Size = 9;
//            style.Font.Color = Colors.SteelBlue;
//        }

//        private void AddHeader(Document document)
//        {
//            var section = document.AddSection();

//            var config = section.PageSetup;
//            config.PageFormat = PageFormat.A4;
//            config.Orientation = Orientation.Portrait;
//            config.TopMargin = "3cm";
//            config.LeftMargin = 15;
//            config.BottomMargin = "3cm";
//            config.RightMargin = 15;
//            config.OddAndEvenPagesHeaderFooter = true; //   разные хедеры на первой и остальных страницах
//            config.StartingNumber = 1;

//            var oddHeader = section.Headers.Primary;

//            //  хедер на первой странице
//            var content = new Paragraph();
//            content.AddText("\tProduct Catalog 2022 - Medical Computer Systems");
//            oddHeader.Add(content);
//            oddHeader.AddTable();

//            //  хедер на следующих страницах
//            var headerForEvenPages = section.Headers.EvenPage;
//            headerForEvenPages.AddParagraph("Product Catalog 2022");
//            headerForEvenPages.AddTable();
//        }

//        private void AddContent(Document document)
//        {
//            AddText1(document);
//            AddImage(document);
//            AddText2(document);
//            AddTable(document);
//            AddChart(document);
//        }

//        private void AddFooter(Document document)
//        {
//            var content = new Paragraph();
//            content.AddText(" Page ");
//            content.AddPageField();
//            content.AddText(" of ");
//            content.AddNumPagesField();

//            var section = document.LastSection;
//            section.Footers.Primary.Add(content);

//            var contentForEvenPages = content.Clone();
//            contentForEvenPages.AddTab();
//            contentForEvenPages.AddText("\tDate: ");
//            contentForEvenPages.AddDateField("dddd, MMMM dd, yyyy HH:mm:ss tt");

//            section.Footers.EvenPage.Add(contentForEvenPages);
//        }

//        private void AddText1(Document document)
//        {
//            var section = document.LastSection;
//            var text = "Привет At Medical Computer Systems Inc, it is our top priority to bring on";
//            var mainParagraph = section.AddParagraph(text, StyleNames.Heading1);
//            mainParagraph.AddLineBreak();

//            text = "All components of Medical Computer Systems Inc sample products have";
//            section.AddParagraph(text, StyleNames.Heading2);
//        }

//        private void AddImage(Document document)
//        {
//            var paragraph = document.LastSection.AddParagraph();
//            paragraph.Format.Alignment = ParagraphAlignment.Center;

//            var imageSource = MigraDocTools.GetImageSourceFromResources("baseline");
//            var image = paragraph.AddImage(imageSource);

//            image.LineFormat = new MigraDocCore.DocumentObjectModel.Shapes.LineFormat { Color = Colors.DarkGreen };
//            image.LockAspectRatio = true;
//            image.Width = "2.5cm";
//        }

//        private void AddText2(Document document)
//        {
//            var section = document.LastSection;
//            var text = "Мы рекомендуем покупать продукты только у нас";
//            var paragraph = section.AddParagraph(text, "MyParagraphStyle");

//            text = "\nНаша продукция протестирована много раз";
//            section.AddParagraph(text, "MyParagraphStyle");

//            paragraph.AddLineBreak();
//        }

//        private void AddTable(Document document)
//        {
//            var titiles = new string[] { "Product", "Name", "Price" };
//            var borderColor = new MigraDocCore.DocumentObjectModel.Color(81, 125, 192);

//            var section = document.LastSection;

//            var table = section.AddTable();
//            table.Style = "MyTableStyle";
//            table.Borders.Color = borderColor;
//            table.Borders.Visible = true;
//            table.Borders.Width = 0.75;
//            table.Rows.LeftIndent = 5;

//            var column = table.AddColumn("5cm");
//            column.Format.Alignment = ParagraphAlignment.Center;

//            column = table.AddColumn("10cm");
//            column.Format.Alignment = ParagraphAlignment.Left;

//            column = table.AddColumn("4cm");
//            column.Format.Alignment = ParagraphAlignment.Right;

//            table.Rows.HeightRule = RowHeightRule.Exactly;
//            table.Rows.Height = "1cm";

//            var headerRow = table.AddRow();
//            headerRow.HeadingFormat = true;
//            headerRow.Format.Alignment = ParagraphAlignment.Center;
//            headerRow.Format.Font.Bold = true;

//            for (int i = 0; i < titiles.Length; i++)
//            {
//                headerRow.Cells[i].AddParagraph(titiles[i]);
//                headerRow.Cells[i].Format.Alignment = ParagraphAlignment.Center;
//                headerRow.Cells[i].VerticalAlignment = VerticalAlignment.Center;
//                headerRow.Shading.Color = Colors.PaleGoldenrod;
//                headerRow.Borders.Width = 1;
//            }

//            var itemsCount = 40;
//            for (var i = 0; i < itemsCount; ++i)
//            {
//                var rowItem = table.AddRow();
//                rowItem.TopPadding = 1.5;
//                rowItem.Borders.Left.Width = 0.25;

//                var titleCell = rowItem.Cells[0];
//                titleCell.AddParagraph($"{i}");
//                titleCell.VerticalAlignment = VerticalAlignment.Center;

//                var authorCell = rowItem.Cells[1];
//                authorCell.AddParagraph($"Author Name {i}");

//                var fechaCell = rowItem.Cells[2];
//                fechaCell.AddParagraph($"$1,99{i}");
//            }

//            var row = table.AddRow();
//            row.Borders.Visible = false;
//        }

//        private void AddChart(Document document)
//        {
//            Paragraph paragraph = document.LastSection.AddParagraph("Chart Overview", StyleNames.Heading1);
//            paragraph.AddBookmark("Charts");

//            document.LastSection.AddParagraph("Sample Chart", StyleNames.Heading2);
//            Chart chart = new Chart();
//            chart.Left = 0;

//            chart.Width = Unit.FromCentimeter(16);
//            chart.Height = Unit.FromCentimeter(12);
//            Series series = chart.SeriesCollection.AddSeries();
//            series.ChartType = ChartType.Line;
//            series.Add(new double[] { 1, 17, 45, 5, 3, 20, 11, 23, 8, 19 });
//            series.HasDataLabel = true;

//            series = chart.SeriesCollection.AddSeries();
//            series.ChartType = ChartType.Line;
//            series.Add(new double[] { 41, 7, 5, 45, 13, 10, 21, 13, 18, 9 });

//            XSeries xseries = chart.XValues.AddXSeries();
//            xseries.Add("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N");


//            chart.XAxis.MajorTickMark = TickMarkType.Outside;
//            chart.XAxis.Title.Caption = "X-Axis";

//            chart.YAxis.MajorTickMark = TickMarkType.Outside;
//            chart.YAxis.HasMajorGridlines = true;

//            chart.PlotArea.LineFormat.Color = Colors.DarkGray;
//            chart.PlotArea.LineFormat.Width = 1;

//            document.LastSection.Add(chart);
//        }

//        private void SaveDocument(Document document, string fileName)
//        {
//            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
//            renderer.Document = document;
//            renderer.RenderDocument();
//            renderer.PdfDocument.Save(fileName);
//        }
//    }
//}
