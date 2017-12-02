using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using NoteKeeper.DataLayer.Exceptions;
using NoteKeeper.Model;
using System.Net.Http;
using System.Net;
using NoteKeeper.Logger;

namespace NoteKeeper.Api.Filters
{
    /// <summary>
    /// Handle exceptions
    /// </summary>
    public class HandleExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var ex = context.Exception;

            if (ex is CreateException<User>)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);

                Log.Instance.Error("Невозможно создать пользователя: {0}", ex.Message);
            }
            else if (ex is CreateException<Note>)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);

                Log.Instance.Error("Невозможно создать заметку: {0}", ex.Message);
            }
            else if (ex is CreateException<Tag>)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);

                Log.Instance.Error("Невозможно создать тег: {0}", ex.Message);
            }
            else if (ex is ChangeException<string>)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);

                var changeEx = ex as ChangeException<string>;
                Log.Instance.Error("Невозможно изменить {0}, Id = {1}: {2}", changeEx.TypeName, changeEx.Id, changeEx.Message);
            }
            else if (ex is CreateRelationException)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);

                var createEx = ex as CreateRelationException;
                Log.Instance.Error("Невозможно добавить связь {0}, FirstName = {1}, SecondName = {2}, FirstId = {3}, SecondId = {4}: {5}", createEx.RelationName, createEx.FirstItemTypeName, createEx.FirstItemTypeName, createEx.FirstItemId, createEx.SecondItemId, createEx.Message);
            }
            else if(ex is GetException)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);

                var getEx = ex as GetException;
                Log.Instance.Error("Невозможно получить {0}, потому что {1}", getEx.ItemName, getEx.Message);
            }
            else if(ex is ArgumentNullException)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);

                Log.Instance.Error("Параметр запроса не указан: {0}", ex.Message);
            }
            else
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

                Log.Instance.Error("Исключение: {0}, сообщение: {1}, стек: {2}", ex.GetType().ToString(), ex.Message, ex.StackTrace);
            }

        }
    }
}