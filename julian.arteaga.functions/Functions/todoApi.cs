using julian.arteaga.common.Models;
using julian.arteaga.common.Models.Responses;
using julian.arteaga.functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace julian.arteaga.functions.Functions
{
    public static class todoApi
    {


        [FunctionName(nameof(Createtodo))]
        public static async Task<IActionResult> Createtodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable TodoTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new todo.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            todo todo = JsonConvert.DeserializeObject<todo>(requestBody); //leyendo el cuerpo del mensaje

            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Response
                {
                    IaSuccess = false,
                    message = "The request must have a TaskDescription."
                });

            }

            todoEntity todoEntity = new todoEntity

            {
                Createdtime = DateTime.UtcNow,
                ETag = "*",
                Iscompleted = false,
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription
            };


            TableOperation AddOperation = TableOperation.Insert(todoEntity);
            await TodoTable.ExecuteAsync(AddOperation);

            string message = "New todo stores in table";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IaSuccess = true,
                message = message,
                Result = todoEntity

            });
        }

        [FunctionName(nameof(Updatetodo))]
        public static async Task<IActionResult> Updatetodo(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
           [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable TodoTable,
           string id,
           ILogger log)
        {
            log.LogInformation($"updated fot todo: {id}. received.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            todo Todo = JsonConvert.DeserializeObject<todo>(requestBody); //leyendo el cuerpo del mensaje


            //Validate todo id
            TableOperation findOperation = TableOperation.Retrieve<todoEntity>("TODO", id);
            TableResult findResult = await TodoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IaSuccess = false,
                    message = "todo not found."
                });

            }

            //update todo
            todoEntity TodoEntity = (todoEntity)findResult.Result;
            TodoEntity.Iscompleted = Todo.Iscompleted;

            if (!string.IsNullOrEmpty(Todo.TaskDescription))
            {
                TodoEntity.TaskDescription = Todo.TaskDescription;

            }

            TableOperation AddOperation = TableOperation.Replace(TodoEntity);
            await TodoTable.ExecuteAsync(AddOperation);

            string message = $"todo: {id}, updated in table.";

            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IaSuccess = true,
                message = message,
                Result = TodoEntity

            });
        }

        [FunctionName(nameof(GetAlltodos))]
        public static async Task<IActionResult> GetAlltodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable TodoTable,
            ILogger log)
        {
            log.LogInformation("Get all todo REceived.");

            TableQuery<todoEntity> query = new TableQuery<todoEntity>();
            TableQuerySegment<todoEntity> Todos = await TodoTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all todos";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IaSuccess = true,
                message = message,
                Result = Todos

            });
        }



        [FunctionName(nameof(GetTodoById))]
        public static IActionResult GetTodoById (
                 [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
                 [Table("todo", "TODO", "{id]", Connection = "AzureWebJobsStorage")] todoEntity TodoEntity,
                 string id,
                 ILogger log)
        {
            log.LogInformation($"Get Todo by: {id}, received.");

            if (TodoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IaSuccess = false,
                    message = "todo not found."
                });

                string message = $"todo: {id}, retrieved.";
                log.LogInformation(message);


                return new OkObjectResult(new Response
                {
                    IaSuccess = true,
                    message = message,
                    Result = TodoEntity

                });
            }
        }

    }
}