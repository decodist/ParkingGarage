using Microsoft.AspNetCore.Mvc;
using DemoAppDotNet.Models;

namespace DemoAppDotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Router : ControllerBase
    {
        // GET: api/<router>
        [HttpGet()]
        public IEnumerable<string> Get()
        {
            return ["API available", "OK"];
        }
        
        // GET api/<router>/5
        // [HttpGet("{id:int}")]
        // public IActionResult GetById(int id)
        // {
        //     return Ok(new GetIdResponse(id));
        // }

        // POST api/<router>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<router>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<router>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
