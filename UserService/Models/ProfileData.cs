﻿namespace UserService.Models
{
    public partial class ProfileData
    {
        public int? Id { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
    }
}