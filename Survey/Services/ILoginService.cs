﻿using Survey.Models;

namespace Survey.Services
{
    public interface ILoginService
    {
        Task<bool> Register(string email, string password, string role = "User");
        Task<string?> Login(string email, string password);
        Task<User?> GetUser(string email);
    }
}
