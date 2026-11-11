public record Product
{
    public int Id {get;set;} 
    public string Name {get;set;} = string.Empty;
    public string Description {get;set;} = string.Empty;
    public decimal Price {get;set;}
    public string ImagePath {get;set;} = string.Empty;
}