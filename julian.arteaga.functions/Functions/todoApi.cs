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
        private static bool findResult;

        [FunctionName(nameof(Createtodo))]
        public static async Task<IActionResult> Createtodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
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
            await todoTable.ExecuteAsync(AddOperation);

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
           [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
           string id,
           ILogger log)
        {
            log.LogInformation($"updated fot todo: {id}. received.");

          
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            todo todo = JsonConvert.DeserializeObject<todo>(requestBody); //leyendo el cuerpo del mensaje


            //Validate todo id
            TableOperation findOperation = TableOperation.Retrieve<todoEntity>("TODO", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IaSuccess = false,
                    message = "todo not found."
                });

            }

            //update todo
            todoEntity todoEntity = (todoEntity)findResult.Result;
            todoEntity.Iscompleted = todo.Iscompleted;

            if (string.IsNullOrEmpty(todo.TaskDescription))
            {
                todoEntity.TaskDescription = todo.TaskDescription;

            }



            TableOperation AddOperation = TableOperation.Replace(todoEntity);
            await todoTable.ExecuteAsync(AddOperation);

            string message = $"todo: {id}, updated in table.";

            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IaSuccess = true,
                message = message,
                Result = todoEntity

            });
        }


    }
}
