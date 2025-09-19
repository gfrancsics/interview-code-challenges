using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class BorrowerRepository : IBorrowerRepository
    {
        public BorrowerRepository()
        {
        }
        public List<Borrower> GetBorrowers()
        {
            using (var context = new LibraryContext())
            {
                var list = context.Borrowers
                    .ToList();
                return list;
            }
        }

        public Guid AddBorrower(Borrower borrower)
        {
            using (var context = new LibraryContext())
            {
                context.Borrowers.Add(borrower);
                context.SaveChanges();
                return borrower.Id;
            }
        }

        public void IssueFine(Borrower borrower)
        {
            using (var context = new LibraryContext())
            {
                if(borrower != null)
                {
                    Borrower borrower1 = context.Borrowers.First(x => x.Id == borrower.Id);
                    borrower1.Fine = 100;
                }
            }
        }
    }
}
