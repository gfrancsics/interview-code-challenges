using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface ICatalogueRepository
    {
        public List<BookStock> GetCatalogue();

        public List<BorrowerData> OnLoan();

        public ApiResponse<BookStock> OnLoan(CatalogueSearch search, string borrowerName);

        public List<BookStock> SearchCatalogue(CatalogueSearch search);
    }
}
