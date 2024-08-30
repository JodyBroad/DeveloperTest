﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UKParliament.CodeTest.Data;
using UKParliament.CodeTest.Data.Entities;
using UKParliament.CodeTest.Data.DTO;

namespace UKParliament.CodeTest.Services
{
    public class TodoListService : ITodoListService
    {
        private readonly ITodoListRepository _repository;
        private readonly ILogger<TodoListService> _logger;
        private readonly IMapper _mapper;

        public TodoListService(ITodoListRepository repository, ILogger<TodoListService> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TodoItem>> GetListAsync()
        {
            // use EF core method to fetch all the ToDo items from the db
            var todoResult = await _repository.GetList();
            if (todoResult == null)
            {
                // if none found throw a meaningful error
                _logger.LogError("No To Do Items found");
                throw new Exception("No To Do Items found");
            }
            return todoResult;
        }

        public async Task<TodoItem> GetByIdAsync(int id)
        {
            var todoitem = await _repository.GetById(id);
            return todoitem;
        }

        public async Task AddToDoAsync(CreateTodoRequestDTO request)
        {
            try
            {
                // use automapper to covert the CreateTodoRequest object into a ToDoItem entity
                var todo = _mapper.Map<TodoItem>(request);
                // add the ToDoItem entity to the TodoItems Dbset in our context and save the changes asynchronously
                _repository.Insert(todo);
                await _repository.SaveChangesAsync(todo);
            }
            catch (Exception ex)
            {
                // catch any exceptions, log the error and throw new excpetion with a descriptive error message 
                _logger.LogError(ex, "An error occurred while creating the new To Do List item");
                throw new Exception("An error occurred while creating the new To Do List item");
            }
        }

        // now using httppatch, so only updates the fields that have been changed
        public async Task UpdateToDoItemAsync(int id, UpdateTodoRequestDTO request)
        {
           try
            {
                // retrieve the specific item by id
                var todo = await _repository.GetById(id);
                // if not found throw an error
                if (todo == null)
                {
                    throw new Exception($"To Do item with Id: {id} not found.");
                }

                // update the properties based on the request object
                if (request.Title != null)
                {
                    todo.Title = request.Title;
                }

                if (request.Description != null)
                {
                    todo.Description = request.Description;
                }

                // bool will never be null, so just compare the existing record and the update to see if it is different
                if (request.IsComplete != todo.IsComplete)
                {
                    todo.IsComplete = request.IsComplete;
                }

                if (request.DueDate != null)
                {
                    todo.DueDate = (DateTime)request.DueDate;
                }
                // use automapper to covert the CreateTodoRequest object into a ToDoItem entity
                var mappedToDo = _mapper.Map<TodoItem>(request);
                // save the changes using the mapped entity
                await _repository.SaveChangesAsync(mappedToDo);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the To Do item with Id: {id}");
                throw;
            }
        }

        // this is just a type of update, only we are just changing the completed status, so use a seperate DTO
        public async Task CompleteToDoItemAsync(int id, CompleteTodoRequestDTO request)
        {
            try
            {
                // retrieve the specific item by id
                var todo = await _repository.GetById(id);
                // if not found throw an error
                if (todo == null)
                {
                    _logger.LogError($"To Do item wiih Id: {id} not found.");
                    throw new Exception($"To Do item wiih Id: {id} not found.");
                }

                // update the properties based on the request object
                // compare the existing record and the update to see if it is different
                if (request.IsComplete != todo.IsComplete)
                {
                    todo.IsComplete = request.IsComplete;
                }
                else
                {
                    _logger.LogError($"To Do item with Id: {id} is already marked as complete!");
                    throw new Exception($"To Do item with Id: {id} is already marked as complete!");
                }

                  // use automapper to covert the CreateTodoRequest object into a ToDoItem entity
                var mappedToDo = _mapper.Map<TodoItem>(request);
                // save the changes using the mapped entity
                await _repository.SaveChangesAsync(mappedToDo);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while trying to complete the To Do item with Id: {id}, {ex.Message}");
                throw;
            }
        }

        public async Task DeleteToDoItemAsync(int id)
        {
            try
            {

                // retrieve the specific item by id
                var todo = await _repository.GetById(id);
                // if not found throw an error
                if (todo == null)
                {
                    _logger.LogError($"To Do item wiih Id: {id} not found.");
                    throw new Exception($"To Do item wiih Id: {id} not found.");
                }

                // if item has been found, delete it
                await _repository.Delete(id);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the To Do item with Id: {id}");
                throw;
            }
                
        }
    }
}
