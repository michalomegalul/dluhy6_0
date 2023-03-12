using System.Transactions;

namespace dluhy6_0
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
