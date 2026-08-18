using Haskoli.Application.Features.Student;
using Haskoli.Domain.Custom;
using Haskoli.Domain.DTO;
using Haskoli.Domain.Parameters;
using Haskoli.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Haskoli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public StudentController(IMediator mediator) => _mediator = mediator;

        // POST: api/Student
        [HttpPost]
        public async Task<ApiResponse<StudentDTO>> CreateStudent([FromBody] CreateStudentDTO student) =>
            await _mediator.Send(student);

        // GET: api/Student
        [HttpGet]
        public async Task<ApiResponse<MetaData<StudentDTO>>> GetStudents([FromQuery] GetAllStudentParameter filter) =>
            await _mediator.Send(new GetAllStudentQuery
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                OrderBy = filter.OrderBy,
                Document = filter.Document,
                LastName = filter.LastName,
                Email = filter.Email,
                Route = Request.Path.Value
            });

        // GET: api/Student/5
        [HttpGet("{id}")]
        public async Task<ApiResponse<StudentDTO>> GetStudent(int id) =>
            await _mediator.Send(new GetStudentQuery(id));

        // PUT: api/Student
        [HttpPut]
        public async Task<ApiResponse<StudentDTO>> UpdateStudent([FromBody] UpdateStudentDTO student) =>
            await _mediator.Send(student);

        // DELETE: api/Student/5
        [HttpDelete("{id}")]
        public async Task<ApiResponse<StudentDTO>> DeleteStudent(int id) =>
            await _mediator.Send(new DeleteStudentDTO { Id = id });
    }
}
