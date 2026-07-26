using QuestPDF.Fluent;
using QuestPDF.Helpers;
using La_Divine.Src.Dtos.Reservation;
using La_Divine.Src.Services.Interfaces;

namespace La_Divine.Src.Services;

/// <summary>
/// Servicio que genera la entrada en PDF de una reserva usando QuestPDF (FUN-005).
/// Aplica los colores institucionales del Teatro La Divine.
/// </summary>
public class PdfServices : IPdfServices
{
    // Colores institucionales
    private const string Primario = "#8B0000";   // Vinotinto
    private const string Dorado = "#C0A060";     // Secundario
    private const string Acento = "#F5F0E8";     // Fondo claro

    public async Task<PdfReservationResponse> GeneratePdfAsync(PdfReservationDto reservationData)
    {
        try
        {
            var pdf = await Task.Run(() =>
            {
                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(25);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Roboto"));

                        page.Content().Column(column =>
                        {
                            // ── ENCABEZADO ──
                            column.Item()
                                .AlignCenter()
                                .PaddingBottom(8)
                                .Text("Teatro La Divine")
                                .FontSize(24).Bold().FontColor(Primario);

                            column.Item()
                                .AlignCenter()
                                .PaddingBottom(10)
                                .Text("Entrada Oficial")
                                .FontSize(13).FontColor(Dorado);

                            column.Item().PaddingBottom(10).LineHorizontal(2).LineColor(Primario);

                            // ── NOMBRE DE LA OBRA ──
                            column.Item()
                                .PaddingVertical(8)
                                .Text(reservationData.FunctionTitle)
                                .FontSize(18).Bold().FontColor("#1a1a1a");

                            // ── FECHA/HORA + SALA ──
                            column.Item()
                                .PaddingVertical(8)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Background(Acento).Padding(12)
                                        .Border(1).BorderColor(Dorado)
                                        .Column(col =>
                                        {
                                            col.Item().Text("Fecha y hora").FontSize(10).Bold().FontColor(Primario);
                                            col.Item().PaddingTop(6)
                                                .Text($"{(DateTime.TryParse(reservationData.FunctionDate, out var fecha) ? fecha.ToString("dd/MM/yyyy") : reservationData.FunctionDate)}  -  {reservationData.FunctionTime}")
                                                .FontSize(12).FontColor("#1a1a1a");
                                        });

                                    row.ConstantItem(10);

                                    row.RelativeItem()
                                        .Background(Acento).Padding(12)
                                        .Border(1).BorderColor(Dorado)
                                        .Column(col =>
                                        {
                                            col.Item().Text("Sala").FontSize(10).Bold().FontColor(Primario);
                                            col.Item().PaddingTop(6)
                                                .Text("Sala principal")
                                                .FontSize(12).FontColor("#1a1a1a");
                                        });
                                });

                            // ── ASIENTOS ──
                            column.Item()
                                .PaddingTop(12)
                                .Text("Asientos reservados")
                                .FontSize(11).Bold().FontColor(Primario);

                            column.Item().PaddingTop(6).Column(seatCol =>
                            {
                                foreach (var seat in reservationData.SelectedSeats)
                                {
                                    seatCol.Item()
                                        .PaddingVertical(3)
                                        .Border(1).BorderColor(Dorado)
                                        .Background(Acento).Padding(10)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Text($"Asiento: {seat.SeatNumber}")
                                                .FontSize(11).Bold().FontColor("#1a1a1a");
                                            row.RelativeItem().Text($"Fila/Sección: {seat.Section}")
                                                .FontSize(11).FontColor("#444");
                                            row.RelativeItem().AlignRight().Text($"${seat.Price:N0}")
                                                .FontSize(11).FontColor("#444");
                                        });
                                }
                            });

                            // ── TITULAR + CÓDIGO ──
                            column.Item()
                                .PaddingTop(14)
                                .Background(Acento).Padding(12)
                                .Border(1).BorderColor(Dorado)
                                .Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("Titular").FontSize(10).Bold().FontColor(Primario);
                                        col.Item().PaddingTop(6).Text(reservationData.HolderName)
                                            .FontSize(12).FontColor("#1a1a1a");
                                    });

                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("Código de entrada").FontSize(10).Bold().FontColor(Primario);
                                        col.Item().PaddingTop(6).Text(reservationData.ReservationCode)
                                            .FontSize(12).Bold().FontColor("#1a1a1a");
                                    });
                                });

                            // ── CÓDIGO DESTACADO ──
                            column.Item()
                                .PaddingTop(14)
                                .Background(Primario).Padding(14)
                                .AlignCenter()
                                .Column(col =>
                                {
                                    col.Item().AlignCenter().Text("CÓDIGO ÚNICO DE ENTRADA")
                                        .FontSize(11).Bold().FontColor("#fff");
                                    col.Item().AlignCenter().PaddingTop(8)
                                        .Text(reservationData.ReservationCode)
                                        .FontSize(20).Bold().FontColor(Dorado);
                                });

                            // ── TOTAL ──
                            column.Item()
                                .PaddingTop(14)
                                .AlignRight()
                                .Text($"Total pagado: ${reservationData.TotalPrice:N0}")
                                .FontSize(14).Bold().FontColor(Primario);

                            // ── FOOTER ──
                            column.Item().PaddingTop(16).LineHorizontal(1).LineColor(Dorado);
                            column.Item().PaddingTop(8).AlignCenter()
                                .Text("Presente este código en la taquilla del teatro el día de la función.")
                                .FontSize(9).FontColor("#888");
                        });
                    });
                }).GeneratePdf();
            });

            return new PdfReservationResponse
            {
                Success = true,
                Message = "PDF generado exitosamente.",
                Data = pdf
            };
        }
        catch (Exception ex)
        {
            return new PdfReservationResponse
            {
                Success = false,
                Message = $"Error al generar el PDF: {ex.Message}",
                Data = []
            };
        }
    }
}