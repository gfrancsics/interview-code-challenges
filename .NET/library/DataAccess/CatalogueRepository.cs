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

        public ApiResponse<BookStock> OnLoan(CatalogueSearch search, string borrowerName)
        {
            try
            {
                using (var context = new LibraryContext())
                {
                    if (String.IsNullOrEmpty(borrowerName))
                    {
                        return CreateResponse<BookStock>(false, "Borrower name is empty.", null);
                    }

                    var borrower = context.Borrowers.FirstOrDefault(b => b.Name == borrowerName);
                    if (borrower == null)
                    {
                        return CreateResponse<BookStock>(false, "Borrower name is not registered.", null);
                    }

                    var bookStock = context.Catalogue
                        .Include(bs => bs.Book)
                        .ThenInclude(x => x.Author)
                        .Include(bs => bs.OnLoanTo)
                        .FirstOrDefault(bs => bs.Book.Name == search.BookName && bs.Book.Author.Name == search.Author);

                    if (bookStock == null)
                    {
                        return CreateResponse<BookStock>(false, "Book not found.", null);
                    }
                    //loaned
                    if (bookStock.OnLoanTo != null)
                    {
                        //borrower match
                        if(bookStock.OnLoanTo.Name == borrower.Name)
                        {
                            //expired
                            if (DateTime.Now > bookStock.LoanEndDate)
                            {
                                //issue a fine
                                borrower.Fine = (borrower.Fine ?? 0) + 100;
                                //make the book available
                                bookStock.OnLoanTo = null;
                                bookStock.LoanEndDate = null;
                                context.SaveChanges();
                                return CreateResponse<BookStock>(true, $"Loaning closed with penalty at borrower:{borrower.Name}.", bookStock);
                            }
                            //not expired
                            else
                            {
                                bookStock.OnLoanTo = null;
                                bookStock.LoanEndDate = null;
                                context.SaveChanges();
                                return CreateResponse<BookStock>(true, $"Loaning closed at borrower:{borrower.Name}.", bookStock);
                            }
                        }
                        else if (!bookStock.Waitlist.Any(w => w.Name == borrowerName))
                        {
                            bookStock.Waitlist.Add(borrower);
                            context.SaveChanges();
                            return CreateResponse<BookStock>(true, $"Borrower {borrower.Name} added to the waitlist.", bookStock);
                        }
                        else
                        {
                            return CreateResponse<BookStock>(false, "Borrower is already on the waitlist.", null);
                        }
                    }
                    else
                    {
                        return CreateResponse<BookStock>(true, "Book is available for borrowing.", bookStock);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                return CreateResponse<BookStock>(false, $"Exception: {ex.Message}", null); 
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

        private ApiResponse<T> CreateResponse<T>(bool isSuccess, string message, T data)
        {
            return new ApiResponse<T>
            {
                IsSuccess = isSuccess,
                Message = message,
                Data = data
            };
        }
    }
}
