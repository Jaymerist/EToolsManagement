﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SaleSystem.Entities;

public partial class Category
{
    [Key]
    public int CategoryID { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Description { get; set; }

    public bool RemoveFromViewFlag { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}