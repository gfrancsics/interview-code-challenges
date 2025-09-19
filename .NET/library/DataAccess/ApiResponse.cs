namespace OneBeyondApi.DataAccess
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; } // Jelezze, hogy sikeres volt-e
        public string Message { get; set; } // Magyarázó üzenet
        public T Data { get; set; } // Az eredmény-adat
    }
}
