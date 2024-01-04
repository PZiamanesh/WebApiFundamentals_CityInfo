using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    [Authorize]
    public class CitiesController : ControllerBase
    {
        const int _maxPageSize = 20;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> 
            GetCities(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > _maxPageSize)
            {
                pageSize = _maxPageSize;
            }

            var (cities, metaData) = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);
            Response.Headers["X-Pagination"] = JsonConvert.SerializeObject(metaData);
            var result = _mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cities);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CityDto>> GetCity(int id, bool includePointsOfIntrest = false)
        {
            //find city
            var cityToReturn = await _cityInfoRepository.GetCityAsync(id, includePointsOfIntrest);

            if (cityToReturn == null)
            {
                return NotFound();
            }

            if (includePointsOfIntrest)
            {
                return Ok(_mapper.Map<CityDto>(cityToReturn));
            }

            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(cityToReturn));
        }
    }
}
