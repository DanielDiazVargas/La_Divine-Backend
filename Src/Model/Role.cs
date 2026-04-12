namespace La_Divine.Src.Model;

public class Role() {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = [];
}