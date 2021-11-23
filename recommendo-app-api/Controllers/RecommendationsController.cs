using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using recommendo_app_api.Models;

namespace recommendo_app_api.Controllers
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recommendation>>> GetAsync(string type)
        {
            //todo: move logic to service layer and include hide filter
            return await _context.Recommendations.Where(x => x.Type == type).ToListAsync();
        }

        // POST: api/Recommendations
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Recommendation>> PostAsync(Recommendation recommendation)
        {
            _context.Recommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            return Ok();
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
