namespace Account.Services.DTO
{
    public class DoctorListFilterDTO
    {
        public string? NameFilter { get; set; }
        public int From { get; set; }
        public int Count { get; set; }
    }
}
