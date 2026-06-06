namespace La_Divine.Src.Dtos.Function;

/// <summary>
/// DTO utilizado para recibir los datos de entrada desde el frontend al crear una función.
/// Soporta la recepción de archivos multimedia a través de IFormFile.
/// </summary>
public class CreateFunction
{
    public string Name {get;set;} = null!;
    public string Description {get;set;} = null!;
    public string DateFunction {get;set;} = null!;
    public string TimeFunction {get;set;} = null!;
    public IFormFile? Image {get;set;} 
    
}

/// <summary>
/// DTO que representa la función de teatro ya procesada y lista para mostrar.
/// Transforma el archivo físico (IFormFile) en una URL segura de acceso (ImageUrl).
/// </summary>
public class CreateFunctionData
{
    public int Id {get;set;}
    public string Name {get;set;} = null!;
    public string Description {get;set;} = null!;
    public string DateFunction {get;set;} = null!;
    public string TimeFunction {get;set;} = null!;
    public bool State {get;set;}
    public string ImageUrl {get;set;} = null!;

}

/// <summary>
/// DTO envoltorio para estandarizar respuestas del servidor hacia el front.
/// Incluye el estado del proceso, un mensaje descriptivo y la carga útil de los datos.
/// </summary>
public class CreateFunctionResponse
{
    public string Message {get;set;} = null!;
    public bool Success {get;set;}
    public CreateFunctionData Data {get;set;} = null!;
}