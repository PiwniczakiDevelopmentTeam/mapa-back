using mapa_back.Data;
using mapa_back.Data.DTO;
using mapa_back.Data.RSPOApi;
using mapa_back.Exceptions;
using mapa_back.Mappers;
using mapa_back.Models;
using mapa_back.Models.DTO;
using mapa_back.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace mapa_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolsController : ControllerBase
    {
        private readonly ISchoolsService schoolsService;
		private readonly IServiceProvider serviceProvider;
		private readonly RSPOProgressTracker progressTracker;
        public SchoolsController(DatabaseContext context, ISchoolsService schoolsService, RSPOProgressTracker progressTracker, IServiceProvider serviceProvider)
        {
            this.schoolsService = schoolsService;
			this.progressTracker = progressTracker;
			this.serviceProvider = serviceProvider;
		}

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetDataFromRSPO")]
        public async Task<ActionResult> GetDataFromRSPO([FromServices] IRSPOApiService service)
        {
			_ = Task.Run(async () =>
			{
				using var scope = serviceProvider.CreateScope();
				var scopedService = scope.ServiceProvider.GetRequiredService<IRSPOApiService>();
				await scopedService.SyncDataFromRSPOApi();
			});
			return Ok(new { message = "RSPO background sync started" });
		}

		[HttpGet("GetRSPOBackgroundSyncProgress")]
		public ActionResult GetRSPOBackgroundSyncProgress()
		{
			return Ok(new
			{
				actualPage = progressTracker.CurrentPage,
				isSyncInProgress = progressTracker.IsSyncInProgress,
				invalidRspoNumbers = progressTracker.InvalidRspoNumbers,
				exceptions = progressTracker.Exceptions
			});
		}

		[HttpGet("GetSchoolsCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<long>> GetSchoolsCount()
        {
            try
            {
                long schoolsCount = await schoolsService.GetSchoolsCount();
                if(schoolsCount > 0)
                {
                    return Ok(schoolsCount);
                }
                return NotFound();
            }
            catch (DatabaseException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get schools count from database. Try again later");
            }
        }

		[HttpPost("GetSingleSchool")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]

		public async Task<ActionResult<SchoolDTO>> GetSingleSchool(int rspoId)
		{
			try
			{
				SchoolDTO singleSchool = await schoolsService.GetSingleSchool(rspoId);
				return singleSchool == null ? NotFound() : Ok(singleSchool);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get single school from database. Try again later");
			}
		}

		[HttpPost("GetSingleSchoolFromRSPO")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]

		public async Task<ActionResult<SchoolDTO>> GetSingleSchoolFromRSPO(int rspoId)
		{
			try
			{
				SchoolDTO singleSchool = await schoolsService.GetSingleSchoolFromRSPO(rspoId);
				return singleSchool == null ? NotFound() : Ok(singleSchool);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get single school from database. Try again later");
			}
		}

		[HttpPost("GetSchoolPage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<PagedResult<School>>> GetSchoolPage(int size, int pageNumber, List<FilterParams>? filters = null)
        {
            try
            {
				PagedResult<SchoolActual> schoolsPage = await schoolsService.GetSchoolsPage(size, pageNumber, filters);
                if (schoolsPage.Items.Count > 0)
                {
                    return Ok(schoolsPage);
                }
                return NotFound();
            }
            catch(SchoolServiceException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get schools count from database. Try again later");
            }
        }

        [HttpDelete("DeleteSchool")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> DeleteSingleSchool(int rspoId)
        {
            try
            {
                if (rspoId <= 0)
                {
                    return BadRequest("Invalid school id");
                }
                await schoolsService.DeleteSingleSchool(rspoId);
                return Ok($"school with id: {rspoId} deleted");           
            }
            catch(DatabaseException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to delete single school. Try again later");
            }

        }

        [HttpDelete("DeleteManySchools")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> DeleteManySchools(List<int> rspoIds)
        {
            try
            {
                if (rspoIds.Count <= 0)
                {
                    return BadRequest("ID list empty");
                }
                var invalidIds = rspoIds.Where(id => id <= 0).ToList();
                if (invalidIds.Any())
                {
                    return BadRequest($"The following IDs are invalid: {string.Join(", ", invalidIds)}");
                }
                await schoolsService.DeleteManySchools(rspoIds);
                return Ok($"school with given ids deleted");
            }
            catch (DatabaseException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to delete single school. Try again later");
            }
        }
        [HttpGet("GetChanges")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ChangedSchoolsResponse>> GetChanges(int size, int page)
        {
            try
            {
				ChangedSchoolsResponse response = await schoolsService.GetChangedSchoolsList(size, page);
                if (response.ChangedSchools.Any() || response.NewSchools.Any() || response.NotExistingSchools.Any())
                {
                    return Ok(response);
                }
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (SchoolServiceException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (DatabaseException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
            }
        }

		[HttpGet("GetSingleSchoolWithChanges")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ChangedSchool>> GetSingleSchoolWithChanges(int rspoId)
		{
			try
			{
				ChangedSchool response = await schoolsService.GetSingleChangedSchool(rspoId);
				if (response == null)
				{
					return NotFound();
				}
				return Ok(response);
			}
			catch (ArgumentException ex)
			{
				return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}

		[HttpPost("AddSingleSchool")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<bool>> AddSingleSchool(SchoolDTO schoolDTO)
		{
			try
			{
                bool response = await schoolsService.PostSingleSchool(SchoolMapper.MapToActualSchool(schoolDTO));
				if (response == false)
				{
					return StatusCode(500, "Unexpected error occurred");
				}
				return Ok(response);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}


		[HttpPost("AddManySchools")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<bool>> AddManySchools(List<SchoolDTO> schoolsDTO)
		{
			try
			{
                List<SchoolActual> schools = schoolsDTO.Select(x => SchoolMapper.MapToActualSchool(x)).ToList();
				bool response = await schoolsService.PostManySchools(schools);
				if (response == false)
				{
					return StatusCode(500, "Unexpected error occurred");
				}
				return Ok(response);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}

		[HttpPut("UpdateSingleSchool")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<bool>> UpdateSingleSchool(SchoolDTO schoolDTO)
		{
			try
			{
				bool response = await schoolsService.UpdateSingleSchool(SchoolMapper.MapToActualSchool(schoolDTO));
				if (response == false)
				{
					return StatusCode(500, "Unexpected error occurred");
				}
				return Ok(response);
			}
			catch (ArgumentException ex)
			{
				return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}

		[HttpPut("UpdateManySchools")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<bool>> UpdateManySchools(List<SchoolDTO> schoolsDTO)
		{
			try
			{
				List<SchoolActual> schools = schoolsDTO.Select(x => SchoolMapper.MapToActualSchool(x)).ToList();
				bool response = await schoolsService.UpdateManySchools(schools);
				if (response == false)
				{
					return StatusCode(500,"Unexpected error occurred");
				}
				return Ok(response);
			}
			catch (ArgumentException ex)
			{
				return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}

		[HttpPut("SyncRspoToActual")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<bool>> SyncRspoToActual()
		{
			try
			{
				bool response = await schoolsService.SyncRspoToActual();
				if (response == false)
				{
					return StatusCode(500, "Unexpected error occurred");
				}
				return Ok(response);
			}
			catch (ArgumentException ex)
			{
				return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}

		[HttpGet("GetMissingSchoolsInRSPOTable")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<List<SchoolDTO>>> GetMissingSchoolsInRSPOTable(int size, int page)
		{
			try
			{
                List<SchoolDTO> response = await schoolsService.GetMissingSchoolsInRSPOTable(size, page);
				if (response.Count <= 0)
				{
					return NoContent();
				}
				return Ok(response);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}

		[HttpGet("GetMissingSchoolsInRSPOTableCount")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<int>> GetMissingSchoolsInRSPOTableCount()
		{
			try
			{
				int response = await schoolsService.GetMissingSchoolsInRSPOTableCount();
				return Ok(response);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}

		[HttpGet("GetMissingSchoolsInSchoolsTable")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<List<SchoolDTO>>> GetMissingSchoolsInSchoolsTable(int size, int page)
		{
			try
			{
				List<SchoolDTO> response = await schoolsService.GetMissingSchoolsInSchoolsTable(size, page);
				if (response.Count <= 0)
				{
					return NoContent();
				}
				return Ok(response);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}

		[HttpGet("GetMissingSchoolsInSchoolsTableCount")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<int>> GetMissingSchoolsInSchoolsTableCount()
		{
			try
			{
				int response = await schoolsService.GetMissingSchoolsInSchoolsTableCount();
				return Ok(response);
			}
			catch (SchoolServiceException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while trying to get changes. Try again later");
			}
		}

		//AWARYJNE COPY SCHOOLS PRZED UZYCIEM DROP WSZYSTKICH RZECZY W PRIVATE SCHOOLS
		[HttpGet("CopySchools")]
        public async Task<ActionResult<bool>> CopySchools()
        {
            await schoolsService.AddSchoolsFromRSPOTableToMapSchoolTable();
            return Ok();
        }
		
	}
}
