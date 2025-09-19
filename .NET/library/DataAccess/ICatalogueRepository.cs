using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface ICatalogueRepository
    {
        public List<BookStock> GetCatalogue();

        public List<BorrowerData> OnLoan();
        
        public BorrowerData OnLoan(CatalogueSearch search);

        public BookStock OnLoanWithReservation(CatalogueSearch search, string borrowerName);

        public List<BookStock> SearchCatalogue(CatalogueSearch search);
    }
}
