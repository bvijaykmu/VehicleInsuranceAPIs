﻿using System;
using System.Collections.Generic;

namespace MetadataService.DTO
{
    public class CustomerDto       
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailId { get; set; }
        public int PhoneNumber { get; set; }
        public string Address { get; set; }
        public int ZipCode { get; set; }
        public IEnumerable<VehicleDto> Vehicles { get; set; }
    }
}
