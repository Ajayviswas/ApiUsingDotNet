using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ChubbOne.Models;
using System.Net.Cache;
using System.Drawing;

namespace ChubbOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        static List<Product> donorList = new List<Product>
        {
             new Product{Id=1,DonorName = "Ajay Viswas",Age = 22,BloodType="O+",ContactInfo="123123",Quantity=500,CollectionDate=DateTime.Now,ExpirationDate = (DateTime.Now).AddDays(33),Status = "Available"},
             new Product{Id=2,DonorName = "Zing",Age = 22,BloodType="O-",ContactInfo="123123",Quantity=500,CollectionDate=DateTime.Now,ExpirationDate = (DateTime.Now).AddDays(32),Status = "Available"},
             new Product{Id=3,DonorName = "Dechen",Age = 22,BloodType="B+",ContactInfo="123123",Quantity=500,CollectionDate=DateTime.Now,ExpirationDate = (DateTime.Now).AddDays(31),Status = "Available"},
        };

        //Get all data 
        [HttpGet("get all")]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            if (!donorList.Any()) { return BadRequest("The list is empty"); }
            return donorList;
        }

        //create data  
        [HttpPost]
        public ActionResult<IEnumerable<Product>> Add(Product d)
        {
            if (string.IsNullOrEmpty(d.DonorName))
            {
                return BadRequest("Donor name is required.");
            }

            if (d.Age <= 18 )
            {
                return BadRequest("Invalid age. Age should be between 18 and 75.");
            }

            if (string.IsNullOrEmpty(d.BloodType) ||
                    !new HashSet<string> { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }.Contains(d.BloodType.ToUpper()))
            {
                return BadRequest("Invalid blood type. Please provide a valid blood type (e.g., A+, O-, B+).");
            }

            if (string.IsNullOrEmpty(d.ContactInfo) || !d.ContactInfo.All(char.IsDigit) || d.ContactInfo.Length != 10)
            {
                return BadRequest("Contact info must be a valid 10-digit phone number.");
            }

            if (d.Quantity < 200)
            {
                return BadRequest("Please enter more than 200ml.");
            }

            d.ExpirationDate = d.CollectionDate.AddDays(42);
            var validStatuses = new HashSet<string> { "available", "requested", "expired" };
            if (string.IsNullOrEmpty(d.Status) || !validStatuses.Contains(d.Status.ToLower()))
            {
                return BadRequest("Invalid status. Please provide a valid status (Available, Requested, or Expired).");
            }
            if ((d.ExpirationDate - d.CollectionDate).Days > 42)
            {
                d.Status = "Expired";
            }

            d.Id = donorList.Any() ? donorList.Max(x => x.Id) + 1 : 1;

            donorList.Add(d);

            return CreatedAtAction(nameof(GetAll), donorList);
        }


        
        


        //get donor by id 
        [HttpGet("{id}")]
        public ActionResult<Product> GetById(int id)
        {
            var res = donorList.Find(x => x.Id == id);
            if (res == null) { return BadRequest("Not Found"); }
            return res;
        }


        //update 
        [HttpPut("{id}")]
        public ActionResult Update(int id, Product d)
        {
            var res = donorList.Find(x => x.Id == id);
            if (res == null) { return NotFound(); }
            var validStatuses = new HashSet<string> { "available", "requested", "expired" };
            if (string.IsNullOrEmpty(d.Status) || !validStatuses.Contains(d.Status.ToLower()))
            {
                return BadRequest("Not Available");
            }
            res.DonorName = d.DonorName;
            res.Age = d.Age;
            res.BloodType = d.BloodType;
            res.ContactInfo = d.ContactInfo;
            res.Quantity = d.Quantity;
            res.CollectionDate = d.CollectionDate;
            res.ExpirationDate = d.ExpirationDate;
            res.Status = d.Status;

            return CreatedAtAction(nameof(GetAll), donorList);
        }



        //Delete
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var res = donorList.Find(x => x.Id == id);
            if (res == null) { return NotFound(); }
            donorList.Remove(res);
            return CreatedAtAction(nameof(GetAll), donorList);
        }


        //pagination 
        [HttpGet("page")]
        public ActionResult<IEnumerable<Product>> GetPage(int page = 1, int size = 10)
        {
            var res = donorList.Skip((page - 1) * size).Take(size).ToList();
            return res;
        }


        //Search
        [HttpGet("blood")]
        public ActionResult<IEnumerable<Product>> BloodSearch(string blood)
        {
            var res = donorList.FindAll(x => x.BloodType.ToUpper() == blood.ToUpper());
            if (res == null) { return NotFound(); };
            return res;
        }

        //Search the status
        [HttpGet("status")]
        public ActionResult<IEnumerable<Product>> StatusSearch(string status)
        {
            var validStatuses = new HashSet<string> { "available", "requested", "expired" };
            if (string.IsNullOrEmpty(status) || !validStatuses.Contains(status.ToLower()))
            {
                return BadRequest("Invalid status.");
            }
            var res = donorList.FindAll(x => x.Status.ToUpper() == status.ToUpper());
            if (res == null) { return NotFound(); }
            return res;
        }

        //search the donor name
        [HttpGet("name")]
        public ActionResult<IEnumerable<Product>> NameSearch(string name)
        {
            var res = donorList.FindAll(x => x.DonorName.ToUpper().Contains(name.ToUpper()));
            if (res == null) { return NotFound(); };
            return res;
        }


    }
}
