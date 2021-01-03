using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommendo_api.Models;

namespace Recommendo_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly RecommendationContext _context; //todo: need a abstraction here

        public RecommendationsController(RecommendationContext context)
        {
            _context = context;
        }

        // GET: api/Recommendations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recommendation>>> GetAsync()
        {
           return await _context.Recommendations.ToListAsync();
        }

        // GET: api/Recommendations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recommendation>> GetAsync(int id)
        {
            var recipe = await _context.Recommendations.FindAsync(id);

            if (recipe == null)
            {
                return NotFound();
            }

            return recipe;
        }
       
        // POST: api/Recommendations
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Recommendation>> PostAsync(Recommendation recommendation)
        {
            _context.Recommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecipe", new { id = recommendation.Id }, recommendation);
        }

        // DELETE: api/Recommendations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Recommendation>> DeleteAsync(int id)
        {
            var recipe = await _context.Recommendations.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            _context.Recommendations.Remove(recipe);
            await _context.SaveChangesAsync();

            return recipe;
        }
    }
}
