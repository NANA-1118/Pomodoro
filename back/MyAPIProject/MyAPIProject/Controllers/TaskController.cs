using Azure.Core;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MyAPIProject.Models;

namespace MyAPIProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly string _connectString;

        public TaskController(IConfiguration configuration)
        {
            _connectString = configuration.GetConnectionString("PomoDb")
                             ?? throw new ArgumentNullException(nameof(configuration), "PomoDb connection string not found.");
        }

        // GET task/unfinished
        [HttpGet("unfinished")]
        public async Task<IActionResult> GetAllUnfinished()
        {
            try
            {
                await using var conn = new SqlConnection(_connectString);
                await conn.OpenAsync();

                string sql = "SELECT * FROM Task WHERE IsDone = 0";
                var tasks = await conn.QueryAsync<TaskItem>(sql);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得未完成任務失敗", detail = ex.Message });
            }
        }

        // GET task/unfinished/1
        [HttpGet("unfinished/{memberId}")]
        public async Task<IActionResult> GetUnfinishedByMember(int memberId)
        {
            try
            {
                await using var conn = new SqlConnection(_connectString);
                await conn.OpenAsync();

                string sql = "SELECT * FROM Task WHERE IsDone = 0 AND MemberId = @MemberId";
                var tasks = await conn.QueryAsync<TaskItem>(sql, new { MemberId = memberId });
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得會員未完成任務失敗", detail = ex.Message });
            }
        }

        // GET task/finished/1
        [HttpGet("finished/{memberId}")]
        public async Task<IActionResult> GetFinishedByMember(int memberId)
        {
            try
            {
                await using var conn = new SqlConnection(_connectString);
                await conn.OpenAsync();

                string sql = "SELECT * FROM Task WHERE IsDone = 1 AND MemberId = @MemberId";
                var tasks = await conn.QueryAsync<TaskItem>(sql, new { MemberId = memberId });
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"取得會員完成任務失敗: {ex.Message}" });
            }
        }

        // POST task/addtime/1
        [HttpPost("addtime/{taskId}")]
        public async Task<IActionResult> AddTime(int taskId, [FromBody] int seconds)
        {
            try
            {
                await using var conn = new SqlConnection(_connectString);
                await conn.OpenAsync();

                string sql = @"UPDATE Task
                               SET TotalSeconds = TotalSeconds + @Seconds,
                                   UpdateTime = @UpdateTime
                               WHERE TaskId = @TaskId";

                int rows = await conn.ExecuteAsync(sql, new { Seconds = seconds, TaskId = taskId, UpdateTime = DateTime.Now });

                if (rows > 0)
                    return Ok(new { message = "更新任務時間成功" });
                else
                    return NotFound(new { message = "找不到對應的任務" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新任務時間失敗", detail = ex.Message });
            }
        }
        // POST task/addtask/1
        [HttpPost("addtask/{memberId}")]
        public async Task<IActionResult> AddTask(int memberId, [FromBody] string newTaskName)
        {
            try
            {
                await using var conn = new SqlConnection(_connectString);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Task (MemberId, TaskName, IsDone, TotalSeconds, UpdateTime, CreateTime)
                               VALUES (@MemberId, @TaskName, 0, 0, @UpdateTime, @UpdateTime);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int newTaskId = await conn.QuerySingleAsync<int>(sql, new
                {
                    MemberId = memberId,
                    TaskName = newTaskName,
                    UpdateTime = DateTime.Now
                });

                return Ok(new { taskId = newTaskId, taskName = newTaskName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"新增任務失敗: {ex.Message}" });
            }
        }

        // POST task/finish
        [HttpPost("finish")]
        public async Task<IActionResult> FinishTasks([FromBody] int[] taskIds)
        {
            if (taskIds == null || taskIds.Length == 0)
                return BadRequest(new { message = "未提供 taskId" });

            try
            {
                await using var conn = new SqlConnection(_connectString);
                await conn.OpenAsync();

                string sql = @"UPDATE Task
                       SET IsDone = 1, UpdateTime = @UpdateTime
                       WHERE TaskId IN @TaskIds";

                int rows = await conn.ExecuteAsync(sql, new { TaskIds = taskIds, UpdateTime = DateTime.Now });

                return Ok(new { message = $"{rows} 個任務標記為完成" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "批次完成任務失敗", detail = ex.Message });
            }
        }




        // POST task/delete
        [HttpPost("delete")]  // 用 POST 批次刪除更方便，傳 body
        public async Task<IActionResult> DeleteTasks([FromBody] int[] taskIds)
        {
            if (taskIds == null || taskIds.Length == 0)
                return BadRequest(new { message = "未提供 taskId" });

            try
            {
                await using var conn = new SqlConnection(_connectString);
                await conn.OpenAsync();

                string sql = @"DELETE FROM Task WHERE TaskId IN @TaskIds";

                int rows = await conn.ExecuteAsync(sql, new { TaskIds = taskIds });

                return Ok(new { message = $"{rows} 個任務已刪除" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "批次刪除任務失敗", detail = ex.Message });
            }
        }








        // GET task/chartdata/1
        [HttpGet("chartdata/{memberId}")]
        public async Task<IActionResult> ChartData(int memberId)
        {
            try
            {
                await using var conn = new SqlConnection(_connectString);
                await conn.OpenAsync();

                string sql = "SELECT * FROM Task WHERE IsDone = 1 AND MemberId = @MemberId";
                var tasks = await conn.QueryAsync<TaskItem>(sql, new { MemberId = memberId });
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"取得會員完成任務失敗: {ex.Message}" });
            }
        }

    }
}
