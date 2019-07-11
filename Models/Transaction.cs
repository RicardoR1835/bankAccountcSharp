using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace bankAccount_cSharp.Models
{
    public class Transaction
    {
       [Key]
        public int TransactionId {get;set;}

        public double CurrentBalance {get;set;}

        [Required]
        public int UserId {get;set;}

        public User AccountOwner {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;

        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }
}