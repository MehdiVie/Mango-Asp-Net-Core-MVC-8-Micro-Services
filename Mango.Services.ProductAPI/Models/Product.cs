﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ProductAPI.Models
{
    public class Product
    {
        [Required] 
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }

        [Range(1, 1000, MinimumIsExclusive = true, MaximumIsExclusive = false)]
        public double Price { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
    }
}