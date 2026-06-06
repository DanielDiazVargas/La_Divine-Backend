namespace La_Divine.Src.Model;

/// <summary>
/// Entidad que representa una obra o evento en la cartelera del teatro.
/// Contiene toda la información pública de la función y su identificador.
/// </summary>
public class Function
{
    public int Id {get;set;}
    public string Name {get;set;} = null!;
    public string Description {get;set;} = null!;
    
    // Fecha programada para la obra
    public string DateFunction {get;set;} = null!;
    
    // Hora de inicio de la obra
    public string TimeFunction {get;set;} = null!;
    
    // Estado de publicación de la función (activa/inactiva)
    public bool State {get;set;}
    
    // URL segura de la imagen del afiche alojada en Cloudinary
    public string? Image {get;set;}
    
    // UUID único generado para la validación interna de la entrada o función
    public string ValidateFunction {get;set;} = string.Empty;

}