namespace La_Divine.Src.Dtos.Function;
public class CreateFunction
{
    public string Name {get;set;} = null!;
    public string Description {get;set;} = null!;
    public string DateFunction {get;set;} = null!;
    public string TimeFunction {get;set;} = null!;
    public IFormFile? Image {get;set;} 
}
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

public class CreateFunctionResponse
{
    public string Message {get;set;} = null!;
    public bool Success {get;set;}
    public CreateFunctionData Data {get;set;} = null!;
}