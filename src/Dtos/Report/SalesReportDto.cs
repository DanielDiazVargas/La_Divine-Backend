namespace La_Divine.Src.Dtos.Report;

/// <summary>
/// Respuesta estándar utilizada por el endpoint del reporte de ventas.
/// </summary>
public class SalesReportResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public SalesReportDto? Data { get; set; }
}

/// <summary>
/// Contiene toda la información del reporte de ventas solicitado por el administrador.
/// </summary>
public class SalesReportDto
{
    public string StartDate { get; set; } = string.Empty;

    public string EndDate { get; set; } = string.Empty;

    public SalesReportSummaryDto Summary { get; set; } = new();

    public List<FunctionSalesReportDto> FunctionSales { get; set; } = [];

    public List<TicketSalesReportDto> TicketDetails { get; set; } = [];

    public List<FunctionOccupancyReportDto> OccupancyByFunction { get; set; } = [];
}

/// <summary>
/// Resume los totales generales del reporte para el rango consultado.
/// </summary>
public class SalesReportSummaryDto
{
    public int FunctionCount { get; set; }

    public int TotalSoldTickets { get; set; }

    public int TotalPendingTickets { get; set; }

    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// Representa el total de ventas e ingresos de una función específica.
/// </summary>
public class FunctionSalesReportDto
{
    public int FunctionId { get; set; }

    public string FunctionName { get; set; } = string.Empty;

    public string DateFunction { get; set; } = string.Empty;

    public string TimeFunction { get; set; } = string.Empty;

    public int SoldTickets { get; set; }

    public int PendingTickets { get; set; }

    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// Representa una entrada individual asociada a una reserva del período consultado.
/// </summary>
public class TicketSalesReportDto
{
    public string ReservationCode { get; set; } = string.Empty;

    public int FunctionId { get; set; }

    public string FunctionName { get; set; } = string.Empty;

    public string DateFunction { get; set; } = string.Empty;

    public string TimeFunction { get; set; } = string.Empty;

    public string SeatNumber { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public decimal SeatPrice { get; set; }

    public decimal RevenueAmount { get; set; }

    public string PaymentStatus { get; set; } = string.Empty;
}

/// <summary>
/// Representa la ocupación general de la sala para una función determinada.
/// </summary>
public class FunctionOccupancyReportDto
{
    public int FunctionId { get; set; }

    public string FunctionName { get; set; } = string.Empty;

    public string DateFunction { get; set; } = string.Empty;

    public string TimeFunction { get; set; } = string.Empty;

    public string RoomName { get; set; } = string.Empty;

    public int SoldSeats { get; set; }

    public int PendingSeats { get; set; }

    public int TotalSeats { get; set; }

    public decimal OccupancyPercentage { get; set; }

    public List<SectionOccupancyReportDto> Sections { get; set; } = [];
}

/// <summary>
/// Representa la ocupación de una sección o tipo de asiento dentro de una función.
/// </summary>
public class SectionOccupancyReportDto
{
    public string Section { get; set; } = string.Empty;

    public int SoldSeats { get; set; }

    public int PendingSeats { get; set; }

    public int TotalSeats { get; set; }

    public decimal OccupancyPercentage { get; set; }
}