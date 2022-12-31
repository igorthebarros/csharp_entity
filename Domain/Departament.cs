namespace EntityFC.Domain
{
    public class Departament
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<Employee> Employees { get; set; }
    }
}