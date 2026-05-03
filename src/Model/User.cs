namespace La_Divine.Src.Model;

public class User {
    public int Id{get;set;}
    public string Name{get;set;} = string.Empty;
    public string LastName{get;set;} = string.Empty;
    public string Email{get;set;} = string.Empty;
    public string Rut {get;set;} = string.Empty;
    public string Phone {get;set;} = string.Empty;
    public string Password {get;set;} = string.Empty;
    public bool Status {get;set;}
    
    public Role Role { get; set; } = null!;
    public int RoleId { get; set; }
}