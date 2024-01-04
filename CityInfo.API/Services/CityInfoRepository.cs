using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<(IEnumerable<City>, PaginationMetadata)>
            GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            var query = _context.Cities as IQueryable<City>;

            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
                query = query.Where(c => c.Name == name).OrderBy(c => c.Name);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                query = query.Where(c => c.Name.Contains(searchQuery)
                    || c.Description != null && c.Description.Contains(searchQuery));
            }

            var collection =  await query
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var paginationMetadata = new PaginationMetadata(collection.Count, pageSize, pageNumber);

            return (collection, paginationMetadata);
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if (includePointsOfInterest)
            {
                return await _context.Cities.Include(c => c.PointsOfInterest)
                    .Where(c => c.Id == cityId).FirstOrDefaultAsync();
            }

            return await _context.Cities
                  .Where(c => c.Id == cityId).FirstOrDefaultAsync();
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(
            int cityId, 
            int pointOfInterestId)
        {
            return await _context.PointsOfInterest
               .Where(p => p.CityId == cityId && p.Id == pointOfInterestId)
               .FirstOrDefaultAsync();
        }

        public async Task<List<PointOfInterest>> GetPointsOfInterestForCityAsync(
            int cityId)
        {
            return await _context.PointsOfInterest
                           .Where(p => p.CityId == cityId).ToListAsync();
        }

        public async Task AddPointOfInterestForCityAsync(int cityId,
            PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            if (city != null)
            {
                city.PointsOfInterest.Add(pointOfInterest);
            }
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointsOfInterest.Remove(pointOfInterest);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }
    }
}
