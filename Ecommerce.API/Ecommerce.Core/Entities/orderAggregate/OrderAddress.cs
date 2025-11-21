namespace Ecommerce.Core.Entities.orderAggregate
{
    public class OrderAddress
    {
        public OrderAddress()
        {
        }

        public OrderAddress(string firstName,
            string lastName,
            string country,
            string government,
            string city,
            string street,
            string zipcode)
        {
            FirstName = firstName;
            LastName = lastName;
            Country = country;
            Government = government;
            City = city;
            Street = street;
            Zipcode = zipcode;
        }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Government { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Zipcode { get; set; } = string.Empty;
    }
}