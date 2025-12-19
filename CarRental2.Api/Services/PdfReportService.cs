using CarRental2.Core.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Reflection.Metadata;


namespace CarRental.Api.Services
{
    public class PdfReportService
    {
        public void ExportFinancialReport(FinancialReportDto report, string filePath)
        {
            // Remarque : QuestPDF nécessite une licence communautaire pour l'usage gratuit
            QuestPDF.Settings.License = LicenseType.Community;

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.SegoeUI));

                    // --- EN-TÊTE ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("RAPPORT FINANCIER").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Période : {report.StartDate:dd/MM/yyyy} au {report.EndDate:dd/MM/yyyy}");
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("CarRental System").FontSize(14).SemiBold();
                            col.Item().Text("Back Office Management");
                        });
                    });

                    // --- CONTENU (TABLEAU) ---
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80); // Date
                                columns.RelativeColumn(2);  // Client
                                columns.RelativeColumn(2);  // Véhicule
                                columns.ConstantColumn(80); // Total Dû
                                columns.ConstantColumn(80); // Payé
                            });

                            // En-têtes du tableau
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Date");
                                header.Cell().Element(CellStyle).Text("Client");
                                header.Cell().Element(CellStyle).Text("Véhicule");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total Dû");
                                header.Cell().Element(CellStyle).AlignRight().Text("Payé");

                                static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            // Lignes de données
                            foreach (var item in report.Details)
                            {
                                table.Cell().Element(RowStyle).Text($"{item.ReservationDate:dd/MM/yyyy}");
                                table.Cell().Element(RowStyle).Text(item.ClientFullName);
                                table.Cell().Element(RowStyle).Text(item.VehicleModel);
                                table.Cell().Element(RowStyle).AlignRight().Text($"{item.TotalReservationPrice:N2} €");
                                table.Cell().Element(RowStyle).AlignRight().Text($"{item.AmountPaid:N2} €");

                                static IContainer RowStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            }
                        });

                        // --- RÉSUMÉ FINAL ---
                        column.Item().AlignRight().PaddingTop(20).Column(col =>
                        {
                            col.Item().Text($"Chiffre d'Affaires Dû : {report.TotalAmountDue:N2} €").FontSize(12);
                            col.Item().Text($"Total Encaissé : {report.TotalPaymentsCollected:N2} €").FontSize(12).FontColor(Colors.Green.Medium);
                            col.Item().PaddingTop(5).Text($"SOLDE À PERCEVOIR : {report.TotalRemainingBalance:N2} €").FontSize(14).SemiBold().FontColor(Colors.Red.Medium);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(filePath);
        }
    }
}