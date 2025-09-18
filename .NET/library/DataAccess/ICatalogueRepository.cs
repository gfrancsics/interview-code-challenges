using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface ICatalogueRepository
    {
        public List<BookStock> GetCatalogue();

        public List<BorrowerList> OnLoan(CatalogueSearch search);

        public List<BookStock> SearchCatalogue(CatalogueSearch search);
    }
}
