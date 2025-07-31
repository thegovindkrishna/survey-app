using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Services;
using AutoMapper;
using Survey.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Survey.Repositories;
using System.Security.Claims;
using Asp.Versioning;

namespace Survey.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SurveyControllerV2 : ControllerBase
    {
        // Copy and modify 
    }
}