using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class CatalogueRepository : ICatalogueRepository
    {
        public CatalogueRepository()
        {
        }
        public List<BookStock> GetCatalogue()
        {
            using (var context = new LibraryContext())
            {
                var list = context.Catalogue
                    .Include(x => x.Book)
                    .ThenInclude(x => x.Author)
                    .Include(x => x.OnLoanTo)
                    .ToList();
                return list;
            }
        }

        public List<BorrowerData> OnLoan()
        {
            using (var context = new LibraryContext())
            {
                var list = context.Catalogue
                    .Include(x => x.Book)
                    .Include(x => x.OnLoanTo)
                    .AsQueryable();

                return list.Where(x => x.OnLoanTo != null).Select(x => new BorrowerData() { BorrowerName = x.OnLoanTo.Name, BookTitle = x.Book.Name }).ToList(); 
            }
        }

        public BorrowerData OnLoan(CatalogueSearch search)
        {
            using (var context = new LibraryContext())
            {
                var list = context.Catalogue
                    .Include(x => x.Book)
                    .ThenInclude(x => x.Author)
                    .Include(x => x.OnLoanTo)
                    .AsQueryable();
                
                if (search != null)
                {
                    if (!string.IsNullOrEmpty(search.Author))
                    {
                        list = list.Where(x => x.Book.Author.Name.Contains(search.Author));
                    }
                    if (!string.IsNullOrEmpty(search.BookName))
                    {
                        list = list.Where(x => x.Book.Name.Contains(search.BookName));
                    }
                }
                //searching for active borrowing record
                BookStock activeBorrower = list.FirstOrDefault(x => x.OnLoanTo != null);
                if(activeBorrower != null)
                {
                    //save borrower data for reporting
                    BorrowerData borrowerData = new BorrowerData() { BorrowerName = activeBorrower.OnLoanTo.Name, BookTitle = activeBorrower.Book.Name };
                    //expired
                    if(DateTime.Now > activeBorrower.LoanEndDate)
                    {
                        //issue a fine
                        activeBorrower.OnLoanTo.Fine += 100;
                        //make the book available
                        activeBorrower.OnLoanTo = null;
                        activeBorrower.LoanEndDate = null;
                        context.SaveChanges();
                    }
                    return borrowerData;
                }
                else
                {
                    return null;
                }
            }
        }

        public BookStock OnLoanWithReservation(CatalogueSearch search, string borrowerName)
        {
            using (var context = new LibraryContext())
            {
                var bookStock = context.Catalogue
                    .Include(bs => bs.Book)
                    .ThenInclude(x => x.Author)
                    .Include(bs => bs.OnLoanTo)
                    .FirstOrDefault(bs => bs.Book.Name == search.BookName && bs.Book.Author.Name == search.Author);

                if (bookStock == null)
                {
                    Console.WriteLine("Book not found.");
                    return null;
                }

                Console.WriteLine($"Book found: {bookStock.Book.Name} by {bookStock.Book.Author.Name}");

                if (bookStock.OnLoanTo != null)
                {
                    Console.WriteLine($"Book is currently on loan to: {bookStock.OnLoanTo.Name}");

                    if (borrowerName == null)
                    {
                        Console.WriteLine("Please give the Borrower name.");
                        return null;
                    }

                    Borrower borrower = context.Borrowers.FirstOrDefault(b => b.Name == borrowerName);
                    if (borrower == null)
                    {
                        Console.WriteLine("Please register before the loaning.");
                        return null;
                    }

                    if (!bookStock.Waitlist.Any(w => w.Name == borrowerName))
                    {
                        bookStock.Waitlist.Add(borrower);
                        context.SaveChanges();
                        Console.WriteLine($"{borrowerName} added to the waitlist.");
                    }
                    else
                    {
                        Console.WriteLine($"{borrowerName} is already on the waitlist.");
                    }
                }
                else
                {
                    Console.WriteLine("Book is available for borrowing.");
                }

                return bookStock;
            }
        }

        public List<BookStock> SearchCatalogue(CatalogueSearch search)
        {
            using (var context = new LibraryContext())
            {
                var list = context.Catalogue
                    .Include(x => x.Book)
                    .ThenInclude(x => x.Author)
                    .Include(x => x.OnLoanTo)
                    .AsQueryable();

                if (search != null)
                {
                    if (!string.IsNullOrEmpty(search.Author)) {
                        list = list.Where(x => x.Book.Author.Name.Contains(search.Author));
                    }
                    if (!string.IsNullOrEmpty(search.BookName)) {
                        list = list.Where(x => x.Book.Name.Contains(search.BookName));
                    }
                }
                    
                return list.ToList();
            }
        }
    }
}
