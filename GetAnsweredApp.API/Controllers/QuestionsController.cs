using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetAnsweredApp.API.Data.Interface;
using GetAnsweredApp.API.Data.Models;
using GetAnsweredApp.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GetAnsweredApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        private readonly IHubContext<QuestionsHub> _questionsHub;
        

        public QuestionsController(IDataRepository dataRepository, IHubContext<QuestionsHub> questionsHub)
        {
            _dataRepository = dataRepository;
            _questionsHub = questionsHub;
        }

        [HttpGet]
        public IEnumerable<QuestionGetManyResponse> GetQuestions(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return _dataRepository.GetQuestions();
            }

            return _dataRepository.GetQuestionsBySearch(search);
        }

        [HttpGet("unanswered")]
        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            return _dataRepository.GetUnansweredQuestions();
        }

        [HttpGet("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }

            return question;
        }

        [HttpPost]
        public ActionResult<QuestionGetSingleResponse> PostQuestion([FromBody] QuestionPostRequest questionPost)
        {
            var savedQuestion = _dataRepository.PostQuestion(new QuestionPostFullRequest
            {
                Title = questionPost.Title,
                Content = questionPost.Content,
                UserId = "1",
                UserName = "bob.test@test.com",
                Created = DateTime.UtcNow
            });

            return CreatedAtAction(nameof(GetQuestion),
                new {questionId = savedQuestion.QuestionId},
                savedQuestion);
        }

        [HttpPut("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> PutQuestion(int questionId, [FromBody] QuestionPutRequest questionPut)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }

            questionPut.Title = string.IsNullOrEmpty(questionPut.Title) ? question.Title : questionPut.Title;
            questionPut.Content = string.IsNullOrEmpty(questionPut.Content) ? question.Content : questionPut.Content;

            var savedQuestion = _dataRepository.PutQuestion(questionId, questionPut);
            return savedQuestion;
        }

        [HttpDelete("{questionId}")]
        public ActionResult DeleteQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);

            if (question == null)
            {
                return NotFound();
            }
            _dataRepository.DeleteQuestion(questionId);
            return NoContent();
        }

        [HttpPost("answer")]
        public ActionResult<AnswerGetResponse> PostAnswer(AnswerPostRequest answerPostRequest)
        {
            var questionExists = _dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);
            if (!questionExists)
            {
                return NotFound();
            }
            
            var savedAnswer = _dataRepository.PostAnswer(new AnswerPostFullRequest
                {
                    QuestionId = answerPostRequest.QuestionId.Value,
                    Content = answerPostRequest.Content,
                    UserId = "1",
                    UserName = "bob.test@test.com",
                    Created = DateTime.UtcNow
                }
            );

            _questionsHub.Clients.Group(
                    $"Question-{answerPostRequest.QuestionId.Value}")
                .SendAsync(
                    "ReceiveQuestion",
                    _dataRepository.GetQuestion(
                answerPostRequest.QuestionId.Value));

            return savedAnswer;
        }
    }
}
