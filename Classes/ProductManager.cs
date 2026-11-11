public class ProductManager
{
    private int _nextId = 0;

    public List<Product> Products {get;set;} = new();

    public int GenerateId()
    {
        return Interlocked.Increment(ref _nextId);
    }

}