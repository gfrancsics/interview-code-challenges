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

        public ApiResponse<BookStock> OnLoanWithReservation(CatalogueSearch search, string borrowerName)
        {
            try
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
                        return new ApiResponse<BookStock>
                        {
                            IsSuccess = false,
                            Message = "Book not found.",
                            Data = null
                        };
                    }
                    //ki van adva
                    if (bookStock.OnLoanTo != null)
                    {
                        if (borrowerName == null)
                        {
                            return new ApiResponse<BookStock>
                            {
                                IsSuccess = false,
                                Message = "Borrower name is empty.",
                                Data = null
                            };
                        }

                        Borrower borrower = context.Borrowers.FirstOrDefault(b => b.Name == borrowerName);
                        if (borrower == null)
                        {
                            return new ApiResponse<BookStock>
                            {
                                IsSuccess = false,
                                Message = "Borrower name is not registered.",
                                Data = null
                            };
                        }
                        //borrower match
                        if(bookStock.OnLoanTo.Name == borrower.Name)
                        {
                            //expired
                            if (DateTime.Now > bookStock.LoanEndDate)
                            {
                                //issue a fine
                                borrower.Fine += 100;
                                //context.Entry(borrower.Fine).State = EntityState.Modified;
                                //make the book available
                                bookStock.OnLoanTo = null;
                                bookStock.LoanEndDate = null;
                                context.SaveChanges();
                                return new ApiResponse<BookStock>
                                {
                                    IsSuccess = true,
                                    Message = $"Loaning closed with penalty at borrower:{borrower.Name}.",
                                    Data = bookStock
                                };
                            }
                            //not expired
                            else
                            {
                                bookStock.OnLoanTo = null;
                                bookStock.LoanEndDate = null;
                                context.SaveChanges();
                                return new ApiResponse<BookStock>
                                {
                                    IsSuccess = true,
                                    Message = $"Loaning closed at borrower:{borrower.Name}.",
                                    Data = bookStock
                                };
                            }
                        }
                        else if (!bookStock.Waitlist.Any(w => w.Name == borrowerName))
                        {
                            bookStock.Waitlist.Add(borrower);
                            context.SaveChanges();
                            return new ApiResponse<BookStock>
                            {
                                IsSuccess = true,
                                Message = $"Borrower {borrower.Name} added to the waitlist.",
                                Data = bookStock
                            };
                        }
                        else
                        {
                            return new ApiResponse<BookStock>
                            {
                                IsSuccess = false,
                                Message = "Borrower is already on the waitlist.",
                                Data = null
                            };
                        }
                    }
                    else
                    {
                        return new ApiResponse<BookStock>
                        {
                            IsSuccess = true,
                            Message = "Book is available for borrowing.",
                            Data = bookStock
                        };
                    }
                    
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<BookStock>
                {
                    IsSuccess = false,
                    Message = "Exception:" +ex.Message,
                    Data = null
                }; 
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
